﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Service;

/// <summary>
/// Objects provides operations of Planning Poker service.
/// </summary>
public class PlanningPokerClient : IPlanningPokerClient
{
    private const string BaseUri = "api/PlanningPokerService/";

    private readonly HttpClient _client;
    private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningPokerClient"/> class.
    /// </summary>
    /// <param name="client">HttpClient used for HTTP communication with server.</param>
    public PlanningPokerClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Creates new Scrum team with specified team name and Scrum master name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="scrumMasterName">Name of the Scrum master.</param>
    /// <param name="deck">Selected deck of estimation cards to use in the team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Created Scrum team.
    /// </returns>
    public async Task<TeamResult> CreateTeam(string teamName, string scrumMasterName, Deck deck, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedScrumMasterName = _urlEncoder.Encode(scrumMasterName);
        var uri = $"CreateTeam?teamName={encodedTeamName}&scrumMasterName={encodedScrumMasterName}&deck={deck}";

        var result = await GetJsonAsync<TeamResult>(uri, cancellationToken);

        ScrumTeamMapper.ConvertScrumTeam(result.ScrumTeam);
        return result;
    }

    /// <summary>
    /// Connects member or observer with specified name to the Scrum team with specified name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member or observer.</param>
    /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// The Scrum team the member or observer joined to.
    /// </returns>
    public async Task<TeamResult> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var encodedAsObserver = asObserver.ToString(CultureInfo.InvariantCulture);
        var uri = $"JoinTeam?teamName={encodedTeamName}&memberName={encodedMemberName}&asObserver={encodedAsObserver}";

        var result = await GetJsonAsync<TeamResult>(uri, cancellationToken);

        ScrumTeamMapper.ConvertScrumTeam(result.ScrumTeam);
        return result;
    }

    /// <summary>
    /// Reconnects member with specified name to the Scrum team with specified name.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// The Scrum team the member or observer reconnected to.
    /// </returns>
    /// <remarks>
    /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
    /// </remarks>
    public async Task<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var uri = $"ReconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

        var result = await GetJsonAsync<ReconnectTeamResult>(uri, cancellationToken);

        ScrumTeamMapper.ConvertScrumTeam(result.ScrumTeam);
        ScrumTeamMapper.ConvertEstimation(result.SelectedEstimation);
        return result;
    }

    /// <summary>
    /// Disconnects member from the Scrum team.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task DisconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var uri = $"DisconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Signal from Scrum master to start the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task StartEstimation(string teamName, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var uri = $"StartEstimation?teamName={encodedTeamName}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Signal from Scrum master to cancel the estimation.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task CancelEstimation(string teamName, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var uri = $"CancelEstimation?teamName={encodedTeamName}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Submits the estimation for specified team member.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="estimation">The estimation the member is submitting.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    [SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Condition has 3 branches.")]
    public Task SubmitEstimation(string teamName, string memberName, double? estimation, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        string encodedEstimation;
        if (!estimation.HasValue)
        {
            encodedEstimation = "-1111111";
        }
        else if (double.IsPositiveInfinity(estimation.Value))
        {
            encodedEstimation = "-1111100";
        }
        else
        {
            encodedEstimation = _urlEncoder.Encode(estimation.Value.ToString(CultureInfo.InvariantCulture));
        }

        var uri = $"SubmitEstimation?teamName={encodedTeamName}&memberName={encodedMemberName}&estimation={encodedEstimation}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Changes deck of estimation cards, if estimation is not in progress.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="deck">New deck of estimation cards to use in the team.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task ChangeDeck(string teamName, Deck deck, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var uri = $"ChangeDeck?teamName={encodedTeamName}&deck={deck}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Starts countdown timer for team with specified duration.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="duration">Duration of countdown timer.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task StartTimer(string teamName, string memberName, TimeSpan duration, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var encodedDuration = _urlEncoder.Encode(duration.TotalSeconds.ToString(CultureInfo.InvariantCulture));
        var uri = $"StartTimer?teamName={encodedTeamName}&memberName={encodedMemberName}&duration={encodedDuration}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Stops active countdown timer.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// Asynchronous operation.
    /// </returns>
    public Task CancelTimer(string teamName, string memberName, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var uri = $"CancelTimer?teamName={encodedTeamName}&memberName={encodedMemberName}";

        return SendAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Begins to get messages of specified member asynchronously.
    /// </summary>
    /// <param name="teamName">Name of the Scrum team.</param>
    /// <param name="memberName">Name of the member.</param>
    /// <param name="sessionId">The session ID for receiving messages.</param>
    /// <param name="lastMessageId">ID of last message the member received.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// List of messages.
    /// </returns>
    public async Task<IList<Message>> GetMessages(string teamName, string memberName, Guid sessionId, long lastMessageId, CancellationToken cancellationToken)
    {
        var encodedTeamName = _urlEncoder.Encode(teamName);
        var encodedMemberName = _urlEncoder.Encode(memberName);
        var encodedSessionId = _urlEncoder.Encode(sessionId.ToString(null, CultureInfo.InvariantCulture));
        var encodedLastMessageId = _urlEncoder.Encode(lastMessageId.ToString(CultureInfo.InvariantCulture));
        var uri = $"GetMessages?teamName={encodedTeamName}&memberName={encodedMemberName}&sessionId={encodedSessionId}&lastMessageId={encodedLastMessageId}";

        try
        {
            var result = await GetJsonAsync<List<Message>>(uri, cancellationToken);

            ScrumTeamMapper.ConvertMessages(result);
            return result;
        }
        catch (PlanningPokerException ex)
        {
            if (ex.InnerException == null)
            {
                throw new UserDisconnectedException();
            }

            if (ex.InnerException is HttpRequestException httpRequestException && httpRequestException.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserDisconnectedException();
            }

            throw;
        }
    }

    /// <summary>
    /// Gets information about current time of service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>Current time of service in UTC time zone.</returns>
    public async Task<TimeResult> GetCurrentTime(CancellationToken cancellationToken)
    {
        var uri = "GetCurrentTime";
        return await GetJsonAsync<TimeResult>(uri, cancellationToken);
    }

    private async Task<T> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        throw ScrumTeamMapper.GetPlanningPokerException(content);
                    }

                    response.EnsureSuccessStatusCode();

                    if (response.Content == null)
                    {
                        throw new PlanningPokerException(UIResources.PlanningPokerService_UnexpectedError);
                    }

                    var result = await response.Content.ReadFromJsonAsync<T>(default(JsonSerializerOptions), cancellationToken);
                    return result ?? throw new PlanningPokerException(UIResources.PlanningPokerService_UnexpectedError);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            throw new PlanningPokerException(UIResources.PlanningPokerService_ConnectionError, ex);
        }
    }

    private async Task SendAsync(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
            {
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        throw ScrumTeamMapper.GetPlanningPokerException(content);
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        throw new PlanningPokerException(UIResources.PlanningPokerService_UnexpectedError);
                    }
                }
            }
        }
        catch (HttpRequestException ex)
        {
            throw new PlanningPokerException(UIResources.PlanningPokerService_ConnectionError, ex);
        }
    }
}
