using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IEnumerable<IActionHandler> handlers,
    IActionRateLimiter rateLimiter,
    IActionCatalog actionCatalog,
    IAutomationExecutorRepository executorRepository)
    : IActionExecutor
{
    private readonly Dictionary<string, IActionHandler> _handlers =
        handlers.ToDictionary(
            handler => $"{handler.TargetSystem}:{handler.ActionType}",
            StringComparer.OrdinalIgnoreCase);
    private readonly IActionRateLimiter _rateLimiter = rateLimiter;
    private readonly IActionCatalog _actionCatalog = actionCatalog;

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

        if (context.Event == null)
        {
            throw new InvalidOperationException(
                "Event context is required for action execution.");
        }

        if (context.Event.CompanyId <= 0)
        {
            throw new InvalidOperationException(
                "Invalid company ID.");
        }

        if (context.Event.CompanyId == 0)
        {
            throw new InvalidOperationException(
                "Invalid company ID.");
        }

        if (then.TargetSystem == TargetSystem.TojSystem)
        {
            if (!then.ExecutorId.HasValue ||
                then.ExecutorId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "TOJ actions require a valid executor.");
            }

            var executorIsEnabled =
                await executorRepository.IsEnabledAsync(
                    then.ExecutorId.Value,
                    cancellationToken);

            if (!executorIsEnabled)
            {
                throw new InvalidOperationException(
                    "The automation executor does not exist or is disabled.");
            }
        }


        var handlerKey = $"{then.TargetSystem}:{then.Type}";
        if (!_handlers.TryGetValue(
                handlerKey,
                out var handler))
        {
            throw new NotSupportedException(
                $"Unsupported action type '{then.Type}' " +
                $"for target system '{then.TargetSystem}'.");
        }

        await _rateLimiter.WaitAsync(
            new ActionRateLimitKey(
                CompanyId: context.Event.CompanyId,
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