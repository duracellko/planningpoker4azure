// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Estimation value of a planning poker card.
    /// </summary>
    [Serializable]
    public class Estimation : IEquatable<Estimation>
    {
        #region Constructor

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
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the estimation value. Estimation can be any positive number (usually Fibonacci numbers) or
        /// positive infinity or null representing unknown estimation.
        /// </summary>
        /// <value>The estimation value.</value>
        public double? Value { get; private set; }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>True</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Estimation))
            {
                return false;
            }

            return this.Equals((Estimation)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Value.HasValue ? this.Value.GetHashCode() : 0;
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// Indicates whether the current Estimation is equal to another Estimation.
        /// </summary>
        /// <param name="other">The other Estimation to compare with.</param>
        /// <returns><c>True</c> if the specified Estimation is equal to this instance; otherwise, <c>false</c>.</returns>
        public bool Equals(Estimation other)
        {
            return other != null ? this.Value == other.Value : false;
        }

        #endregion
    }
}
