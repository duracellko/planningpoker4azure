using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR
{
    internal sealed class MockConnectionContext : ConnectionContext
    {
        private string _connectionId = Guid.NewGuid().ToString();
        private IDuplexPipe _transport;

        public MockConnectionContext(IDuplexPipe transport)
        {
            _transport = transport;
        }

        public override string ConnectionId
        {
            get => _connectionId;
            set => throw new NotSupportedException();
        }

        public override IDuplexPipe Transport
        {
            get => _transport;
            set => throw new NotSupportedException();
        }

        public override IFeatureCollection Features { get; } = new FeatureCollection();

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Inherited property")]
        public override IDictionary<object, object?> Items { get; set; } = new ConnectionItems();
    }
}
