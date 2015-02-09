// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace Duracellko.PlanningPoker.Azure.ServiceBus
{
    /// <summary>
    /// Instance of this class is able to convert messages of type <see cref="T:NodeMessage"/> to BrokeredMessage and vice versa.
    /// </summary>
    public class MessageConverter : IMessageConverter
    {
        /// <summary>
        /// Name of property in BrokeredMessage holding recipient node ID.
        /// </summary>
        internal const string RecipientIdPropertyName = "RecipientId";

        /// <summary>
        /// Name of property in BrokeredMessage holding sender node ID.
        /// </summary>
        internal const string SenderIdPropertyName = "SenderId";
        
        private const string MessageTypePropertyName = "MessageType";
        private const string MessageSubtypePropertyName = "MessageSubtype";

        /// <summary>
        /// Converts <see cref="T:NodeMessage"/> message to BrokeredMessage object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of BrokeredMessage type.</returns>
        public BrokeredMessage ConvertToBrokeredMessage(NodeMessage message)
        {
            var result = new BrokeredMessage(message.Data);
            result.Properties[MessageTypePropertyName] = message.MessageType.ToString();
            if (message.Data != null)
            {
                result.Properties[MessageSubtypePropertyName] = message.Data.GetType().Name;
            }

            result.Properties[SenderIdPropertyName] = message.SenderNodeId;
            result.Properties[RecipientIdPropertyName] = message.RecipientNodeId;
            return result;
        }

        /// <summary>
        /// Converts BrokeredMessage message to <see cref="T:NodeMessage"/> object.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>Converted message of NodeMessage type.</returns>
        public NodeMessage ConvertToNodeMessage(BrokeredMessage message)
        {
            var messageType = (NodeMessageType)Enum.Parse(typeof(NodeMessageType), (string)message.Properties[MessageTypePropertyName]);
            var messageSubtype = message.Properties.ContainsKey(MessageSubtypePropertyName) ? (string)message.Properties[MessageSubtypePropertyName] : null;

            var result = new NodeMessage(messageType);
            result.SenderNodeId = (string)message.Properties[SenderIdPropertyName];
            result.RecipientNodeId = (string)message.Properties[RecipientIdPropertyName];

            switch (result.MessageType)
            {
                case NodeMessageType.ScrumTeamMessage:
                    if (string.Equals(messageSubtype, typeof(ScrumTeamMemberMessage).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Data = message.GetBody<ScrumTeamMemberMessage>();
                    }
                    else if (string.Equals(messageSubtype, typeof(ScrumTeamMemberEstimationMessage).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Data = message.GetBody<ScrumTeamMemberEstimationMessage>();
                    }
                    else
                    {
                        result.Data = message.GetBody<ScrumTeamMessage>();
                    }

                    break;
                case NodeMessageType.TeamCreated:
                case NodeMessageType.InitializeTeam:
                    result.Data = message.GetBody<byte[]>();
                    break;
                case NodeMessageType.TeamList:
                case NodeMessageType.RequestTeams:
                    result.Data = message.GetBody<string[]>();
                    break;
            }

            return result;
        }
    }
}
