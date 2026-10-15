using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Thens;

namespace AutomationStation.Application.Abstractions
{
    public interface IActionDispatcher
    {
        Task ExecuteAsync(
            Then then,
            ActionContext context,
            CancellationToken cancellationToken);
    }
}