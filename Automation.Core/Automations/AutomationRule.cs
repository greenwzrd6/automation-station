namespace Automation.Core.Automations
{
    public sealed class AutomationRule(
        Guid id,
        string name,
        bool isEnabled,
        When when,
        IEnumerable<Then> thens)
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
        public bool IsEnabled { get; private set; } = isEnabled;

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
