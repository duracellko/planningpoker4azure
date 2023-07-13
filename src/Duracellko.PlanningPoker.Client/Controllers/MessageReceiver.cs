using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Receives messages from server and sends them to <see cref="PlanningPokerController"/>.
    /// </summary>
    public class MessageReceiver
    {
        private readonly IPlanningPokerClient _planningPokerClient;
        private readonly IServiceTimeProvider _serviceTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceiver"/> class.
        /// </summary>
        /// <param name="planningPokerClient">Planning poker client to load messages from server.</param>
        /// <param name="serviceTimeProvider">Service to update time from server.</param>
        public MessageReceiver(IPlanningPokerClient planningPokerClient, IServiceTimeProvider serviceTimeProvider)
        {
            _planningPokerClient = planningPokerClient ?? throw new ArgumentNullException(nameof(planningPokerClient));
            _serviceTimeProvider = serviceTimeProvider ?? throw new ArgumentNullException(nameof(serviceTimeProvider));
        }

        /// <summary>
        /// Starts process of receiving messages from server.
        /// </summary>
        /// <param name="planningPokerController">Instance of <see cref="PlanningPokerController"/> to send messages to.</param>
        /// <returns><see cref="IDisposable"/> object that can be used to stop receiving of messages.</returns>
        public IDisposable StartReceiving(PlanningPokerController planningPokerController)
        {
            var result = new MessageController(planningPokerController, _planningPokerClient, _serviceTimeProvider);
            result.StartReceiving();
            return result;
        }

        private sealed class MessageController : IDisposable
        {
            private readonly PlanningPokerController _planningPokerController;
            private readonly IPlanningPokerClient _planningPokerClient;
            private readonly IServiceTimeProvider _serviceTimeProvider;

            [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "CancellationToken is disposed, when task ends.")]
            private CancellationTokenSource? _cancellationTokenSource;

            public MessageController(PlanningPokerController planningPokerController, IPlanningPokerClient planningPokerClient, IServiceTimeProvider serviceTimeProvider)
            {
                _planningPokerController = planningPokerController;
                _planningPokerClient = planningPokerClient;
                _serviceTimeProvider = serviceTimeProvider;
            }

            public void Dispose()
            {
                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore, when CancellationToken is disposed.
                    _cancellationTokenSource = null;
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignore error, when application is closing.")]
            [SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"", Justification = "Task life time is controlled by controller, not consumer.")]
            public async void StartReceiving()
            {
                try
                {
                    using (_cancellationTokenSource = new CancellationTokenSource())
                    {
                        var cancellationToken = _cancellationTokenSource.Token;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            await _serviceTimeProvider.UpdateServiceTimeOffset(cancellationToken);
                            var success = await ReceiveMessages(cancellationToken);
                            await Task.Delay(success ? 100 : 500, cancellationToken);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore exception. Job was stopped regularly.
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            private async Task<bool> ReceiveMessages(CancellationToken cancellationToken)
            {
                if (_planningPokerController.TeamName == null || _planningPokerController.User == null)
                {
                    throw new TaskCanceledException();
                }

                try
                {
                    var messages = await _planningPokerClient.GetMessages(
                        _planningPokerController.TeamName,
                        _planningPokerController.User.Name,
                        _planningPokerController.SessionId,
                        _planningPokerController.LastMessageId,
                        cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    _planningPokerController.ProcessMessages(messages);
                    return true;
                }
                catch (PlanningPokerException ex) when (ex.InnerException != null)
                {
                    // Network connection failed. Try again.
                    return false;
                }
            }
        }
    }
}
