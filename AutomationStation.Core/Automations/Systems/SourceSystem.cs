namespace AutomationStation.Core.Automations.Systems
{
    /// <summary>
    /// Enum <c>SourceSystem</c> specifies the supported systems that can produce integration events.
    /// </summary>
    /// <remarks>The value is stored on every integration event and matched against the trigger of an automation, so an
    /// automation only fires for events originating from the system named by its trigger.</remarks>
    public enum SourceSystem
    {
        Kanban,
        TojSystem,
    }
}