using System.Collections.Generic;
using System.Linq;

namespace Duracellko.PlanningPoker.Client.Controllers
{
    /// <summary>
    /// Summary values of estimations, e.g. average value.
    /// </summary>
    public class EstimationSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationSummary" /> class.
        /// </summary>
        /// <param name="memberEstimations">Member estimations to calculate summary for.</param>
        internal EstimationSummary(IEnumerable<MemberEstimation> memberEstimations)
        {
            var numericEstimations = memberEstimations.Where(IsNumericEstimation)
                .Select(e => e.Estimation!.Value)
                .ToList();

            if (numericEstimations.Count > 0)
            {
                Average = numericEstimations.Average();
                Sum = numericEstimations.Sum();
                Median = GetMedian(numericEstimations);
            }
        }

        /// <summary>
        /// Gets average (arithmetic mean) of estimations.
        /// </summary>
        public double? Average { get; }

        /// <summary>
        /// Gets sum of estimations.
        /// </summary>
        public double? Sum { get; }

        /// <summary>
        /// Gets median value of estimations.
        /// </summary>
        public double? Median { get; }

        private static bool IsNumericEstimation(MemberEstimation memberEstimation)
        {
            if (!memberEstimation.HasEstimation || !memberEstimation.Estimation.HasValue)
            {
                return false;
            }

            var value = memberEstimation.Estimation.Value;
            return double.IsFinite(value) && value >= 0;
        }

        private static double GetMedian(List<double> values)
        {
            values.Sort();

            if ((values.Count & 1) == 1)
            {
                return values[(values.Count - 1) / 2];
            }
            else
            {
                var medianIndex = values.Count / 2;
                var median1 = values[medianIndex - 1];
                var median2 = values[medianIndex];
                return (median1 + median2) / 2;
            }
        }
    }
}
