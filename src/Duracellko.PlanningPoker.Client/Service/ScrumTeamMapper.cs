using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public static void ConvertScrumTeam(ScrumTeam? scrumTeam)
        {
            if (scrumTeam != null)
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
        }

        /// <summary>
        /// Convert estimation value.
        /// </summary>
        /// <param name="estimation">The estimation to convert.</param>
        public static void ConvertEstimation(Estimation? estimation)
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

        /// <summary>
        /// Gets new <see cref="PlanningPokerException"/> from response value.
        /// </summary>
        /// <param name="value">The response value that contains data for the PlanningPokerException.</param>
        /// <returns>A new instance of PlanningPokerException.</returns>
        public static PlanningPokerException GetPlanningPokerException(string? value) => GetPlanningPokerException(value, null);

        /// <summary>
        /// Gets new <see cref="PlanningPokerException"/> from response value.
        /// </summary>
        /// <param name="value">The response value that contains data for the PlanningPokerException.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        /// <returns>A new instance of PlanningPokerException.</returns>
        public static PlanningPokerException GetPlanningPokerException(string? value, Exception? innerException)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new PlanningPokerException(value, innerException);
            }

            var prefix = nameof(PlanningPokerException) + ':';
            var index = value.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var dataJson = value.Substring(index + prefix.Length);
                var data = JsonSerializer.Deserialize<PlanningPokerExceptionData>(dataJson);
                if (data != null)
                {
                    return new PlanningPokerException(data.Message, data.Error, data.Argument, innerException);
                }
            }

            return new PlanningPokerException(value, innerException);
        }

        private static void ConvertMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.EstimationEnded:
                    var estimationResultMessage = (EstimationResultMessage)message;
                    ConvertEstimations(estimationResultMessage.EstimationResult);
                    break;
                case MessageType.AvailableEstimationsChanged:
                    var estimationSetMessage = (EstimationSetMessage)message;
                    ConvertEstimations(estimationSetMessage.Estimations);
                    break;
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
