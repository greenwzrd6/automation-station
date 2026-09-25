
namespace Automation.Infrastructure.Utils
{
    public sealed class CreateAutomationHistory(
        Guid AutomationId)
    {
        public Guid AutomationId { get; } = AutomationId;
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        
    }
}
