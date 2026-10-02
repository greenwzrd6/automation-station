using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IEventBlocker
    {
        Task<bool> IsBlockedAsync(
            EventBlockerContext context,
            CancellationToken cancellationToken);
    }
}
