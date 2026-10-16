namespace MessageClient;

internal static class SubscriptionIds
{
    public static string For<T>(string prefix) =>
        string.IsNullOrWhiteSpace(prefix) ? typeof(T).Name : $"{prefix}-{typeof(T).Name}";
}
