using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MessageClient;

namespace Bizcord.Messaging.Tests.Support;

/// <summary>
/// Hosts the real service in memory. The broker is replaced by a <see cref="FakeMessageClient"/>,
/// so these tests need no RabbitMQ. Everything else (routing, JSON, domain, repository) is real.
/// </summary>
public class MessagingApiFactory : WebApplicationFactory<Program>
{
    /// <summary>When set, switches on the optional channel check against this base URL.</summary>
    public string? ChannelServiceUrl { get; init; }

    public FakeMessageClient MessageClient { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (ChannelServiceUrl is not null)
            builder.UseSetting("ChannelService:BaseUrl", ChannelServiceUrl);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IMessageClient>();
            services.AddSingleton<IMessageClient>(MessageClient);
        });
    }
}
