using System.Threading.Tasks;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Storage of <see cref="MemberCredentials"/>.
/// </summary>
public interface IMemberCredentialsStore
{
    /// <summary>
    /// Loads member credentials from the store.
    /// </summary>
    /// <param name="permanentScope">Specifies, whether to get credentials from permanent scope or session (browser tab) only.</param>
    /// <returns>Loaded <see cref="MemberCredentials"/> instance.</returns>
    Task<MemberCredentials?> GetCredentialsAsync(bool permanentScope);

    /// <summary>
    /// Saves member credentials into the store.
    /// </summary>
    /// <param name="credentials"><see cref="MemberCredentials"/> object to be saved.</param>
    /// <returns>Asynchronous operation.</returns>
    Task SetCredentialsAsync(MemberCredentials? credentials);
}
