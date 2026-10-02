using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Abstractions
{
    public interface IActionRateLimiter
    {
        Task WaitAsync(
            ActionRateLimitKey key,
            int permittedActions,
            TimeSpan window,
            CancellationToken cancellationToken);
    }

    public sealed record ActionRateLimitKey(
        int CompanyId,
        string ActionType,
        TargetSystem TargetSystem);
}