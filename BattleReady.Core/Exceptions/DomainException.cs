namespace BattleReady.Core.Exceptions;

// Base class for all domain exceptions in BattleReady.
// Having a common base type lets the API layer catch all domain
// exceptions in one place, while still being able to distinguish
// between specific types (NotFoundException, ValidationException, etc.)
// using pattern matching if needed.
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}