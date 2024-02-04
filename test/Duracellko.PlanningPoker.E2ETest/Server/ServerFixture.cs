using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Web;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.E2ETest.Server
{
    public class ServerFixture : IAsyncDisposable, IDisposable
    {
        private bool _disposed;
        private Uri? _uri;

        ~ServerFixture()
        {
            Dispose(false);
        }

        public bool UseServerSide { get; set; }

        public bool UseHttpClient { get; set; }

        public IHost? WebHost { get; private set; }

        public Uri? Uri
        {
            get
            {
                if (WebHost == null)
                {
                    return null;
                }

                if (_uri == null)
                {
                    var server = (IServer)WebHost.Services.GetRequiredService(typeof(IServer));
                    var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
                    if (serverAddressesFeature != null)
                    {
                        var address = serverAddressesFeature.Addresses.Single();
                        _uri = new Uri(address);
                    }
                }

                return _uri;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Start()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (WebHost != null)
            {
                throw new InvalidOperationException("WebHost is already started.");
            }

            WebHost = Program.CreateWebApplication(GetProgramArguments());
            await RunInBackgroundThread(() => WebHost.StartAsync()).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            if (WebHost != null)
            {
                try
                {
                    await WebHost.StopAsync().ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // Ignore time out error
                }

                WebHost = null;
                _uri = null;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!_disposed)
            {
                await Stop().ConfigureAwait(false);
                _disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    RunInBackgroundThread(async () =>
                    {
                        try
                        {
                            await Stop().ConfigureAwait(false);
                        }
                        catch (TaskCanceledException)
                        {
                            // Ignore time out error
                        }
                    }).Wait();
                }

                _disposed = true;
            }
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Forward all exceptions to parent task.")]
        private static Task RunInBackgroundThread(Func<Task> action)
        {
            var isDone = new TaskCompletionSource();
            new Thread(async () =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                    isDone.SetResult();
                }
                catch (Exception ex)
                {
                    isDone.TrySetException(new AggregateException(ex));
                }
            }).Start();

            return isDone.Task;
        }

        private string[] GetProgramArguments()
        {
            var useServerSideValue = UseServerSide ? "Always" : "Never";

            return new string[]
            {
                "--urls", "http://127.0.0.1:0",
                "--applicationName", "Duracellko.PlanningPoker.Web",
                "--PlanningPokerClient:UseServerSide", useServerSideValue,
                "--PlanningPokerClient:UseHttpClient", UseHttpClient.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
