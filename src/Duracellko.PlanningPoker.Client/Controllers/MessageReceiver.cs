using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Receives messages from server and sends them to <see cref="PlanningPokerController"/>.
    /// </summary>
    public class MessageReceiver
    {
        private readonly IPlanningPokerClient _planningPokerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceiver"/> class.
        /// </summary>
        /// <param name="planningPokerClient">Planning poker client to load messages from server.</param>
        public MessageReceiver(IPlanningPokerClient planningPokerClient)
        {
            _planningPokerClient = planningPokerClient ?? throw new ArgumentNullException(nameof(planningPokerClient));
        }

        /// <summary>
        /// Starts process of receiving messages from server.
        /// </summary>
        /// <param name="planningPokerController">Instance of <see cref="PlanningPokerController"/> to send messages to.</param>
        /// <returns><see cref="IDisposable"/> object that can be used to stop receiving of messages.</returns>
        public IDisposable StartReceiving(PlanningPokerController planningPokerController)
        {
            var result = new MessageController(planningPokerController, _planningPokerClient);
            result.StartReceiving();
            return result;
        }

        private sealed class MessageController : IDisposable
        {
            private readonly PlanningPokerController _planningPokerController;
            private readonly IPlanningPokerClient _planningPokerClient;
            private CancellationTokenSource _cancellationTokenSource;

            public MessageController(PlanningPokerController planningPokerController, IPlanningPokerClient planningPokerClient)
            {
                _planningPokerController = planningPokerController;
                _planningPokerClient = planningPokerClient;
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

            public async void StartReceiving()
            {
                try
                {
                    using (_cancellationTokenSource = new CancellationTokenSource())
                    {
                        var cancellationToken = _cancellationTokenSource.Token;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var success = await ReceiveMessages(cancellationToken);
                            await Task.Delay(success ? 100 : 500, cancellationToken);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignore excpetion. Job was stopped regularly.
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            private async Task<bool> ReceiveMessages(CancellationToken cancellationToken)
            {
                try
                {
                    var messages = await _planningPokerClient.GetMessages(
                        _planningPokerController.TeamName,
                        _planningPokerController.User.Name,
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
