using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations.Whens
{
    public sealed record Trigger(
        string EventType, 
        SourceSystem SourceSystem);
}
