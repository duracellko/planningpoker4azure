using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Storage of <see cref="MemberCredentials"/>. Credentials are stored in browser local storage.
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
    /// <param name="permanentScope">Specifies, whether to get credentials from permanent scope or session (browser tab) only.</param>
    /// <returns>Loaded <see cref="MemberCredentials"/> instance.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore exception. User can connect manually.")]
    public async Task<MemberCredentials?> GetCredentialsAsync(bool permanentScope)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<MemberCredentials?>("Duracellko.PlanningPoker.getMemberCredentials", permanentScope);
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
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore exception. User can connect manually.")]
    public async Task SetCredentialsAsync(MemberCredentials? credentials)
    {
        try
        {
            await _jsRuntime.InvokeAsync<object>("Duracellko.PlanningPoker.setMemberCredentials", credentials);
        }
        catch (Exception)
        {
            // Ignore exception. User can connect manually.
        }
    }
}
