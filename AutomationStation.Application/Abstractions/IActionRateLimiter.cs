using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Throttles automation actions so that no more than a configured number execute per time window.
    /// </summary>
    /// <remarks>Each key has its own independent window, so limits for one company, action type and target system do
    /// not affect the others. Callers wait until a slot is available rather than being rejected, so a caller that
    /// exceeds its limit is delayed rather than failed.</remarks>
    public interface IActionRateLimiter
    {
        /// <summary>
        /// Consumes one action slot for the given rate-limit key, waiting until one is available.
        /// </summary>
        /// <remarks>When the current window is exhausted, the call blocks until the window expires and then acquires
        /// a slot in the new window.</remarks>
        /// <param name="key">Identifies the bucket to charge the action against: company, action type and target system.</param>
        /// <param name="permittedActions">Maximum number of actions allowed within one window.</param>
        /// <param name="window">Length of the rate-limit window.</param>
        /// <param name="cancellationToken">A token used to cancel the wait.</param>
        /// <returns>A task that completes once a slot has been acquired and the action may proceed.</returns>
        /// <exception cref="OperationCanceledException">The wait is canceled through <paramref name="cancellationToken"/>.</exception>
        Task WaitAsync(
            ActionRateLimitKey key,
            int permittedActions,
            TimeSpan window,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Identifies a rate-limit bucket that actions are throttled against.
    /// </summary>
    /// <remarks>Two actions compete for the same window only when all three components are equal; the record uses
    /// value-based equality for that comparison.</remarks>
    /// <param name="CompanyId">The company whose actions are being limited.</param>
    /// <param name="ActionType">The type of action being limited, e.g. "UpdateTaskStatus".</param>
    /// <param name="TargetSystem">The system the action is executed against, see <see cref="Core.Automations.Systems.TargetSystem"/>.</param>
    public sealed record ActionRateLimitKey(
        int CompanyId,
        string ActionType,
        TargetSystem TargetSystem);
}