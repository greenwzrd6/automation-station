using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Application.Abstractions
{
    public interface IActionCatalog
    {
        bool TryGet(
            string actionType,
            TargetSystem targetSystem,
            out ActionCatalogEntry entry);
    }
}
