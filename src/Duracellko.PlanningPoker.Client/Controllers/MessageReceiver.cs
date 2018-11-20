using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class MessageReceiver
    {
        private readonly IPlanningPokerClient _planningPokerClient;

        public MessageReceiver(IPlanningPokerClient planningPokerClient)
        {
            _planningPokerClient = planningPokerClient ?? throw new ArgumentNullException(nameof(planningPokerClient));
        }

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

            private async Task<bool> ReceiveMessages(CancellationToken cancellationToken)
            {
                try
                {
                    var messages = await _planningPokerClient.GetMessages(
                        _planningPokerController.TeamName,
                        _planningPokerController.User.Name,
                        _planningPokerController.LastMessageId,
                        cancellationToken);
                    _planningPokerController.ProcessMessages(messages);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
        }
    }
}
