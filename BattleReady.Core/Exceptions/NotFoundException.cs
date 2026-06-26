namespace BattleReady.Core.Exceptions;

// Thrown when a requested resource does not exist.
// The API layer translates this to HTTP 404.
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}