using AutomationStation.Application.Models;

namespace AutomationStation.Application.Abstractions
{
    /// <summary>
    /// Provides access to the stored automation executors, which identify the company an automation's actions are
    /// performed on behalf of and whether that executor is currently allowed to run.
    /// </summary>
    /// <remarks>Executors are looked up when a TOJ-targeted action is dispatched so the company can be validated and
    /// used for rate limiting. A missing executor is reported as <see langword="null"/> rather than an exception so
    /// the caller decides how to fail.</remarks>
    public interface IAutomationExecutorRepository
    {
        /// <summary>
        /// Retrieves the executor with the specified identifier.
        /// </summary>
        /// <param name="executorId">Identifier of the executor to retrieve.</param>
        /// <param name="cancellationToken">A token used to cancel the database lookup.</param>
        /// <returns>A task that contains the executor when one exists, or <see langword="null"/> when no executor with
        /// that identifier is stored.</returns>
        Task<AutomationExecutor?> GetByIdAsync(
            Guid executorId,
            CancellationToken cancellationToken);
    }
}