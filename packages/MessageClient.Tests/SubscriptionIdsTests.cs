namespace MessageClient.Tests;

public class SubscriptionIdsTests
{
    [Fact]
    public void For_CombinesPrefixAndMessageTypeName()
        => Assert.Equal("messaging-PingMessage", SubscriptionIds.For<PingMessage>("messaging"));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void For_UsesOnlyTypeName_WhenPrefixIsBlank(string prefix)
        => Assert.Equal("PingMessage", SubscriptionIds.For<PingMessage>(prefix));

    [Fact]
    public void For_GivesDifferentServicesDifferentQueues()
        => Assert.NotEqual(SubscriptionIds.For<PingMessage>("messaging"), SubscriptionIds.For<PingMessage>("search"));
}
