namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Record <c>Actor</c> represents a user or automation.
    /// </summary>
    /// <param name="Id">The actors own Id.</param>
    /// <param name="Type">The type of the actor, User or Automation.</param>
    public record Actor(
        string Id,
        string Type);
}