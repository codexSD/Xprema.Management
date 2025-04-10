namespace Xprema.Framework.Entities.Common;

/// <summary>
/// Interface for entities that can be activated or deactivated
/// </summary>
public interface IActivable
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is active
    /// </summary>
    bool IsActive { get; set; }
} 