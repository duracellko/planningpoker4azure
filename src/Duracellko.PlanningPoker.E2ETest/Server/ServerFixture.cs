using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;

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

        public IWebHost WebHost { get; private set; }

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
                    var address = WebHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single();
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

            var builder = Program.CreateWebHostBuilder(GetProgramArguments());
            WebHost = builder.Build();
            RunInBackgroundThread(WebHost.Start);
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            if (WebHost != null)
            {
                await WebHost.StopAsync();
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
                        Stop().Wait();
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
            return new string[]
            {
                "--urls", "http://127.0.0.1:5000",
                "--PlanningPokerClient:UseServerSide", UseServerSide.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
