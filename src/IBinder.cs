using Microsoft.AspNetCore.Routing;

namespace Natech.MassTransit.Swagger;

internal interface IBinder
{
    void Build(IEndpointRouteBuilder app, string route);
}
