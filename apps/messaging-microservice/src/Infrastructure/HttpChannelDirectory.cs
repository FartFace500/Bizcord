using System.Net;
using Bizcord.Messaging.Domain;

namespace Bizcord.Messaging.Infrastructure;

/// <summary>Checks channels with <c>GET {baseUrl}/api/channels/{id}</c> on the Channel Management service.</summary>
public class HttpChannelDirectory : IChannelDirectory
{
    private readonly HttpClient _http;

    public HttpChannelDirectory(HttpClient http) => _http = http;

    public async Task<bool> ExistsAsync(Guid channelId, CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync($"api/channels/{channelId}", cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ChannelServiceUnavailableException("The channel service could not be reached.", ex);
        }

        using (response)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,
                _ => throw new ChannelServiceUnavailableException(
                    $"The channel service answered {(int)response.StatusCode}.")
            };
        }
    }
}
