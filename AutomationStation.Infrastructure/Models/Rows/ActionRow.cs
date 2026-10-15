using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    public sealed record ActionRow
    {
        public Guid AutomationId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public TargetSystem? TargetSystem { get; set; } = null;
        public string? ConfigurationJson { get; set; } = string.Empty;
        public int ExecutionOrder { get; set; }
        public Guid? ExecutorId { get; set; }
    }
}
