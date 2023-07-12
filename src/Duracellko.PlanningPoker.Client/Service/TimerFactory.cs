using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Timers;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Factory object that can start periodic invocation of specific action.
    /// </summary>
    public class TimerFactory : ITimerFactory
    {
        private readonly TimeSpan _interval;
        private Func<Action, Task>? _dispatcherDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerFactory" /> class.
        /// </summary>
        /// <param name="interval">The interval of created timers.</param>
        public TimerFactory(TimeSpan interval)
        {
            _interval = interval;
        }

        /// <summary>
        /// Starts periodic invocation of specified action.
        /// </summary>
        /// <param name="action">The action to invoke periodically.</param>
        /// <returns>The disposable object that should be disposed to stop the timer.</returns>
        public IDisposable StartTimer(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var dispatcherDelegate = _dispatcherDelegate;
            if (dispatcherDelegate == null)
            {
                throw new InvalidOperationException(Resources.Error_TimerStartedWithoutDispatcher);
            }

            return new DisposableTimer(action, dispatcherDelegate, _interval);
        }

        /// <summary>
        /// Sets delegate that can invoke action on Dispatcher.
        /// </summary>
        /// <param name="dispatcherDelegate">The delegate to invoke action on Dispatcher.</param>
        internal void SetDispatcherDelegate(Func<Action, Task>? dispatcherDelegate)
        {
            _dispatcherDelegate = dispatcherDelegate;
        }

        private sealed class DisposableTimer : IDisposable
        {
            private readonly Action _action;
            private readonly Func<Action, Task> _dispatcherDelegate;
            private Timer? _timer;

            public DisposableTimer(Action action, Func<Action, Task> dispatcherDelegate, TimeSpan interval)
            {
                _action = action;
                _dispatcherDelegate = dispatcherDelegate;

                var timer = new Timer(interval.TotalMilliseconds);
                _timer = timer;
                timer.Elapsed += TimerOnElapsed;
                timer.Start();
            }

            public void Dispose()
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }

            [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Log timer event errors.")]
            private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
            {
                try
                {
                    await _dispatcherDelegate(_action);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
