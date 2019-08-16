using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Storage of <see cref="MemberCredentials"/>. Credentials are stored in cookie.
    /// </summary>
    public class MemberCredentialsStore : IMemberCredentialsStore
    {
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberCredentialsStore"/> class.
        /// </summary>
        /// <param name="jsInterop">JavaScript runtime to execute JavaScript functions.</param>
        public MemberCredentialsStore(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        }

        /// <summary>
        /// Loads member credentials from the store.
        /// </summary>
        /// <returns>Loaded <see cref="MemberCredentials"/> instance.</returns>
        public async Task<MemberCredentials> GetCredentialsAsync()
        {
            try
            {
                var credentialsString = await _jsRuntime.InvokeAsync<string>("Duracellko.PlanningPoker.getMemberCredentials");
                if (string.IsNullOrEmpty(credentialsString))
                {
                    return null;
                }
                else
                {
                    return JsonSerializer.Deserialize<MemberCredentials>(credentialsString);
                }
            }
            catch (Exception)
            {
                // Ignore exception. User can connect manually.
                return null;
            }
        }

        /// <summary>
        /// Saves member credentials into the store.
        /// </summary>
        /// <param name="credentials"><see cref="MemberCredentials"/> object to be saved.</param>
        /// <returns>Asynchronous operation.</returns>
        public async Task SetCredentialsAsync(MemberCredentials credentials)
        {
            try
            {
                var credentialsString = credentials != null ? JsonSerializer.Serialize(credentials) : null;
                await _jsRuntime.InvokeAsync<object>("Duracellko.PlanningPoker.setMemberCredentials", credentialsString);
            }
            catch (Exception)
            {
                // Ignore exception. User can connect manually.
            }
        }
    }
}
