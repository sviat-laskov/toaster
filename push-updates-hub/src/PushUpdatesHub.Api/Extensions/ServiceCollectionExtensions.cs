﻿using EasyNetQ;
using EasyNetQ.AutoSubscribe;
using EasyNetQ.DI;
using EasyNetQ.Serialization.SystemTextJson;
using PushUpdatesHub.Api.IntegrationMessageHandlers;
using PushUpdatesHub.Api.Options;
using PushUpdatesHub.Api.Services;

namespace PushUpdatesHub.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyNetQ(this IServiceCollection services, RabbitMqOptions rabbitMqOptions)
        {
            services.RegisterEasyNetQ(rabbitMqOptions.ConnectionString, options => options
                .EnableMicrosoftLogging()
                .Register<ISerializer, SystemTextJsonSerializer>());
            services.AddSingleton<IAutoSubscriberMessageDispatcher, ServiceProviderMessageDispatcher>();
            services.AddSingleton(serviceProvider => new AutoSubscriber(serviceProvider.GetRequiredService<IBus>(), AppDomain.CurrentDomain.FriendlyName)
            {
                AutoSubscriberMessageDispatcher = serviceProvider.GetRequiredService<IAutoSubscriberMessageDispatcher>()
            });

            services.AddScoped<PushUpdateIntegrationMessageHandler>();

            return services;
        }
    }
}