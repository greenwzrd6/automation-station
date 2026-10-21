using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;

namespace AutomationStation.Infrastructure.Actions
{
    /// <summary>
    /// Executes actions by resolving an appropriate IActionHandler, validating the action against the action catalog,
    /// enforcing rate limits, and performing TOJ executor validation when required.
    /// </summary>
    /// <remarks>Builds a case-insensitive handler lookup from handlers; ensures an event is present; verifies catalog
    /// entry and TOJ executor validity when applicable; waits on the rate limiter using catalog settings; then delegates
    /// execution to the resolved handler.</remarks>
    /// <param name="handlers">Collection of IActionHandler instances used to resolve handlers by target system and action type (keyed as
    /// "{TargetSystem}:{ActionType}").</param>
    /// <param name="rateLimiter">IActionRateLimiter used to enforce action rate limits per company, action type, and target system.</param>
    /// <param name="actionCatalog">IActionCatalog containing allowed actions, permitted action counts, and rate limit windows.</param>
    /// <param name="executorRepository">IAutomationExecutorRepository used to retrieve and validate automation executors for TOJ target system actions.</param>
    public sealed class ActionDispatcher(
        IEnumerable<IActionHandler> handlers,
        IActionRateLimiter rateLimiter,
        IActionCatalog actionCatalog,
        IAutomationExecutorRepository executorRepository)
        : IActionDispatcher
    {
        private readonly Dictionary<string, IActionHandler> _handlers =
            handlers.ToDictionary(
                handler => $"{handler.TargetSystem}:{handler.ActionType}",
                StringComparer.OrdinalIgnoreCase);

        private readonly IActionRateLimiter _rateLimiter = rateLimiter;
        private readonly IActionCatalog _actionCatalog = actionCatalog;
        private readonly IAutomationExecutorRepository _executorRepository = executorRepository;

        /// <summary>
        /// Execute the specified action using the appropriate handler while enforcing catalog permissions and per-company
        /// rate limits.
        /// </summary>
        /// <remarks>Validates the action against the action catalog, performs TOJ-specific executor validation
        /// and company resolution for rate limiting, waits on the rate limiter according to the catalog entry, then invokes
        /// the resolved handler.</remarks>
        /// <param name="then">Action descriptor containing Type, TargetSystem, and optional ExecutorId used to select the catalog entry and
        /// handler.</param>
        /// <param name="context">Execution context containing the triggering event and ambient data required by handlers.</param>
        /// <param name="cancellationToken">Token to observe while awaiting the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous execution.</returns>
        /// <exception cref="NotSupportedException">When the action type or target system is not allowed by the action catalog or when no handler is registered for
        /// the specified target system and action type.</exception>
        /// <exception cref="InvalidOperationException">When the event context is missing, when a TOJ action lacks a valid executor, when the executor does not exist or
        /// is disabled, or when an invalid company identifier is encountered.</exception>
        /// <exception cref="OperationCanceledException">When the operation is canceled through
        /// <paramref name="cancellationToken"/>, for example while waiting for a rate limit slot.</exception>
        public async Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken)
        {
            if (!_actionCatalog.TryGet(
                    then.Type,
                    then.TargetSystem,
                    out var catalogEntry))
            {
                throw new NotSupportedException(
                    $"Action '{then.Type}' is not allowed " +
                    $"for target system '{then.TargetSystem}'.");
            }

            if (context.Event is null)
            {
                throw new InvalidOperationException(
                    "Event context is required for action execution.");
            }

            var handlerKey = $"{then.TargetSystem}:{then.Type}";

            if (!_handlers.TryGetValue(handlerKey, out var handler))
            {
                throw new NotSupportedException(
                    $"Unsupported action type '{then.Type}' " +
                    $"for target system '{then.TargetSystem}'.");
            }

            var rateLimitCompanyId = context.Event.CompanyId;

            if (then.TargetSystem == TargetSystem.TojSystem)
            {
                if (!then.ExecutorId.HasValue ||
                    then.ExecutorId.Value == Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "TOJ actions require a valid executor.");
                }

                var executor = await _executorRepository.GetByIdAsync(
                    then.ExecutorId.Value,
                    cancellationToken);

                if (executor is null || !executor.Enabled)
                {
                    throw new InvalidOperationException(
                        "The automation executor does not exist or is disabled.");
                }

                if (executor.CompanyId <= 0)
                {
                    throw new InvalidOperationException("Invalid company ID.");
                }

                rateLimitCompanyId = executor.CompanyId;
            }

            await _rateLimiter.WaitAsync(
                new ActionRateLimitKey(
                    CompanyId: rateLimitCompanyId,
                    ActionType: then.Type,
                    TargetSystem: then.TargetSystem),
                permittedActions: catalogEntry.PermittedActions,
                window: catalogEntry.RateLimitWindow,
                cancellationToken);

            await handler.ExecuteAsync(
                then,
                context,
                cancellationToken);
        }
    }
}