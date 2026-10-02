using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations
{
    public sealed record Then(
        string Type,
        TargetSystem TargetSystem,
        IReadOnlyDictionary<string, string> Parameters);
}
