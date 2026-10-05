using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Blockers
{
    public sealed class CorrelationLoopBlocker(
        ICorrelationLoopRepository correlationLoopRepository)
        : IEventBlocker
    {
        private readonly ICorrelationLoopRepository _correlationLoopRepository = correlationLoopRepository;

        public async Task<bool> IsBlockedAsync(
            EventBlockerContext context,
            CancellationToken cancellationToken)
        {

            return await _correlationLoopRepository.IsBlockedAsync(
                context.CorrelationId,
                context.EventId,
                cancellationToken);
        }
    }
}