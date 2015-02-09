// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Estimation value of a planning poker card.
    /// </summary>
    [Serializable]
    [DataContract(Name = "estimation", Namespace = Namespaces.PlanningPokerData)]
    public class Estimation
    {
        /// <summary>
        /// Value representing estimation of positive infinity.
        /// </summary>
        public const double PositiveInfinity = -1111100.0;

        /// <summary>
        /// Gets or sets the estimation value. Estimation can be any positive number (usually Fibonacci numbers) or
        /// positive infinity or null representing unknown estimation.
        /// </summary>
        /// <value>The estimation value.</value>
        [DataMember(Name = "value")]
        public double? Value { get; set; }
    }
}
