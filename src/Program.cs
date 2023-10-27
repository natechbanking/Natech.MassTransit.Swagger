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

            //check if 3 arguments are passed
            if (args.Length != 3)
            {
                Console.WriteLine("Please provide the following arguments: <useKeyvault> <ProjectReference> <consumerAssemblyPath>");
                return;
            }

            //print arguments
            Console.WriteLine("Arguments:");
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
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

            builder.Configuration.AddAzureKeyVault(new Uri("https://kv-snappi-dev-westeu.vault.azure.net/"), new DefaultAzureCredential(identityOptions));

            // Scan the referenced assembly for consumer types
            var ProjectReference = "src\\Natech.BNPL.Orchestrator";
            var ProjectPath = Path.GetFullPath(ProjectReference);
            var OutputPath = Path.Combine(ProjectPath, "bin", "Debug", "net6.0");

            var consumerAssemblyPath = Path.Combine(OutputPath, "Natech.BNPL.Orchestrator.Service.dll");
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
                    config.Host(builder.Configuration["NatechBus"]);
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
