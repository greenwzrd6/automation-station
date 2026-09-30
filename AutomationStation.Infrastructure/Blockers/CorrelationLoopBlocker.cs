using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Blockers;

public sealed class CorrelationLoopBlocker(
    ICorrelationLoopRepository correlationLoopRepository)
    : IEventBlocker
{
    public async Task<bool> IsBlockedAsync(
        EventBlockerContext context,
        CancellationToken cancellationToken)
    {
        if (context.CorrelationId is null)
        {
            return false;
        }

        return await correlationLoopRepository.IsBlockedAsync(
            context.CorrelationId.Value,
            context.EventId,
            cancellationToken);
    }
}
