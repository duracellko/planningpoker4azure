using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Objects provides operations of Planning Poker service using SignalR.
    /// </summary>
    public sealed class PlanningPokerSignalRClient : IPlanningPokerClient, IDisposable
    {
        private const string ServiceUri = "signalr/PlanningPoker";

        private readonly IPlanningPokerUriProvider _uriProvider;

        private HubConnection _hubConnection;
        private bool _disposed;
        private TaskCompletionSource<IList<Message>> _getMessagesTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerSignalRClient"/> class.
        /// </summary>
        /// <param name="uriProvider">URI provider that provides URI of Planning Poker service.</param>
        public PlanningPokerSignalRClient(IPlanningPokerUriProvider uriProvider)
        {
            _uriProvider = uriProvider ?? throw new ArgumentNullException(nameof(uriProvider));
        }

        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        public async Task<ScrumTeam> CreateTeam(string teamName, string scrumMasterName, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);
            var result = await _hubConnection.InvokeAsync<ScrumTeam>("CreateTeam", teamName, scrumMasterName, cancellationToken);

            ConvertScrumTeam(result);
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
        public async Task<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);
            var result = await _hubConnection.InvokeAsync<ScrumTeam>("JoinTeam", teamName, memberName, asObserver, cancellationToken);

            ConvertScrumTeam(result);
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
            await EnsureConnected(cancellationToken);
            var result = await _hubConnection.InvokeAsync<ReconnectTeamResult>("ReconnectTeam", teamName, memberName, cancellationToken);

            ConvertScrumTeam(result.ScrumTeam);
            ConvertEstimation(result.SelectedEstimation);
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
        public async Task DisconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);
            await _hubConnection.InvokeAsync("DisconnectTeam", teamName, memberName, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to starts the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public async Task StartEstimation(string teamName, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);
            await _hubConnection.InvokeAsync("StartEstimation", teamName, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to cancels the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public async Task CancelEstimation(string teamName, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);
            await _hubConnection.InvokeAsync("CancelEstimation", teamName, cancellationToken);
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
        public async Task SubmitEstimation(string teamName, string memberName, double? estimation, CancellationToken cancellationToken)
        {
            double encodedEstimation = estimation ?? -1111111;
            if (double.IsPositiveInfinity(encodedEstimation))
            {
                encodedEstimation = -1111100;
            }

            await EnsureConnected(cancellationToken);
            await _hubConnection.InvokeAsync("SubmitEstimation", teamName, memberName, encodedEstimation, cancellationToken);
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// List of messages.
        /// </returns>
        public async Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId, CancellationToken cancellationToken)
        {
            await EnsureConnected(cancellationToken);

            try
            {
                if (_getMessagesTask != null)
                {
                    throw new InvalidOperationException("GetMessages is already in progress.");
                }

                _getMessagesTask = new TaskCompletionSource<IList<Message>>();

                await _hubConnection.InvokeAsync("GetMessages", teamName, memberName, lastMessageId, cancellationToken);

                var result = await _getMessagesTask.Task;
                ConvertMessages(result);
                return result;
            }
            finally
            {
                _getMessagesTask = null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private static void ConvertScrumTeam(ScrumTeam scrumTeam)
        {
            if (scrumTeam.AvailableEstimations != null)
            {
                ConvertEstimations(scrumTeam.AvailableEstimations);
            }

            if (scrumTeam.EstimationResult != null)
            {
                ConvertEstimations(scrumTeam.EstimationResult);
            }
        }

        private static void ConvertEstimations(IEnumerable<Estimation> estimations)
        {
            foreach (var estimation in estimations)
            {
                ConvertEstimation(estimation);
            }
        }

        private static void ConvertEstimations(IEnumerable<EstimationResultItem> estimationResultItems)
        {
            foreach (var estimationResultItem in estimationResultItems)
            {
                ConvertEstimation(estimationResultItem.Estimation);
            }
        }

        private static void ConvertEstimation(Estimation estimation)
        {
            if (estimation != null && estimation.Value == Estimation.PositiveInfinity)
            {
                estimation.Value = double.PositiveInfinity;
            }
        }

        private static void ConvertMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                ConvertMessage(message);
            }
        }

        private static void ConvertMessage(Message message)
        {
            if (message.Type == MessageType.EstimationEnded)
            {
                var estimationResultMessage = (EstimationResultMessage)message;
                ConvertEstimations(estimationResultMessage.EstimationResult);
            }
        }

        private void OnNotify(IList<Message> messages)
        {
            if (_getMessagesTask != null)
            {
                _getMessagesTask.SetResult(messages);
            }
        }

        private HubConnection CreateHubConnection()
        {
            return new HubConnectionBuilder()
                .WithUrl(new Uri(_uriProvider.BaseUri, ServiceUri))
                .AddNewtonsoftJsonProtocol()
                .Build();
        }

        private async Task EnsureConnected(CancellationToken cancellationToken)
        {
            CheckDisposed();

            if (_hubConnection == null)
            {
                _hubConnection = CreateHubConnection();
                _hubConnection.On<IList<Message>>("Notify", OnNotify);

                await _hubConnection.StartAsync(cancellationToken);
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PlanningPokerSignalRClient));
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_hubConnection != null)
                    {
                        _hubConnection.DisposeAsync().Wait();
                        _hubConnection = null;
                    }
                }

                _disposed = true;
            }
        }
    }
}
