namespace Natech.MassTransit.Swagger;

using Azure.Identity;
using global::MassTransit;
using global::MassTransit.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class Program
{
    public static void Main(string[] args)
    {
        var argumentManager = new ArgumentManager(args);
        var builder = WebApplication.CreateBuilder(args);

        var dllPath = argumentManager.GetDllPath();
        var keyvaultArgs = argumentManager.GetKeyvaultArgs();
        var rabbitArgs = argumentManager.GetRabbitArgs();
        var sbConnectionString = argumentManager.GetAzureServiceBusConnectionString();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        if (keyvaultArgs.useKeyvault && keyvaultArgs.keyvaultUri is not null)
        {
            var identityOptions = new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCodeCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeInteractiveBrowserCredential = true
            };
            builder.Configuration.AddAzureKeyVault(new Uri(keyvaultArgs.keyvaultUri), new DefaultAzureCredential(identityOptions));
        }

        try
        {
            var consumerAssembly = Assembly.LoadFrom(dllPath ?? throw (new FileNotFoundException(Constants.FileNotFound)));
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
                if (!rabbitArgs.useRabbitMq)
                {
                    cfg.UsingAzureServiceBus((context, config) =>
                    {
                        config.Host((keyvaultArgs.useKeyvault) ? builder.Configuration[keyvaultArgs.keyvaultBusKey] : sbConnectionString);
                        config.ConfigureEndpoints(context);
                    });
                }
                else
                {
                    cfg.UsingRabbitMq((context, config) =>
                    {
                        config.Host(rabbitArgs.rabbitMqUri, rabbitArgs.rabbitMqVhost, h =>
                        {
                            h.Username(rabbitArgs.rabbitMqUsername);
                            h.Password(rabbitArgs.rabbitMqPassword);
                        });
                    });
                }
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
            var routePrefix = Constants.Consumers;
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