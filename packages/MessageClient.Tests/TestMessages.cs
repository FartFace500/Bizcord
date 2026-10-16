namespace MessageClient.Tests;

public class PingMessage
{
    public string Text { get; set; } = string.Empty;
}

public class RecordingHandler : IMessageHandler<PingMessage>
{
    public static List<string> Received { get; } = new();

    public Task Handle(PingMessage message, CancellationToken cancellationToken)
    {
        lock (Received) Received.Add(message.Text);
        return Task.CompletedTask;
    }
}
