namespace AutomationStation.Application.Abstractions
{
    public interface IActionContext
    {
        Guid CausationEventId { get; }
        Guid? SourceAutomationId { get; }
    }
}
