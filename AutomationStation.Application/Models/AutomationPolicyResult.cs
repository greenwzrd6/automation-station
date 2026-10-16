namespace AutomationStation.Application.Models
{
    /// <summary>
    /// Record <c>AutomationPolicyResult</c> represents the decision of whether an automation was allowed to trigger or not.
    /// </summary>
    /// <param name="Allowed">Whether it was allowed or not.</param>
    /// <param name="Reason">The reason the automation was denied, if it was.</param>
    public sealed record AutomationPolicyResult(
        bool Allowed,
        string? Reason)
    {
        public static AutomationPolicyResult Allow() =>
            new(true, null);

        public static AutomationPolicyResult Block(string reason) =>
            new(false, reason);
    }
}
