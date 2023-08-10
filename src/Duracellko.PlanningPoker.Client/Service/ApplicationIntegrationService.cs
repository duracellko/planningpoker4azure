using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// The service to integrate Planning Poker with a 3rd-party application.
    /// This includes sending data like estimation result to the application.
    /// </summary>
    public class ApplicationIntegrationService : IApplicationIntegrationService
    {
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationIntegrationService" /> class.
        /// </summary>
        public ApplicationIntegrationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        /// <summary>
        /// Posts the estimation result to specified application.
        /// </summary>
        /// <param name="estimation">The estimation value to post.</param>
        /// <param name="callbackReference">Reference to the application that should receive the result.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task PostEstimationResult(double estimation, ApplicationCallbackReference callbackReference)
        {
            await _jsRuntime.InvokeAsync<object>("Duracellko.PlanningPoker.postEstimationResult", estimation, callbackReference);
        }
    }
}
