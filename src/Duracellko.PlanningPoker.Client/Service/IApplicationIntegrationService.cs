using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// The object provides a service to integrate Planning Poker
    /// with a 3rd-party application. This includes sending data
    /// like estimation result to the application.
    /// </summary>
    public interface IApplicationIntegrationService
    {
        /// <summary>
        /// Posts the estimation result to specified application.
        /// </summary>
        /// <param name="estimation">The estimation value to post.</param>
        /// <param name="callbackReference">Reference to the application that should receive the result.</param>
        /// <returns>Asynchronous operation.</returns>
        Task PostEstimationResult(double estimation, ApplicationCallbackReference callbackReference);
    }
}
