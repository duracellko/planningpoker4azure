using System.Net;
using System.Net.Sockets;

namespace Duracellko.PlanningPoker.Client.Test.MockSignalR;

internal sealed class MockEndPoint : EndPoint
{
    public override AddressFamily AddressFamily => AddressFamily.Unspecified;

    public override EndPoint Create(SocketAddress socketAddress)
    {
        return new MockEndPoint();
    }

    public override SocketAddress Serialize()
    {
        return new SocketAddress(AddressFamily.Unspecified, 2);
    }
}
