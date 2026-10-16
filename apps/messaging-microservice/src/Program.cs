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
