
namespace AutomationStation.Infrastructure.Actions;

public interface IActionHandler
{
    string ActionType { get; }

    Task ExecuteAsync(
        Then then,
        AutomationActionContext context,
        CancellationToken cancellationToken);
}