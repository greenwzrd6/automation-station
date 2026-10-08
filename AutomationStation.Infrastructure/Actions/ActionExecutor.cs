using AutomationStation.Core.Automations;
using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Models;

namespace AutomationStation.Infrastructure.Actions;

public sealed class ActionExecutor(
    IEnumerable<IActionHandler> handlers,
    IActionRateLimiter rateLimiter,
    IActionCatalog actionCatalog)
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