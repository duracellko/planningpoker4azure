// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game.
    /// </summary>
    [Serializable]
    [DataContract(Name = "teamMember", Namespace = Namespaces.PlanningPokerData)]
    public class TeamMember
    {
        /// <summary>
        /// Gets or sets the type of member. Value can be one of ScrumMaster, Member or Observer.
        /// </summary>
        /// <value>
        /// The type of member.
        /// </value>
        [DataMember(Name = "type")]
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "Provides type of member for javascript.")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the member's name.
        /// </summary>
        /// <value>The member's name.</value>
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
