using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrdersService.Application.Commands.Handlers;
using OrdersService.Application.PipelineBehaviours;

namespace OrdersService.Application.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddMediatR(typeof(OrderCreationCommandHandler))
        .AddAutoMapper(typeof(MapperProfile))
        .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
}