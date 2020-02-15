using System;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.MockSignalR;
using Duracellko.PlanningPoker.Service;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Service
{
    [TestClass]
    public class PlanningPokerSignalRClientTest
    {
        [TestMethod]
        public async Task CreateTeam_TeamNameAndScrumMaster_HubMessageIsSent()
        {
            using var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            using var hubConnection = new MockHubConnection();
            using var sentMessages = new HubMessageQueue(hubConnection.SentMessages);
            using var target = new PlanningPokerSignalRClient(hubConnection.HubConnectionBuilder);

            var resultTask = target.CreateTeam("My Team", "Master", timeoutToken.Token);

            var sentMessage = await sentMessages.GetNextAsync();
            Assert.IsNotNull(sentMessage);
            Assert.IsInstanceOfType(sentMessage, typeof(InvocationMessage));
            var sentInvocationMessage = (InvocationMessage)sentMessage;
            Assert.AreEqual("CreateTeam", sentInvocationMessage.Target);
            Assert.AreEqual(2, sentInvocationMessage.Arguments.Length);
            Assert.AreEqual("My Team", sentInvocationMessage.Arguments[0]);
            Assert.AreEqual("Master", sentInvocationMessage.Arguments[1]);

            var scrumTeam = new ScrumTeam
            {
                Name = "My Team",
                ScrumMaster = new TeamMember { Type = "ScrumMaster", Name = "Master" }
            };
            var returnMessage = new CompletionMessage(sentInvocationMessage.InvocationId, null, scrumTeam, true);
            await hubConnection.ReceiveMessage(returnMessage, timeoutToken.Token);

            var result = await resultTask;

            Assert.IsNotNull(result);
            Assert.AreEqual("My Team", result.Name);
            Assert.AreEqual("Master", result.ScrumMaster.Name);
        }
    }
}
