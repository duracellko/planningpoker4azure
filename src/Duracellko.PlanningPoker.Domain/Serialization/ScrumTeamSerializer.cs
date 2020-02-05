using System;
using System.IO;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Object serializes and deserializes ScrumTeam objects to/from JSON text.
    /// </summary>
    public class ScrumTeamSerializer
    {
        private DateTimeProvider _dateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamSerializer"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        public ScrumTeamSerializer(DateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider ?? DateTimeProvider.Default;
        }

        /// <summary>
        /// Serializes ScrumTeam object to JSON format.
        /// </summary>
        /// <param name="writer">Text writer to serialize the Scrum Team into.</param>
        /// <param name="scrumTeam">The Scrum Team to serialize.</param>
        public void Serialize(TextWriter writer, ScrumTeam scrumTeam)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes ScrumTeam object from JSON.
        /// </summary>
        /// <param name="reader">Text reader to deserialize Scrum Team from.</param>
        /// <returns>Deserialized ScrumTeam object.</returns>
        public ScrumTeam Deserialize(TextReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
