using System;

namespace Duracellko.PlanningPoker.Client.UI
{
    /// <summary>
    /// Object provides functionality to display busy indicator during long running operation.
    /// </summary>
    public class BusyIndicatorService : IBusyIndicatorService
    {
        private Action<bool> _handler;
        private int _counter;

        /// <summary>
        /// Displays busy indicator to notify user about running operation.
        /// </summary>
        /// <returns><see cref="IDisposable"/> object that should be disposed, when operation is finished.</returns>
        public IDisposable Show()
        {
            if (_counter == 0)
            {
                _handler?.Invoke(true);
            }

            _counter++;
            return new BusyIndicatorOperation(this);
        }

        /// <summary>
        /// Setup handler function that shows or hides busy indicator in Blazor component.
        /// </summary>
        /// <param name="handler">Handler delegate to show or hide busy indicator.</param>
        public void SetBusyIndicatorHandler(Action<bool> handler)
        {
            _handler = handler;
        }

        private void Hide()
        {
            _counter--;
            if (_counter == 0)
            {
                _handler?.Invoke(false);
            }
        }

        private sealed class BusyIndicatorOperation : IDisposable
        {
            private readonly BusyIndicatorService _service;
            private bool _disposed;

            public BusyIndicatorOperation(BusyIndicatorService service)
            {
                _service = service;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _service.Hide();
                    _disposed = true;
                }
            }
        }
    }
}
