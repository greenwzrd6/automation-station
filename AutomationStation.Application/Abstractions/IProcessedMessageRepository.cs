namespace AutomationStation.Application.Abstractions
{
    public interface IProcessedMessageRepository
    {
        Task<bool> HasProcessedAsync(
            Guid messageId,
            CancellationToken cancellationToken);

        Task MarkProcessedAsync(
            Guid messageId,
            CancellationToken cancellationToken);
    }
}
