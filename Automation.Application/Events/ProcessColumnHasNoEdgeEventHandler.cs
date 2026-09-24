using Automation.Application.Abstractions;
using Automation.Application.Contracts;
using Automation.Application.Models;

namespace Automation.Application.Events
{
    public sealed class ProcessColumnHasNoEdgeEventHandler(
        IAutomationRepository automationRepository,
        IActionExecutor actionExecutor)
    {
        private const string ColumnHasNoEdge = "ColumnHasNoEdge";

        private readonly IAutomationRepository _automationRepository = automationRepository;
        private readonly IActionExecutor _actionExecutor = actionExecutor;

        public async Task HandleAsync(
            IntegrationEvent<ColumnHasNoEdgePayload> integrationEvent,
            CancellationToken cancellationToken)
        {
            if (!string.Equals(
                    integrationEvent.EventType,
                    ColumnHasNoEdge,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var automations =
                await _automationRepository.GetEnabledByEventTypeAsync(
                    integrationEvent.EventType,
                    cancellationToken);
            foreach (var automation in automations)
            {
                if (!Matches(
                        automation.When,
                        integrationEvent))
                {
                    continue;
                }
                var context = new ColumnActionContext(
                    ColumnId: integrationEvent.Payload.ColumnId,
                    CausationEventId: integrationEvent.EventId);
                foreach (var then in automation.Thens)
                {
                    await _actionExecutor.ExecuteAsync(
                        then,
                        context,
                        cancellationToken);
                }
            }
        }
    }
}
