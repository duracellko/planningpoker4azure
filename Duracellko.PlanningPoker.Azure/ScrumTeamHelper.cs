// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Duracellko.PlanningPoker.Domain;

namespace Duracellko.PlanningPoker.Azure
{
    /// <summary>
    /// Helper methods for ScrumTeam objects.
    /// </summary>
    internal static class ScrumTeamHelper
    {
        /// <summary>
        /// Serializes Scrum team into array of bytes.
        /// </summary>
        /// <param name="scrumTeam">The Scrum team.</param>
        /// <returns>The byte array.</returns>
        public static byte[] SerializeScrumTeam(ScrumTeam scrumTeam)
        {
            if (scrumTeam == null)
            {
                throw new ArgumentNullException("scrumTeam");
            }

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, scrumTeam);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes Scrum team from array of bytes.
        /// </summary>
        /// <param name="buffer">The byte array.</param>
        /// <returns>The Scrum team.</returns>
        public static ScrumTeam DeserializeScrumTeam(byte[] buffer)
        {
            return DeserializeScrumTeam(buffer, null);
        }

        /// <summary>
        /// Deserializes Scrum team from array of bytes.
        /// </summary>
        /// <param name="buffer">The byte array.</param>
        /// <param name="dateTimeProvider">The date time provider used be the Scrum team.</param>
        /// <returns>The Scrum team.</returns>
        public static ScrumTeam DeserializeScrumTeam(byte[] buffer, DateTimeProvider dateTimeProvider)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            using (var memoryStream = new MemoryStream(buffer))
            {
                var formatter = dateTimeProvider != null ? new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All, dateTimeProvider)) : new BinaryFormatter();
                return (ScrumTeam)formatter.Deserialize(memoryStream);
            }
        }
    }
}
