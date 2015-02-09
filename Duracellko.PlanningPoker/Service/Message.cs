// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Generic message that can be sent to Scrum team members or observers.
    /// </summary>
    [Serializable]
    [DataContract(Name = "message", Namespace = Namespaces.PlanningPokerData)]
    [KnownType(typeof(MemberMessage))]
    [KnownType(typeof(EstimationResultMessage))]
    public class Message
    {
        /// <summary>
        /// Gets or sets the message ID unique to member, so that member can track which messages he/she already got.
        /// </summary>
        /// <value>The message ID.</value>
        [DataMember(Name = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        [DataMember(Name = "type")]
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Provides type of message for javascript.")]
        public MessageType Type { get; set; }
    }
}
