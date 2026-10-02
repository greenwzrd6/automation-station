namespace AutomationStation.Application.Models
{
    public sealed record AutomationPolicyResult(
    bool Allowed,
    string? Reason)
    {
        public static AutomationPolicyResult Allow() =>
            new(true, null);

        public static AutomationPolicyResult Block(
            string reason) =>
            new(false, reason);
    }
}
