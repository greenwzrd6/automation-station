using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations.Thens
{
    /// <summary>
    /// Record <c>Then</c> represents what should happen when a <see cref="Whens.When"/> is met. 
    /// </summary>
    /// <param name="Type">The type of action to perform, such as "CreatePlacement".</param>
    /// <param name="TargetSystem">The system where the action should happen, see <see cref="Systems.TargetSystem"/>.</param>
    /// <param name="ExecutorId">The optional id of the executor used to perform the action.</param>
    /// <param name="Parameters">The values needed to do the action, usually GUIDs.</param>
    public sealed record Then(
        string Type,
        TargetSystem TargetSystem,
        Guid? ExecutorId,
        IReadOnlyDictionary<string, string> Parameters);
}
