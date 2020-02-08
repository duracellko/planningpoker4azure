using System;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Estimation value of a planning poker card.
    /// </summary>
    public class Estimation : IEquatable<Estimation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Estimation"/> class.
        /// </summary>
        public Estimation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Estimation"/> class.
        /// </summary>
        /// <param name="value">The estimation value.</param>
        public Estimation(double? value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the estimation value. Estimation can be any positive number (usually Fibonacci numbers) or
        /// positive infinity or null representing unknown estimation.
        /// </summary>
        /// <value>The estimation value.</value>
        public double? Value { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>True</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Estimation))
            {
                return false;
            }

            return Equals((Estimation)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Value.HasValue ? Value.GetHashCode() : 0;
        }

        /// <summary>
        /// Indicates whether the current Estimation is equal to another Estimation.
        /// </summary>
        /// <param name="other">The other Estimation to compare with.</param>
        /// <returns><c>True</c> if the specified Estimation is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Estimation other)
        {
            return other != null ? Value == other.Value : false;
        }
    }
}
