namespace CleanArchitecture.Domain.Shared.Attributes;

/// <summary>
/// This attributes ensures any command tagged by it is audited and the action will be logged
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuditableAttribute : Attribute;
