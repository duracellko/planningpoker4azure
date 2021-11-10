using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;

namespace Duracellko.PlanningPoker.E2ETest.Server
{
    public class ServerFixture : IDisposable
    {
        private bool _disposed;
        private Uri _uri;

        ~ServerFixture()
        {
            Dispose(false);
        }

        public bool UseServerSide { get; set; }

        public bool UseHttpClient { get; set; }

        public IHost WebHost { get; private set; }

        public Uri Uri
        {
            get
            {
                if (WebHost == null)
                {
                    return null;
                }

                if (_uri == null)
                {
                    var server = (IServer)WebHost.Services.GetService(typeof(IServer));
                    var address = server.Features.Get<IServerAddressesFeature>().Addresses.Single();
                    _uri = new Uri(address);
                }

                return _uri;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task Start()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ServerFixture));
            }

            if (WebHost != null)
            {
                throw new InvalidOperationException("WebHost is already started.");
            }

            WebHost = Program.CreateWebApplication(GetProgramArguments());
            RunInBackgroundThread(WebHost.Start);
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            if (WebHost != null)
            {
                try
                {
                    await WebHost.StopAsync();
                }
                catch (TaskCanceledException)
                {
                    // Ignore time out error
                }

                WebHost = null;
                _uri = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    RunInBackgroundThread(() =>
                    {
                        try
                        {
                            Stop().Wait();
                        }
                        catch (AggregateException ex)
                            when (ex.InnerException != null && ex.InnerException is TaskCanceledException)
                        {
                            // Ignore time out error
                        }
                    });
                }

                _disposed = true;
            }
        }

        private static void RunInBackgroundThread(Action action)
        {
            using (var isDone = new ManualResetEvent(false))
            {
                new Thread(() =>
                {
                    action();
                    isDone.Set();
                }).Start();

                isDone.WaitOne();
            }
        }

        private string[] GetProgramArguments()
        {
            var useServerSideValue = UseServerSide ? "Always" : "Never";

            // Use Development environment, so that static web content is served without building project with publish.
            return new string[]
            {
                "--urls", "http://127.0.0.1:0",
                "--environment", "Development",
                "--PlanningPokerClient:UseServerSide", useServerSideValue,
                "--PlanningPokerClient:UseHttpClient", UseHttpClient.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
