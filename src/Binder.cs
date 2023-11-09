using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Natech.MassTransit.Swagger;

internal class Binder<TConsumer, TMessage> : IBinder where TConsumer : class, IConsumer
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