using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    public interface IActionContext
    {
        Guid CausationEventId { get; }
        Actor Actor { get; }
    }
}
