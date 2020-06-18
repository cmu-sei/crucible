/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using AutoMapper;
using Caster.Api.Data;
using Caster.Api.Infrastructure.Options;
using Caster.Api.Infrastructure.Extensions;
using Caster.Api.Infrastructure.Mapping;
using Caster.Api.Infrastructure.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Caster.Api.Infrastructure.Exceptions.Middleware;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.ClaimsTransformers;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Caster.Api.Hubs;
using Caster.Api.Features.Files;
using SimpleInjector;
using Caster.Api.Infrastructure.Identity;
using Caster.Api.Extensions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;

[assembly: ApiController]
namespace Caster.Api
{
    public class Startup
    {
        private Container container = new SimpleInjector.Container();
        public IConfiguration Configuration { get; }

        private readonly AuthorizationOptions _authOptions = new AuthorizationOptions();
        private readonly ClientOptions _clientOptions = new ClientOptions();
        private readonly TerraformOptions _terraformOptions = new TerraformOptions();
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            Configuration.GetSection("Authorization").Bind(_authOptions);
            Configuration.GetSection("Client").Bind(_clientOptions);
            Configuration.GetSection("Terraform").Bind(_terraformOptions);

            _loggerFactory = loggerFactory;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<CasterContext>(builder => builder.UseNpgsql(Configuration.GetConnectionString("PostgreSQL")));
            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));

            services.AddOptions()
                .Configure<TerraformOptions>(Configuration.GetSection("Terraform"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<TerraformOptions>>().CurrentValue);

            services.AddOptions()
                .Configure<ClientOptions>(Configuration.GetSection("Client"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<ClientOptions>>().CurrentValue);

            services.AddOptions()
                .Configure<PlayerOptions>(Configuration.GetSection("Player"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<PlayerOptions>>().CurrentValue);

            services.AddOptions()
                .Configure<ClaimsTransformationOptions>(Configuration.GetSection("ClaimsTransformation"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<ClaimsTransformationOptions>>().CurrentValue);

            services.AddOptions()
                .Configure<SeedDataOptions>(Configuration.GetSection("SeedData"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<SeedDataOptions>>().CurrentValue);

            services.AddOptions()
                .Configure<FileVersionScrubOptions>(Configuration.GetSection("FileVersions"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<FileVersionScrubOptions>>().CurrentValue);


            services.AddMvc()
            .AddJsonOptions(options =>
            {
                // must be synced with DefaultJsonSettings.cs
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
                options.JsonSerializerOptions.Converters.Add(new OptionalConverter());
            })
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    // must be synced with DefaultJsonSettings.cs
                    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
                    options.PayloadSerializerOptions.Converters.Add(new OptionalConverter());
                });

            services.AddSwagger(_authOptions);

            services.AddAutoMapper(cfg => {
                cfg.ForAllPropertyMaps(
                    pm => pm.SourceType != null && Nullable.GetUnderlyingType(pm.SourceType) == pm.DestinationType,
                    (pm, c) => c.MapFrom<object, object, object, object>(new IgnoreNullSourceValues(), pm.SourceMember.Name));
            }, typeof(Startup));

            services.AddMediator(container);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = _authOptions.Authority;
                    options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;
                    options.Audience = _authOptions.AuthorizationScope;
                    options.SaveToken = true;

                    // We have to hook the OnMessageReceived event in order to
                    // allow the JWT authentication handler to read the access
                    // token from the query string when a WebSocket or
                    // Server-Sent Events request comes in.

                    // Sending the access token in the query string is required due to
                    // a limitation in Browser APIs. We restrict it to only calls to the
                    // SignalR hub in this code.
                    // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
                    // for more information about security considerations when using
                    // the query string to transmit the access token.
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

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthorizationPolicy();

            services.AddApiClients(_clientOptions, _terraformOptions, _loggerFactory);

            services.AddScoped<ITerraformService, TerraformService>();
            services.AddScoped<IClaimsTransformation, AuthorizationClaimsTransformer>();
            services.AddScoped<IUserClaimsService, UserClaimsService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IIdentityResolver, IdentityResolver>();
            services.AddScoped<IGitlabRepositoryService, GitlabRepositoryService>();
            services.AddScoped<IArchiveService, ArchiveService>();
            services.AddScoped<IImportService, ImportService>();

            services.AddSingleton<Caster.Api.Domain.Services.IAuthenticationService, Caster.Api.Domain.Services.AuthenticationService>();
            services.AddSingleton<ILockService, LockService>();
            services.AddSingleton<IUserIdProvider, SubUserIdProvider>();
            services.AddSingleton<IOutputService, OutputService>();

            services.AddSingleton<IFileVersionScrubService, FileVersionScrubService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(x => x.GetService<IFileVersionScrubService>());

            services.AddSingleton<IPlayerSyncService, PlayerSyncService>();
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService>(x => x.GetService<IPlayerSyncService>());

            services.AddScoped<IGetFileQuery, GetFileQuery>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSimpleInjector(container);
            app.UseCustomExceptionHandler();
            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ProjectHub>("/hubs/project");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Caster API V1");
                c.RoutePrefix = "api";
                c.OAuthClientId(_authOptions.ClientId);
                c.OAuthClientSecret(_authOptions.ClientSecret);
                c.OAuthAppName(_authOptions.ClientName);
            });

            container.Verify();
        }
    }
}
