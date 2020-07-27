/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Player.Vm.Api.Infrastructure.HttpHandlers;
using Player.Vm.Api.Infrastructure.OperationFilters;
using Player.Vm.Api.Infrastructure.Options;
using S3.Player.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;

namespace Player.Vm.Api.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Swagger

        public static void AddSwagger(this IServiceCollection services, AuthorizationOptions authOptions)
        {
            // XML Comments path
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            string commentsFile = Path.Combine(baseDirectory, commentsFileName);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Player VM API", Version = "v1" });

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
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

                c.EnableAnnotations();
                c.IncludeXmlComments(commentsFile);
                c.CustomSchemaIds(schemaIdStrategy);
                c.OperationFilter<DefaultResponseOperationFilter>();
                c.OperationFilter<JsonIgnoreQueryOperationFilter>();
            });
        }

        private static string schemaIdStrategy(Type currentClass)
        {
            var dataContractAttribute = currentClass.GetCustomAttribute<DataContractAttribute>();
            return dataContractAttribute != null && dataContractAttribute.Name != null ? dataContractAttribute.Name : currentClass.Name;
        }

        #endregion

        #region Api Clients

        public static void AddApiClients(
            this IServiceCollection services,
            IdentityClientOptions identityClientOptions,
            ClientOptions clientOptions)
        {
            services.AddHttpClient();
            services.AddIdentityClient(identityClientOptions);
            services.AddPlayerClient(clientOptions);
            services.AddTransient<AuthenticatingHandler>();
        }

        private static void AddIdentityClient(
            this IServiceCollection services,
            IdentityClientOptions identityClientOptions)
        {
            services.AddHttpClient("identity");
        }


        private static void AddPlayerClient(
            this IServiceCollection services,
            ClientOptions clientOptions)
        {
            services.AddHttpClient("player-admin")
                .AddHttpMessageHandler<AuthenticatingHandler>();

            services.AddScoped<IS3PlayerApiClient, S3PlayerApiClient>(p =>
            {
                var httpContextAccessor = p.GetRequiredService<IHttpContextAccessor>();
                var httpClientFactory = p.GetRequiredService<IHttpClientFactory>();
                var clientOptions = p.GetRequiredService<ClientOptions>();

                var playerUri = new Uri(clientOptions.urls.playerApi);

                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                if (authHeader == null)
                {
                    var token = httpContextAccessor.HttpContext.Request.Query["access_token"];
                    authHeader = new AuthenticationHeaderValue("Bearer", token).ToString();
                }

                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = playerUri;
                httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                var s3PlayerApiClient = new S3PlayerApiClient(httpClient, true);
                s3PlayerApiClient.BaseUri = playerUri;

                return s3PlayerApiClient;
            });
        }

        #endregion
    }
}
