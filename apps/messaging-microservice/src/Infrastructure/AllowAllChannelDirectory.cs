using Bizcord.Messaging.Domain;

namespace Bizcord.Messaging.Infrastructure;

/// <summary>Used when no Channel service is configured, so the service runs on its own.</summary>
public class AllowAllChannelDirectory : IChannelDirectory
{
    public Task<bool> ExistsAsync(Guid channelId, CancellationToken cancellationToken) => Task.FromResult(true);
}
