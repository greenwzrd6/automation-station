using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Infrastructure.Models.Rows
{
    /// <summary>
    /// Record <c>ConditionRow</c> is the raw row shape of an automation condition as read from the
    /// AutomationConditions table.
    /// </summary>
    /// <remarks>Dapper maps the conditions result set onto this record; the repository then groups the rows by
    /// automation and deserializes <c>ConfigurationJson</c> into a domain condition, so this type never
    /// leaves the persistence layer. Value-based record equality applies.</remarks>
    /// <param name="AutomationId">Identifier of the automation the condition belongs to.</param>
    /// <param name="ConditionType">Name of the condition kind, stored as free text on the row.</param>
    /// <param name="SourceSystem">The source system recorded for the condition, see <see cref="SourceSystem"/>. The
    /// repository currently maps only <c>ConfigurationJson</c> onto the domain condition, so this value is not carried
    /// over.</param>
    /// <param name="ConfigurationJson">JSON holding the condition's settings; null when the row carries none, which the
    /// repository treats as an error rather than an empty condition.</param>
    internal sealed record ConditionRow(
        Guid AutomationId,
        string ConditionType,
        SourceSystem SourceSystem,
        string? ConfigurationJson);
}
