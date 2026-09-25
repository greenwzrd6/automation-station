using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;

namespace AutomationStation.Application.Events
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
                    CausationEventId: integrationEvent.CausationEventId ?? integrationEvent.EventId);
                foreach (var then in automation.Thens)
                {
                    await _actionExecutor.ExecuteAsync(
                        then,
                        context,
                        cancellationToken);
                }
            }
        }

        private static bool Matches(
            When when,
            IntegrationEvent<ColumnHasNoEdgePayload> integrationEvent)
        {
            if (!string.Equals(
                    when.EventType,
                    integrationEvent.EventType,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(
                    when.EventSource,
                    integrationEvent.Source,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
