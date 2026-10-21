using Microsoft.Extensions.Logging;

using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Whens;

namespace AutomationStation.Application.Services
{
    /// <summary>
    /// Orchestrates the evaluation and execution of automations in response to an integration event.
    /// </summary>
    /// <remarks>For each enabled automation whose trigger matches the event, the processor applies the execution
    /// guard, records the execution against a per-automation, per-correlation rate limit, and then dispatches the
    /// automation's actions in order. Automations that fail any of these checks are skipped, and a rate-limit rejection
    /// is logged as a warning. Non-matching automations are ignored silently.</remarks>
    /// <param name="automationRepository">Repository used to load the enabled automations for the event type.</param>
    /// <param name="conditionEvaluator">Evaluates the automation's conditions against the event payload.</param>
    /// <param name="executionGuard">Decides whether an automation is blocked from executing for this event.</param>
    /// <param name="actionExecutor">Dispatches each action of a matching automation.</param>
    /// <param name="automationExecutionRepository">Persists and retrieves the execution record for an
    /// automation/event pair.</param>
    /// <param name="correlationLoopRepository">Rate limiter that records executions and rejects automations that
    /// exceed the allowed number of executions per correlation window.</param>
    /// <param name="logger">Logger used to report blocked executions.</param>
    public sealed class AutomationProcessor(
        IAutomationRepository automationRepository,
        IConditionEvaluator conditionEvaluator,
        IAutomationExecutionGuard executionGuard,
        IActionDispatcher actionExecutor,
        IAutomationExecutionRepository automationExecutionRepository,
        IAutomationExecutionLimiter correlationLoopRepository,
        ILogger<AutomationProcessor> logger)
        : IAutomationProcessor
    {
        private readonly IAutomationRepository _automationRepository = automationRepository;
        private readonly IConditionEvaluator _conditionEvaluator = conditionEvaluator;
        private readonly IAutomationExecutionGuard _executionGuard = executionGuard;
        private readonly IActionDispatcher _actionExecutor = actionExecutor;
        private readonly IAutomationExecutionRepository _automationExecutionRepository = automationExecutionRepository;
        private readonly IAutomationExecutionLimiter _correlationLoopRepository = correlationLoopRepository;
        private readonly ILogger<AutomationProcessor> _logger = logger;

        /// <summary>
        /// Processes an integration event by running every enabled automation whose trigger and conditions match it.
        /// </summary>
        /// <remarks>Each matching automation is subjected to the execution guard and the execution rate limit before
        /// any action runs; when the rate limit rejects the automation, a warning naming the automation, actor, and
        /// correlation id is logged and the automation is skipped. Actions are dispatched sequentially in the
        /// automation's defined order, sharing a single <see cref="ActionContext"/>.</remarks>
        /// <param name="integrationEvent">The event to process; its <c>EventType</c> and <c>Source</c> determine which
        /// automations are considered.</param>
        /// <param name="cancellationToken">A token used to cancel repository reads and action dispatch.</param>
        /// <returns>A task that completes when all matching automations have been evaluated and their actions
        /// dispatched.</returns>
        public async Task ProcessAsync(
            IntegrationEvent integrationEvent,
            CancellationToken cancellationToken)
        {
            var automations = await _automationRepository.GetEnabledByEventTypeAsync(
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

                var evaluationContext = new EvaluationContext(automation, integrationEvent);

                if (await _executionGuard.IsBlockedAsync(
                        evaluationContext,
                        cancellationToken))
                {
                    continue;
                }

                if (!await _correlationLoopRepository.TryRecordExecutionAsync(
                        evaluationContext,
                        cancellationToken))
                {
                    _logger.LogWarning(
                        """
                        Automation '{AutomationName}' ({AutomationId}) blocked.
                        ActorId: {ActorId}, CorrelationId: {CorrelationId}, EventId: {EventId}.
                        Reason: actor reached the limit of 10000 executions within 3 minutes.
                        """,
                        evaluationContext.AutomationName,
                        evaluationContext.AutomationId,
                        evaluationContext.Actor.Id,
                        evaluationContext.CorrelationId,
                        integrationEvent.EventId);

                    continue;
                }

                var executionId = await _automationExecutionRepository.GetOrCreateAsync(
                    automation.Id,
                    integrationEvent.EventId,
                    cancellationToken);

                var context = new ActionContext(
                    integrationEvent,
                    executionId);

                foreach (var then in automation.Thens)
                {
                    await _actionExecutor.ExecuteAsync(
                        then,
                        context,
                        cancellationToken);
                }
            }
        }

        // Matches when the event type (compared case-insensitively) and source system equal the trigger and every
        // condition evaluates to true.
        private bool Matches(
            When when,
            IntegrationEvent integrationEvent)
        {
            if (!string.Equals(
                    when.Trigger.EventType,
                    integrationEvent.EventType,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (when.Trigger.SourceSystem != integrationEvent.Source)
            {
                return false;
            }

            return when.Conditions.All(
                condition =>
                    _conditionEvaluator.Matches(
                        condition,
                        integrationEvent));
        }
    }
}
