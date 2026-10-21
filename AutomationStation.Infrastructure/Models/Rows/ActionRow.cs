using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    /// <summary>
    /// Row model mapping a single stored automation action, including its type, target system, and configuration.
    /// </summary>
    /// <remarks>Mutable record used as a data-transfer shape when reading action rows from the database; property
    /// names align with the corresponding table columns.</remarks>
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
