using Azure.Identity;
using MassTransit;
using MassTransit.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace Natech.MassTransit.Swagger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var projectArg = args.FirstOrDefault(arg => arg.StartsWith("--project="));
            var dllPath = args.FirstOrDefault(arg => arg.StartsWith("--dllPath="));
            if (projectArg == null || dllPath == null)
            {
                Console.WriteLine("Missing --project or --dllPath argument");
                return;
            }
            var projectArgValue = Regex.Match(projectArg, @"--project=(.*)").Groups[1].Value;
            var dllPathValue = Regex.Match(dllPath, @"--dllPath=(.*)").Groups[1].Value;

            Console.WriteLine($"Using project: {projectArgValue}");
            Console.WriteLine($"Using dllPath: {dllPathValue}");

            //check if a --useKeyvault argument was passed, if so then a --keyvaultUri argument must also be passed , if any of those are missing set useKeyvault to false
            var useKeyvaultArg = args.FirstOrDefault(arg => arg.StartsWith("--useKeyvault"));
            var keyvaultUriArg = args.FirstOrDefault(arg => arg.StartsWith("--keyvaultUri="));
            var keyvaultBusKey = args.FirstOrDefault(arg => arg.StartsWith("--keyvaultBusKey="));
            var useKeyvault = false;
            useKeyvault = (useKeyvaultArg != null && keyvaultUriArg != null && keyvaultBusKey != null);
            var keyvaultUri = (useKeyvault) ? Regex.Match(keyvaultUriArg, @"--keyvaultUri=(.*)").Groups[1].Value : null;
            var keyVaultBusKey = (useKeyvault) ? Regex.Match(keyvaultBusKey, @"--keyvaultBusKey=(.*)").Groups[1].Value : null;

            Console.WriteLine($"Using keyvault: {useKeyvault}");
            if (useKeyvault)
            {
                Console.WriteLine($"Using keyvaultUri: {keyvaultUri}");
                Console.WriteLine($"Using keyvaultBusKey: {keyVaultBusKey}");
            }


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var identityOptions = new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCodeCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeInteractiveBrowserCredential = true
            };
            if (useKeyvault && keyvaultUri is not null)
            {
                try
                {
                    builder.Configuration.AddAzureKeyVault(new Uri("https://kv-snappi-dev-westeu.vault.azure.net/"), new DefaultAzureCredential(identityOptions));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error while trying to add keyvault {e.Message}");
                }
            }

            // Scan the referenced assembly for consumer types
            var ProjectReference = projectArgValue;
            var ProjectPath = Path.GetFullPath(ProjectReference);
            var OutputPath = Path.Combine(ProjectPath, "bin", "Debug", "net6.0");
            try
            {
                var consumerAssemblyPath = Path.Combine(OutputPath, dllPathValue);
                var consumerAssembly = Assembly.LoadFrom(consumerAssemblyPath);
                var consumerTypes = consumerAssembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IConsumer).IsAssignableFrom(type))
                .ToList();



                // Create a new controller to list consumer names
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Consumer API", Version = "v1" });
                });

                builder.Services.AddSingleton<IEnumerable<Type>>(consumerTypes);
                builder.Services.AddMassTransit(cfg =>
                {
                    // Configure MassTransit options
                    cfg.SetKebabCaseEndpointNameFormatter();

                    cfg.UsingAzureServiceBus((context, config) =>
                    {
                        config.Host((useKeyvault) ? builder.Configuration[keyVaultBusKey] : "Test");
                        config.ConfigureEndpoints(context);
                    });
                });

                var app = builder.Build();

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Consumer API v1");
                    c.RoutePrefix = string.Empty;
                });

                app.UseRouting();

                // Dynamically generate routes and configure controller actions for each consumer
                var routePrefix = "/consumers";
                for (var i = 0; i < consumerTypes.Count; i++)
                {
                    var consumerType = consumerTypes[i];
                    var messageType = consumerType.ClosesType(typeof(IConsumer<>), out Type[] types)
                    ? types[0]
                    : throw new InvalidOperationException();

                    var binder = (IBinder)Activator.CreateInstance(typeof(Binder<,>).MakeGenericType(consumerType, messageType))!;

                    binder.Build(app, routePrefix);
                }
                app.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while trying to load assembly {e.Message}");
            }


        }
    }

    interface IBinder
    {
        void Build(IEndpointRouteBuilder app, string route);
    }

    class Binder<TConsumer, TMessage> :
        IBinder
        where TConsumer : class, IConsumer
    {
        public void Build(IEndpointRouteBuilder app, string routePrefix)
        {
            var consumerName = KebabCaseEndpointNameFormatter.Instance.Consumer<TConsumer>();

            var route = $"{routePrefix}/{consumerName}";

            // Create a POST endpoint for each consumer
            app.MapPost(route,
                (IPublishEndpoint publishEndpoint, [FromBody] TMessage message) => publishEndpoint.Publish(message));
        }
    }
}
