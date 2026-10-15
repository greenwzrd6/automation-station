using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations
{
    /// <summary>
    /// Record <c>Then</c> represents the structure of what should happen when a <see cref="When"/> is met. 
    /// </summary>
    /// <param name="Type">The action to perform in the target system, such as CreatePlacement.</param>
    /// <param name="TargetSystem">The system where the action should happen. See <see cref="Systems.TargetSystem"/>.</param>
    /// <param name="ExecutorId">The optional id of the executor used to perform the action.</param>
    /// <param name="Parameters">The values needed to do the action, usually GUIDs.</param>
    public sealed record Then(
        string Type,
        TargetSystem TargetSystem,
        Guid? ExecutorId,
        IReadOnlyDictionary<string, string> Parameters);
}
