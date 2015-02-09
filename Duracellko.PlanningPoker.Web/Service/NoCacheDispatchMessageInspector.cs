// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Web;

namespace Duracellko.PlanningPoker.Web.Service
{
    /// <summary>
    /// Dispatch message inspector that adds no-cache header to HTTP response of a service.
    /// </summary>
    public class NoCacheDispatchMessageInspector : System.ServiceModel.Dispatcher.IDispatchMessageInspector
    {
        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// The object used to correlate state. This object is passed back in the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.BeforeSendReply(System.ServiceModel.Channels.Message@,System.Object)"/> method.
        /// </returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // do nothing
            return null;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)"/> method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var webOperationContext = WebOperationContext.Current;
            if (webOperationContext != null)
            {
                webOperationContext.OutgoingResponse.Headers.Add("Cache-Control", "no-cache");
                webOperationContext.OutgoingResponse.Headers.Add("Pragma", "no-cache");
            }
        }
    }
}