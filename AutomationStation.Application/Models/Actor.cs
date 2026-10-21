namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Record <c>Actor</c> represents a user or automation.
    /// </summary>
    /// <remarks>Every integration event carries the actor that caused it, and the same values are forwarded on
    /// outgoing requests (for example the Actor-Id and Actor-Type headers used by the kanban integration), so
    /// downstream systems can attribute a change to the human or automation that performed it. Instances are
    /// immutable value objects shared by reference between events.</remarks>
    /// <param name="Id">The actors own Id.</param>
    /// <param name="Type">The type of the actor, User or Automation.</param>
    public record Actor(
        string Id,
        string Type);
}