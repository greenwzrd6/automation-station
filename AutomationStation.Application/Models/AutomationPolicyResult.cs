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
        /// <summary>
        /// Creates a result that allows the automation to trigger, with no denial reason.
        /// </summary>
        /// <returns>A result with <see cref="Allowed"/> set to <see langword="true"/> and <see cref="Reason"/> set to
        /// <see langword="null"/>.</returns>
        public static AutomationPolicyResult Allow() =>
            new(true, null);

        /// <summary>
        /// Creates a result that blocks the automation from triggering, with an explanation of why.
        /// </summary>
        /// <param name="reason">Human-readable explanation of why the automation was denied.</param>
        /// <returns>A result with <see cref="Allowed"/> set to <see langword="false"/> and the given
        /// <paramref name="reason"/>.</returns>
        public static AutomationPolicyResult Block(string reason) =>
            new(false, reason);
    }
}
