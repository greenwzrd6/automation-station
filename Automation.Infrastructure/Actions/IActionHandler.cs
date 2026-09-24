using Automation.Application.Models;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Actions;

public interface IActionHandler
{
    string ActionType { get; }

    Task ExecuteAsync(
        Then then,
        PlacementActionContext context,
        CancellationToken cancellationToken);
}