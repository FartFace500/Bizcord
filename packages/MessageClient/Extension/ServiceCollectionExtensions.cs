using EasyNetQ;
using EasyNetQ.Serialization.SystemTextJson;
using MessageClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageClient.Extension;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IMessageClient"/> backed by RabbitMQ. The connection is created
    /// lazily on first use, so the application can start (and be tested) without a broker.
    /// </summary>
    public static IServiceCollection AddRabbitMqMessageClient(
        this IServiceCollection services,
        RabbitMqClientOptions clientOptions,
        MessageHandlerOptions handlerOptions)
    {
        services.TryAddSingleton(handlerOptions);
        services.TryAddSingleton<IBus>(_ => RabbitHutch.CreateBus(clientOptions.ConnectionString, register => register.EnableSystemTextJson()));
        services.TryAddSingleton<IMessageClient, RabbitMqMessageClient>();
        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler"/> for <typeparamref name="TMessage"/> and subscribes
    /// to the message type when the host starts.
    /// </summary>
    public static IServiceCollection AddMessageHandler<TMessage, THandler>(this IServiceCollection services)
        where TMessage : class
        where THandler : class, IMessageHandler<TMessage>
    {
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        services.AddHostedService<MessageHandlerSubscriber<TMessage>>();
        return services;
    }
}
