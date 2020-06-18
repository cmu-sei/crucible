/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Alloy.Api.Infrastructure.Options;
using Alloy.Api.Infrastructure.OperationFilters;
using Alloy.Api.Options;
using Alloy.Api.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using S3.Player.Api;
using Caster.Api;
using Steamfitter.Api;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using System.Text.Json;
using Microsoft.OpenApi.Models;

namespace Alloy.Api.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwagger(this IServiceCollection services, AuthorizationOptions authOptions)
        {
            // XML Comments path
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            string commentsFile = Path.Combine(baseDirectory, commentsFileName);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Alloy API", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(authOptions.AuthorizationUrl),
                            Scopes = new Dictionary<string, string>()
                            {
                                {authOptions.AuthorizationScope, "public api access"}
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            Scheme = "oauth2"
                        },
                        new[] {authOptions.AuthorizationScope}
                    }
                });

                c.IncludeXmlComments(commentsFile);
                c.OperationFilter<DefaultResponseOperationFilter>();
                c.MapType<Optional<Guid?>>(() => new OpenApiSchema { Type = "string", Format = "uuid", Nullable= true });
                c.MapType<JsonElement?>(() => new OpenApiSchema { Type = "object" , Nullable = true});
            });
        }

        public static void AddS3PlayerApiClient(this IServiceCollection services)
        {
            services.AddScoped<IS3PlayerApiClient, S3PlayerApiClient>(p =>
            {
                var httpContextAccessor = p.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = p.GetRequiredService<IHttpClientFactory>();
                var clientOptions = p.GetRequiredService<ClientOptions>();

                var playerUri = new Uri(clientOptions.urls.playerApi);

                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = playerUri;
                httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                var apiClient = new S3PlayerApiClient(httpClient, true);
                apiClient.BaseUri = playerUri;

                return apiClient;
            });
        }

        public static void AddCasterApiClient(this IServiceCollection services)
        {
            services.AddScoped<ICasterApiClient, CasterApiClient>(p =>
            {
                var httpContextAccessor = p.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = p.GetRequiredService<IHttpClientFactory>();
                var clientOptions = p.GetRequiredService<ClientOptions>();

                var casterUri = new Uri(clientOptions.urls.casterApi);

                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = casterUri;
                httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                var apiClient = new CasterApiClient(httpClient, true);
                apiClient.BaseUri = casterUri;

                return apiClient;
            });
        }

        public static void AddSteamfitterApiClient(this IServiceCollection services)
        {
            services.AddScoped<ISteamfitterApiClient, SteamfitterApiClient>(p =>
            {
                var httpContextAccessor = p.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = p.GetRequiredService<IHttpClientFactory>();
                var clientOptions = p.GetRequiredService<ClientOptions>();

                var steamfitterUri = new Uri(clientOptions.urls.steamfitterApi);

                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = steamfitterUri;
                httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                var apiClient = new SteamfitterApiClient(httpClient, true);
                apiClient.BaseUri = steamfitterUri;

                return apiClient;
            });
        }

        public static void AddAlloyBackgroundService(this IServiceCollection services)
        {
            services.AddSingleton<IAlloyEventQueue, AlloyEventQueue>();
            services.AddHostedService<AlloyQueryService>();
            services.AddHostedService<AlloyBackgroundService>();
        }
    }
}
