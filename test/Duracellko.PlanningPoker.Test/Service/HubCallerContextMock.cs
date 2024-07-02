using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace Duracellko.PlanningPoker.Test.Service;

internal sealed class HubCallerContextMock : HubCallerContext
{
    private string _connectionId = string.Empty;

    public override CancellationToken ConnectionAborted => default(CancellationToken);

    public override string ConnectionId => _connectionId;

    public override IFeatureCollection Features => throw new NotImplementedException();

    public override IDictionary<object, object?> Items => throw new NotImplementedException();

    public override ClaimsPrincipal User => throw new NotImplementedException();

    public override string UserIdentifier => throw new NotImplementedException();

    public override void Abort()
    {
        throw new NotImplementedException();
    }

    public void SetConnectionId(string connectionId)
    {
        _connectionId = connectionId;
    }
}
