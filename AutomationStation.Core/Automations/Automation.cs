namespace AutomationStation.Core.Automations
{

    /// <summary>
    /// Class <c>Automation</c> is the domainclass that represents the structure of an automation.
    /// </summary>
    /// <param name="id">The id of the automation.</param>
    /// <param name="name">The name of the automation.</param>
    /// <param name="isEnabled">Whether the automation is enabled or not.</param>
    /// <param name="automationTriggerable">Whether events caused by other automations are allowed to trigger this automation.</param>
    /// <param name="when">The trigger and conditions defined by <see cref="Automations.When"/>.</param>
    /// <param name="thens">The action(s) to execute, represented by <see cref="Then"/> records.</param>
    public sealed class Automation(
        Guid id,
        string name,
        bool isEnabled,
        bool automationTriggerable,
        When when,
        IEnumerable<Then> thens)
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public bool IsEnabled { get; private set; } = isEnabled;
        public bool AutomationTriggerable { get; } = automationTriggerable;
        public When When { get; } = when;
        public IReadOnlyCollection<Then> Thens { get; } = thens.ToList().AsReadOnly();

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }
    }
}
