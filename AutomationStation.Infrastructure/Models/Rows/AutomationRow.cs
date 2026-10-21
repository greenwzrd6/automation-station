namespace AutomationStation.Infrastructure.Models.Rows
{
    /// <summary>
    /// Record <c>AutomationRow</c> is a data-access row type mapped from the Automations database table.
    /// </summary>
    /// <remarks>Read with Dapper as the first result set when loading enabled automations for an event type; the
    /// columns of each row are combined with the corresponding trigger, condition, and action rows to rebuild the
    /// domain <c>Automation</c> objects.</remarks>
    /// <param name="Id">The automation's unique identifier.</param>
    /// <param name="Name">The automation's display name.</param>
    /// <param name="Enabled">Whether the automation is enabled in the database.</param>
    /// <param name="AutomationTriggerable">Whether events caused by other automations may trigger this automation.
    /// </param>
    internal sealed record AutomationRow(
        Guid Id,
        string Name,
        bool Enabled,
        bool AutomationTriggerable);
}