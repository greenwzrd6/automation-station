namespace AutomationStation.Application.Models
{
    public sealed record AutomationExecutor(
        Guid Id,
        int CompanyId,
        bool Enabled);
}
