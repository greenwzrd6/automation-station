namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Represents an automation executor for running automated operations for a specific company.
    /// </summary>
    /// <remarks>Sealed positional record with value-based equality and immutable properties.</remarks>
    /// <param name="Id">Unique identifier of the executor.</param>
    /// <param name="CompanyId">Identifier of the company that owns the executor.</param>
    /// <param name="Enabled">True if the executor is enabled; otherwise, false.</param>
    public sealed record AutomationExecutor(
        Guid Id,
        int CompanyId,
        bool Enabled);
}
