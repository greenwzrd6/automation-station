namespace AutomationStation.Application.Events
{
    /// <summary>
    /// Describes how an incoming integration event was handled: processed for the first time or skipped as a
    /// redelivery.
    /// </summary>
    /// <remarks>Returned by the message handler so callers can log and metric the distinction between first-time
    /// processing and duplicates, which are ignored for idempotency.</remarks>
    public enum MessageOutcome
    {
        Processed,

        Duplicate
    }
}
