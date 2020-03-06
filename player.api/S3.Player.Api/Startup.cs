/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using S3.Player.Api.Extensions;
using S3.Player.Api.Data.Data;
using S3.Player.Api.Options;
using S3.Player.Api.Services;
using System;
using Newtonsoft.Json.Converters;
using AutoMapper;
using S3.Player.Api.Infrastructure.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using S3.Player.Api.Infrastructure.ClaimsTransformers;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using S3.Player.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Mvc;
using S3.Player.Api.Infrastructure.Extensions;

namespace S3.Player.Api
{
    public class Startup
    {
        public Options.AuthorizationOptions _authOptions = new Options.AuthorizationOptions();
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.GetSection("Authorization").Bind(_authOptions);
        }        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var provider = Configuration["Database:Provider"];
            switch (provider)
            {
                case "InMemory":
                    services.AddDbContextPool<PlayerContext>(opt => opt.UseInMemoryDatabase("api"));
                    break;
                case "Sqlite":
                case "SqlServer":
                case "PostgreSQL":
                    services.AddDbProvider(Configuration);
                    services.AddDbContextPool<PlayerContext>(builder => builder.UseConfiguredDatabase(Configuration));
                    break;
            }

            services.AddOptions()
                .Configure<DatabaseOptions>(Configuration.GetSection("Database"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<DatabaseOptions>>().CurrentValue)

                .Configure<ClaimsTransformationOptions>(Configuration.GetSection("ClaimsTransformation"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<ClaimsTransformationOptions>>().CurrentValue)

                .Configure<SeedDataOptions>(Configuration.GetSection("SeedData"))
                    .AddScoped(config => config.GetService<IOptionsMonitor<SeedDataOptions>>().CurrentValue);

            services.AddCors(options => options.UseConfiguredCors(Configuration.GetSection("CorsPolicy")));

            services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateFilter));
                options.Filters.Add(typeof(JsonExceptionFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwagger(_authOptions);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = _authOptions.Authority;
                options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;                
                options.Audience = _authOptions.AuthorizationScope;
                options.SaveToken = true;
            });

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddMemoryCache();            

            services.AddScoped<IExerciseService, ExerciseService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IExerciseMembershipService, ExerciseMembershipService>();
            services.AddScoped<ITeamMembershipService, TeamMembershipService>();

            services.AddScoped<IClaimsTransformation, AuthorizationClaimsTransformer>();
            services.AddScoped<IUserClaimsService, UserClaimsService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(p => p.GetService<IHttpContextAccessor>().HttpContext.User);            

            ApplyPolicies(services);

            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors("default");
            
            //move any querystring jwt to Auth bearer header
            app.Use(async (context, next) =>
            {
                if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"])
                    && context.Request.QueryString.HasValue)
                {
                    string token = context.Request.QueryString.Value
                        .Substring(1)
                        .Split('&')
                        .SingleOrDefault(x => x.StartsWith("bearer="))?.Split('=')[1];

                    if (!String.IsNullOrWhiteSpace(token))
                        context.Request.Headers.Add("Authorization", new[] {$"Bearer {token}"});
                }

                await next.Invoke();

            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exercise Player v1");
                c.OAuthClientId(_authOptions.ClientId);
                c.OAuthClientSecret(_authOptions.ClientSecret);
                c.OAuthAppName(_authOptions.ClientName);
            });

            app.UseAuthentication();

            app.UseSignalR(routes =>
                {
                    routes.MapHub<Hubs.ExerciseHub>("/notifications/exercise");
                    routes.MapHub<Hubs.TeamHub>("/notifications/team");
                    routes.MapHub<Hubs.UserHub>("/notifications/user");
                }
            );

            app.UseMvc();
        }


        private void ApplyPolicies(IServiceCollection services)
        {
            services.AddAuthorization();


            // TODO: Add these automatically with reflection?
            services.AddSingleton<IAuthorizationHandler, FullRightsHandler>();
            services.AddSingleton<IAuthorizationHandler, ExerciseAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, TeamAccessHandler>();
            services.AddSingleton<IAuthorizationHandler, SameUserOrExerciseAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, SameUserHandler>();
            services.AddSingleton<IAuthorizationHandler, ExerciseMemberHandler>();
            services.AddSingleton<IAuthorizationHandler, ExerciseCreationHandler>();
            services.AddSingleton<IAuthorizationHandler, ManageTeamHandler>();
            services.AddSingleton<IAuthorizationHandler, TeamMemberHandler>();
            services.AddSingleton<IAuthorizationHandler, PrimaryTeamHandler>();
            services.AddSingleton<IAuthorizationHandler, ManageExerciseHandler>();
        }
    }
}

