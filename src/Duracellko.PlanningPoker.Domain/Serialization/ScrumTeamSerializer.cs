using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Object serializes and deserializes ScrumTeam objects to/from JSON text.
    /// </summary>
    public class ScrumTeamSerializer
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        private readonly DateTimeProvider _dateTimeProvider;
        private readonly GuidProvider _guidProvider;

        static ScrumTeamSerializer()
        {
            _jsonSerializerOptions.Converters.Add(new EstimationJsonConverter());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScrumTeamSerializer"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time provider to provide current time. If null is specified, then default date time provider is used.</param>
        /// <param name="guidProvider">The GUID provider to provide new GUID objects. If null is specified, then default GUID provider is used.</param>
        public ScrumTeamSerializer(DateTimeProvider? dateTimeProvider, GuidProvider? guidProvider)
        {
            _dateTimeProvider = dateTimeProvider ?? DateTimeProvider.Default;
            _guidProvider = guidProvider ?? GuidProvider.Default;
        }

        /// <summary>
        /// Serializes ScrumTeam object to JSON format.
        /// </summary>
        /// <param name="utf8Json">UTF8 stream to serialize the Scrum Team into.</param>
        /// <param name="scrumTeam">The Scrum Team to serialize.</param>
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Object should be injected.")]
        public void Serialize(Stream utf8Json, ScrumTeam scrumTeam)
        {
            if (utf8Json == null)
            {
                throw new ArgumentNullException(nameof(utf8Json));
            }

            if (scrumTeam == null)
            {
                throw new ArgumentNullException(nameof(scrumTeam));
            }

            var data = scrumTeam.GetData();
            JsonSerializer.Serialize(utf8Json, data, _jsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes ScrumTeam object from JSON.
        /// </summary>
        /// <param name="utf8Json">UTF8 stream to deserialize Scrum Team from.</param>
        /// <returns>Deserialized ScrumTeam object.</returns>
        public ScrumTeam Deserialize(Stream utf8Json)
        {
            if (utf8Json == null)
            {
                throw new ArgumentNullException(nameof(utf8Json));
            }

            var data = JsonSerializer.Deserialize<ScrumTeamData>(utf8Json, _jsonSerializerOptions);

            if (data == null)
            {
                throw new InvalidOperationException(Resources.Error_DeserializationFailed);
            }

            return new ScrumTeam(data, _dateTimeProvider, _guidProvider);
        }
    }
}
