namespace Pid.Domain.Common;

/// <summary>Thrown when a domain rule is violated (e.g. negative heat capacity).</summary>
public sealed class DomainException(string message) : Exception(message);
