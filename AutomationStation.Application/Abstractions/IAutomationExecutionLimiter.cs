using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Limits how often a single automation may execute for one incoming event correlation.
    /// </summary>
    /// <remarks>Guards against feedback loops in which an automation reacts to events caused by its own actions:
    /// every accepted execution is recorded with the automation id and the correlation id of the event being
    /// processed, and later attempts for the same pair inside the rolling window are rejected once the limit is
    /// reached. A rejected attempt records nothing, so concurrent callers cannot consume each other's budget by
    /// racing. Implementations are expected to make the check and the insert atomic.</remarks>
    public interface IAutomationExecutionLimiter
    {
        /// <summary>
        /// Attempts to record an execution of the automation described by the evaluation context.
        /// </summary>
        /// <remarks>Callers invoke this before dispatching an automation's actions and treat a false result as a
        /// signal to skip the automation for this event. When the attempt is accepted, the history row is written
        /// as part of the same atomic operation as the limit check; when the limit is already reached, no row is
        /// written and the existing history is left untouched.</remarks>
        /// <param name="context">Identifies the automation being executed together with the correlation, causation
        /// and actor information of the triggering event.</param>
        /// <param name="cancellationToken">A token used to cancel the underlying storage operation.</param>
        /// <returns>true when the execution was recorded and the automation may proceed; false when the execution
        /// limit for this automation and correlation has been reached and the automation must be skipped.</returns>
        /// <exception cref="OperationCanceledException">
        /// The operation is canceled through <paramref name="cancellationToken"/>.
        /// </exception>
        Task<bool> TryRecordExecutionAsync(
            EvaluationContext context,
            CancellationToken cancellationToken);
    }
}