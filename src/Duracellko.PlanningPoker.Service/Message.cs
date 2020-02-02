using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Generic message that can be sent to Scrum team members or observers.
    /// </summary>
    [JsonConverter(typeof(Serialization.MessageJsonConverter))]
    public class Message
    {
        /// <summary>
        /// Gets or sets the message ID unique to member, so that member can track which messages he/she already got.
        /// </summary>
        /// <value>The message ID.</value>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Provides type of message for javascript.")]
        public MessageType Type { get; set; }
    }
}
