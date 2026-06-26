namespace BattleReady.Core.Exceptions;

// Thrown when a request is structurally valid but violates
// a domain rule that model validation cannot catch.
// The API layer translates this to HTTP 422 Unprocessable Entity.
public class ValidationException : DomainException
{
    public ValidationException(string message) : base(message) { }
}