// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Defines operation planning poker service provided for web clients.
    /// </summary>
    [ServiceContract(Name = "PlanningPokerService", Namespace = Namespaces.PlanningPokerService)]
    public interface IPlanningPokerService
    {
        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <returns>Created Scrum team.</returns>
        [OperationContract]
        [WebGet]
        ScrumTeam CreateTeam(string teamName, string scrumMasterName);

        /// <summary>
        /// Connects member or observer with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member or observer.</param>
        /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
        /// <returns>The Scrum team the member or observer joined to.</returns>
        [OperationContract]
        [WebGet]
        ScrumTeam JoinTeam(string teamName, string memberName, bool asObserver);

        /// <summary>
        /// Reconnects member with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>
        /// The Scrum team the member or observer reconnected to.
        /// </returns>
        /// <remarks>
        /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
        /// </remarks>
        [OperationContract]
        [WebGet]
        ReconnectTeamResult ReconnectTeam(string teamName, string memberName);

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        [OperationContract]
        [WebGet]
        void DisconnectTeam(string teamName, string memberName);

        /// <summary>
        /// Signal from Scrum master to starts the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        [OperationContract]
        [WebGet]
        void StartEstimation(string teamName);

        /// <summary>
        /// Signal from Scrum master to cancel the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        [OperationContract]
        [WebGet]
        void CancelEstimation(string teamName);

        /// <summary>
        /// Submits the estimation for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimation">The estimation the member is submitting.</param>
        [OperationContract]
        [WebGet]
        void SubmitEstimation(string teamName, string memberName, double estimation);

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="callback">The callback delegate to call, when the member receives new message.</param>
        /// <param name="asyncState">State object of asynchronous operation.</param>
        /// <returns>The <see cref="T:System.IAsyncResult"/> object representing asynchronous operation.</returns>
        [OperationContract(AsyncPattern = true)]
        [WebGet]
        IAsyncResult BeginGetMessages(string teamName, string memberName, long lastMessageId, AsyncCallback callback, object asyncState);

        /// <summary>
        /// Ends the asynchronous operation of getting messages for specified team member.
        /// </summary>
        /// <param name="ar">The <see cref="T:System.IAsyncResult"/> object.</param>
        /// <returns>Collection of new messages sent to the team member.</returns>
        IList<Message> EndGetMessages(IAsyncResult ar);
    }
}
