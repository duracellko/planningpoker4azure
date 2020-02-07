using System;
using Newtonsoft.Json.Serialization;

namespace Duracellko.PlanningPoker.Domain.Serialization
{
    /// <summary>
    /// Used by <see cref="JsonSerializer"/> to resolve a <see cref="JsonContract"/> for a given <see cref="System.Type"/>.
    /// </summary>
    internal class ScrumTeamContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a <see cref="JsonObjectContract"/> for the given type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>A <see cref="JsonObjectContract"/> for the given type.</returns>
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var result = base.CreateObjectContract(objectType);

            if (objectType == typeof(Estimation))
            {
                result.OverrideCreator = CreateEstimation;
                result.CreatorParameters.Add(result.Properties[nameof(Estimation.Value)]);
            }

            return result;
        }

        private static object CreateEstimation(params object[] args)
        {
            return new Estimation((double?)args[0]);
        }
    }
}
