using Bizcord.Messaging.Domain;
using Bizcord.Messaging.Infrastructure;
using MessageClient.Configuration;
using MessageClient.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var rabbitMqHost = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";
builder.Services.AddRabbitMqMessageClient(
    new RabbitMqClientOptions { ConnectionString = $"host={rabbitMqHost}" },
    new MessageHandlerOptions { SubscriptionPrefix = "messaging" });

builder.Services.AddSingleton<IMessageRepository, InMemoryMessageRepository>();

// The channel check is optional: it is switched on by setting ChannelService:BaseUrl
// (environment variable ChannelService__BaseUrl). Read lazily so test overrides apply.
builder.Services.AddHttpClient(nameof(HttpChannelDirectory), client => client.Timeout = TimeSpan.FromSeconds(3));
builder.Services.AddSingleton<IChannelDirectory>(sp =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ChannelService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        return new AllowAllChannelDirectory();

    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(HttpChannelDirectory));
    http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    return new HttpChannelDirectory(http);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
