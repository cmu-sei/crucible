/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using Caster.Api.Domain.Services;
using Caster.Api.Infrastructure.HttpHandlers;
using Caster.Api.Infrastructure.Options;
using Caster.Api.Infrastructure.Swashbuckle.OperationFilters;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using S3.VM.Api;
using SimpleInjector;
using Swashbuckle.AspNetCore.Swagger;

namespace Caster.Api.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        #region Api Clients

        public static void AddApiClients(this IServiceCollection services, ClientOptions clientOptions, TerraformOptions terraformOptions, ILoggerFactory loggerFactory)
        {
            services.AddPlayerClient(clientOptions, loggerFactory);
            services.AddIdentityClient(clientOptions, loggerFactory);
            services.AddGitlabClient(clientOptions, terraformOptions, loggerFactory);

            services.AddTransient<AuthenticatingHandler>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetPolicy(int maxRetryDelaySeconds, string serviceName, ILoggerFactory loggerFactory)
        {
            var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryForeverAsync(retryAttempt =>
            {
                var logger = loggerFactory.CreateLogger<Policy>();
                logger.LogError($"Attempt {retryAttempt}: Retrying connection to {serviceName}");
                return TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), maxRetryDelaySeconds));
            });

            return retryPolicy;
        }

        private static void AddPlayerClient(this IServiceCollection services, ClientOptions clientOptions, ILoggerFactory loggerFactory)
        {
            var policy = GetPolicy(clientOptions.MaxRetryDelaySeconds, "Player Vm Api", loggerFactory);

            services.AddHttpClient("player", client =>
            {
                // Workaround to avoid TaskCanceledException after several retries. TODO: find a better way to handle this.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddHttpMessageHandler<AuthenticatingHandler>()
            .AddPolicyHandler(policy);

            services.AddScoped<IS3VmApiClient, S3VmApiClient>(p =>
            {
                var httpClientFactory = p.GetRequiredService<IHttpClientFactory>();
                var playerOptions = p.GetRequiredService<PlayerOptions>();

                var uri = new Uri(playerOptions.VmApiUrl);

                var httpClient = httpClientFactory.CreateClient("player");
                httpClient.BaseAddress = uri;

                var s3VmApiClient = new S3VmApiClient(httpClient, true);
                s3VmApiClient.BaseUri = uri;

                return s3VmApiClient;
            });
        }

        private static void AddIdentityClient(this IServiceCollection services, ClientOptions clientOptions, ILoggerFactory loggerFactory)
        {
            var policy = GetPolicy(clientOptions.MaxRetryDelaySeconds, "Identity", loggerFactory);

            services.AddHttpClient("identity", client =>
            {
                // Workaround to avoid TaskCanceledException after several retries. TODO: find a better way to handle this.
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddPolicyHandler(policy);
        }

        private static void AddGitlabClient(this IServiceCollection services, ClientOptions clientOptions, TerraformOptions terraformOptions, ILoggerFactory loggerFactory)
        {
            var policy = GetPolicy(clientOptions.MaxRetryDelaySeconds, "Gitlab Api", loggerFactory);
            services.AddHttpClient("gitlab", client =>
            {
                // Workaround to avoid TaskCanceledException after several retries. TODO: find a better way to handle this.
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.BaseAddress = new Uri(terraformOptions.GitlabApiUrl);
            }).AddPolicyHandler(policy);

        }


        #endregion

        #region Swagger

        public static void AddSwagger(this IServiceCollection services, AuthorizationOptions authOptions)
        {
            // XML Comments path
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            var commentsFilePath = Path.Combine(baseDirectory, commentsFileName);

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Caster API", Version = "v1" });
                c.CustomSchemaIds(schemaIdStrategy);
                c.OperationFilter<JsonIgnoreQueryOperationFilter>();
                c.OperationFilter<DefaultResponseOperationFilter>();

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

                c.IncludeXmlComments(commentsFilePath);
                c.MapType<Optional<Guid?>>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
                c.MapType<JsonElement?>(() => new OpenApiSchema { Type = "object" , Nullable = true});
            });
        }

        private static string schemaIdStrategy(Type currentClass)
        {
            var dataContractAttribute = currentClass.GetCustomAttribute<DataContractAttribute>();
            return dataContractAttribute != null && dataContractAttribute.Name != null ? dataContractAttribute.Name : currentClass.Name;
        }

        #endregion

        #region MediatR

        public static void AddMediator(this IServiceCollection services, Container container)
        {
            services.AddSimpleInjector(container, options =>
            {
                options.AddAspNetCore().AddControllerActivation();
                options.AddHostedService<RunQueueService>();
            });

            // use SimpleInjector DI container for MediatR
            // in order to support constrained open generics for Behaviors
            container.BuildMediator(new [] {Assembly.GetExecutingAssembly()});

            // register with simpleinjector because it uses MediatR
            container.RegisterSingleton<IRunQueueService, RunQueueService>();
        }

        #endregion
    }
}
