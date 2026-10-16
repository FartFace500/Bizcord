using Bizcord.Messaging.Domain;

namespace Bizcord.Messaging.Tests.Domain;

public class MessageTests
{
    [Fact]
    public void Message_Create_SetsAllFields()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var message = Message.Create(channelId, authorId, "Hello world");

        message.Id.Should().NotBe(Guid.Empty);
        message.ChannelId.Should().Be(channelId);
        message.AuthorId.Should().Be(authorId);
        message.Content.Should().Be("Hello world");
        message.PostedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Message_Create_Throws_WhenContentIsEmpty()
    {
        var act = () => Message.Create(Guid.NewGuid(), Guid.NewGuid(), "");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\t")]
    public void Message_Create_Throws_WhenContentIsBlank(string? content)
    {
        var act = () => Message.Create(Guid.NewGuid(), Guid.NewGuid(), content);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Message_Create_Throws_WhenChannelIdIsEmpty()
    {
        var act = () => Message.Create(Guid.Empty, Guid.NewGuid(), "Hi");

        act.Should().Throw<DomainException>().WithMessage("*channel*");
    }

    [Fact]
    public void Message_Create_Throws_WhenAuthorIdIsEmpty()
    {
        var act = () => Message.Create(Guid.NewGuid(), Guid.Empty, "Hi");

        act.Should().Throw<DomainException>().WithMessage("*author*");
    }

    [Fact]
    public void Message_Create_AcceptsContentAtTheMaximumLength()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), new string('a', Message.MaxContentLength));

        message.Content.Should().HaveLength(Message.MaxContentLength);
    }

    [Fact]
    public void Message_Create_Throws_WhenContentIsTooLong()
    {
        var act = () => Message.Create(Guid.NewGuid(), Guid.NewGuid(), new string('a', Message.MaxContentLength + 1));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Message_Create_GivesEachMessageItsOwnId()
    {
        var first = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "one");
        var second = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "two");

        first.Id.Should().NotBe(second.Id);
    }
}
