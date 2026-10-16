namespace Bizcord.Messaging.Domain;

/// <summary>Raised when a domain rule is violated, for example an empty message.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
