using System.Collections.Generic;
using Duracellko.PlanningPoker.Service;

namespace Duracellko.PlanningPoker.Client.Service
{
    /// <summary>
    /// Converts estimation values on Scrum Team entities.
    /// </summary>
    /// <remarks>
    /// Infinity value cannot be serialized in JSON, so it is mapped from numeric value.
    /// </remarks>
    internal static class ScrumTeamMapper
    {
        /// <summary>
        /// Converts estimation values in Scrum Team.
        /// </summary>
        /// <param name="scrumTeam">The Scrum Team to convert.</param>
        public static void ConvertScrumTeam(ScrumTeam scrumTeam)
        {
            if (scrumTeam.AvailableEstimations != null)
            {
                ConvertEstimations(scrumTeam.AvailableEstimations);
            }

            if (scrumTeam.EstimationResult != null)
            {
                ConvertEstimations(scrumTeam.EstimationResult);
            }
        }

        /// <summary>
        /// Convert estimation value.
        /// </summary>
        /// <param name="estimation">The estimation to convert.</param>
        public static void ConvertEstimation(Estimation estimation)
        {
            if (estimation != null && estimation.Value == Estimation.PositiveInfinity)
            {
                estimation.Value = double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Convert estimation values in message.
        /// </summary>
        /// <param name="messages">The message to convert.</param>
        public static void ConvertMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                ConvertMessage(message);
            }
        }

        private static void ConvertMessage(Message message)
        {
            if (message.Type == MessageType.EstimationEnded)
            {
                var estimationResultMessage = (EstimationResultMessage)message;
                ConvertEstimations(estimationResultMessage.EstimationResult);
            }
        }

        private static void ConvertEstimations(IEnumerable<Estimation> estimations)
        {
            foreach (var estimation in estimations)
            {
                ConvertEstimation(estimation);
            }
        }

        private static void ConvertEstimations(IEnumerable<EstimationResultItem> estimationResultItems)
        {
            foreach (var estimationResultItem in estimationResultItems)
            {
                ConvertEstimation(estimationResultItem.Estimation);
            }
        }
    }
}
