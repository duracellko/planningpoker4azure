using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Storage of <see cref="MemberCredentials"/>. Credentials are stored in cookie.
    /// </summary>
    public class MemberCredentialsStore : IMemberCredentialsStore
    {
        /// <summary>
        /// Loads member credentials from the store.
        /// </summary>
        /// <returns>Loaded <see cref="MemberCredentials"/> instance.</returns>
        public async Task<MemberCredentials> GetCredentialsAsync()
        {
            try
            {
                var credentialsString = await JSRuntime.Current.InvokeAsync<string>("Duracellko.PlanningPoker.getMemberCredentials");
                if (string.IsNullOrEmpty(credentialsString))
                {
                    return null;
                }
                else
                {
                    return Json.Deserialize<MemberCredentials>(credentialsString);
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
                var credentialsString = credentials != null ? Json.Serialize(credentials) : null;
                await JSRuntime.Current.InvokeAsync<object>("Duracellko.PlanningPoker.setMemberCredentials", credentialsString);
            }
            catch (Exception)
            {
                // Ignore exception. User can connect manually.
            }
        }
    }
}
