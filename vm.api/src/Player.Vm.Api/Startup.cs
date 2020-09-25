/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Player.Vm.Api.Infrastructure.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using S3.Player.Api;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Text.Json.Serialization;
using AuthorizationOptions = Player.Vm.Api.Infrastructure.Options.AuthorizationOptions;
using Player.Vm.Api.Infrastructure.Extensions;
using Player.Vm.Api.Data;
using Player.Vm.Api.Infrastructure.Exceptions.Middleware;
using Player.Vm.Api.Features.Vms;
using Player.Vm.Api.Domain.Services;
using Player.Vm.Api.Infrastructure.DbInterceptors;
using Player.Vm.Api.Domain.Vsphere.Options;
using Microsoft.Extensions.Hosting;
using Player.Vm.Api.Domain.Vsphere.Services;
using MediatR;
using System.Reflection;
using Player.Vm.Api.Hubs;
using System.Threading.Tasks;
using Player.Vm.Api.Features.Shared.Behaviors;
using Microsoft.IdentityModel.Logging;

namespace Player.Vm.Api
{
    public class Startup
    {
        private readonly AuthorizationOptions _authOptions = new AuthorizationOptions();
        private readonly ClientOptions _clientOptions = new ClientOptions();
        private readonly IdentityClientOptions _identityClientOptions = new IdentityClientOptions();

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.GetSection("Authorization").Bind(_authOptions);
            Configuration.GetSection("ClientSettings").Bind(_clientOptions);
            Configuration.GetSection("IdentityClient").Bind(_identityClientOptions);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var provider = Configuration["Database:Provider"];
            switch (provider)
            {
                case "InMemory":
                    services.AddDbContextPool<VmContext>((serviceProvider, optionsBuilder) => optionsBuilder
                        .AddInterceptors(serviceProvider.GetRequiredService<EventTransactionInterceptor>())
                        .UseInMemoryDatabase("vm"));
                    break;
                case "Sqlite":
                case "SqlServer":
                case "PostgreSQL":
                    services.AddDbContextPool<VmContext>((serviceProvider, optionsBuilder) => optionsBuilder
                        .AddInterceptors(serviceProvider.GetRequiredService<EventTransactionInterceptor>())
                        .UseConfiguredDatabase(Configuration));
                    break;
            }

            services.AddOptions()
                .Configure<DatabaseOptions>(Configuration.GetSection("Database"))
                .AddScoped(config => config.GetService<IOptionsMonitor<DatabaseOptions>>().CurrentValue);

            IConfiguration isoConfig = Configuration.GetSection("IsoUpload");
            IsoUploadOptions isoOptions = new IsoUploadOptions();
            isoConfig.Bind(isoOptions);

            services.AddOptions()
                .Configure<IsoUploadOptions>(isoConfig)
                .AddScoped(config => config.GetService<IOptionsMonitor<IsoUploadOptions>>().CurrentValue);

            services
                .Configure<ClientOptions>(Configuration.GetSection("ClientSettings"))
                .AddScoped(config => config.GetService<IOptionsMonitor<ClientOptions>>().CurrentValue);

            services
                .Configure<VsphereOptions>(Configuration.GetSection("Vsphere"))
                .AddScoped(config => config.GetService<IOptionsSnapshot<VsphereOptions>>().Value);

            services
                .Configure<RewriteHostOptions>(Configuration.GetSection("RewriteHost"))
                .AddScoped(config => config.GetService<IOptionsSnapshot<RewriteHostOptions>>().Value);

            services
                .Configure<IdentityClientOptions>(Configuration.GetSection("IdentityClient"))
                .AddScoped(config => config.GetService<IOptionsSnapshot<IdentityClientOptions>>().Value);

            services
                .Configure<ConsoleUrlOptions>(Configuration.GetSection("ConsoleUrls"))
                .AddScoped(config => config.GetService<IOptionsSnapshot<ConsoleUrlOptions>>().Value);

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));
            services.AddMvc(options =>
            {
                // Require all scopes in authOptions
                var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
                Array.ForEach(_authOptions.AuthorizationScope.Split(' '), x => policyBuilder.RequireScope(x));

                var policy = policyBuilder.Build();
                options.Filters.Add(new AuthorizeFilter(policy));

            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddSignalR()
               .AddJsonProtocol(options =>
               {
                   options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                   options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
               });

            // allow upload of large files
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = isoOptions.MaxFileSize;
            });

            services.AddSwagger(_authOptions);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = _authOptions.Authority;
                options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidAudiences = _authOptions.AuthorizationScope.Split(' ')
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/hubs")))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(p => p.GetService<IHttpContextAccessor>().HttpContext.User);

            services.AddScoped<IVmService, VmService>();
            services.AddScoped<IPlayerService, PlayerService>();
            services.AddScoped<IViewService, ViewService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // Vsphere Services
            services.AddSingleton<ConnectionService>();
            services.AddSingleton<IHostedService>(x => x.GetService<ConnectionService>());
            services.AddSingleton<IConnectionService>(x => x.GetService<ConnectionService>());
            services.AddScoped<IVsphereService, VsphereService>();
            services.AddSingleton<TaskService>();
            services.AddSingleton<IHostedService>(x => x.GetService<TaskService>());
            services.AddSingleton<ITaskService>(x => x.GetService<TaskService>());
            services.AddSingleton<MachineStateService>();
            services.AddSingleton<IHostedService>(x => x.GetService<MachineStateService>());
            services.AddSingleton<IMachineStateService>(x => x.GetService<MachineStateService>());

            services.AddTransient<EventTransactionInterceptor>();

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CheckTasksBehavior<,>));

            services.AddMemoryCache();

            services.AddApiClients(identityClientOptions: _identityClientOptions, clientOptions: _clientOptions);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }

            app.UseCustomExceptionHandler();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ProgressHub>("/hubs/progress");
                endpoints.MapHub<VmHub>("/hubs/vm");
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Player VM API V1");
                c.OAuthClientId(_authOptions.ClientId);
                c.OAuthClientSecret(_authOptions.ClientSecret);
                c.OAuthAppName(_authOptions.ClientName);
            });
        }
    }
}
