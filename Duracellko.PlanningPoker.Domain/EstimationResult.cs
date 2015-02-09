// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain
{
    /// <summary>
    /// Collection of estimations of all members involved in planning poker.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "EstimationResult is more than just a collection.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Interface implemetnation members are grouped together.")]
    public sealed class EstimationResult : ICollection<KeyValuePair<Member, Estimation>>
    {
        #region Fields

        private readonly Dictionary<Member, Estimation> estimations = new Dictionary<Member, Estimation>();
        private bool isReadOnly;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EstimationResult"/> class.
        /// </summary>
        /// <param name="members">The members involved in planning poker.</param>
        public EstimationResult(IEnumerable<Member> members)
        {
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }

            foreach (var member in members)
            {
                this.estimations.Add(member, null);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Duracellko.PlanningPoker.Domain.Estimation"/> for the specified member.
        /// </summary>
        /// <param name="member">The member to get or set estimation for.</param>
        /// <returns>The estimation of the member.</returns>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Member is valid indexer of EstimationResult.")]
        public Estimation this[Member member]
        {
            get
            {
                if (!this.ContainsMember(member))
                {
                    throw new KeyNotFoundException(Properties.Resources.Error_MemberNotInResult);
                }

                return this.estimations[member];
            }

            set
            {
                if (this.isReadOnly)
                {
                    throw new InvalidOperationException(Properties.Resources.Error_EstimationResultIsReadOnly);
                }

                if (!this.ContainsMember(member))
                {
                    throw new KeyNotFoundException(Properties.Resources.Error_MemberNotInResult);
                }

                this.estimations[member] = value;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the specified member contains member.
        /// </summary>
        /// <param name="member">The Scrum team member.</param>
        /// <returns>
        ///   <c>True</c> if the specified member contains member; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsMember(Member member)
        {
            return this.estimations.ContainsKey(member);
        }

        /// <summary>
        /// Sets the collection as read only. Mostly used after all members picked their estimations.
        /// </summary>
        public void SetReadOnly()
        {
            this.isReadOnly = true;
        }

        #endregion

        #region ICollection

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count
        {
            get { return this.estimations.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        ///     <c>True</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<Member, Estimation>> GetEnumerator()
        {
            return this.estimations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        bool ICollection<KeyValuePair<Member, Estimation>>.Contains(KeyValuePair<Member, Estimation> item)
        {
            return ((ICollection<KeyValuePair<Member, Estimation>>)this.estimations).Contains(item);
        }

        void ICollection<KeyValuePair<Member, Estimation>>.CopyTo(KeyValuePair<Member, Estimation>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Member, Estimation>>)this.estimations).CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<Member, Estimation>>.Add(KeyValuePair<Member, Estimation> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<Member, Estimation>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<Member, Estimation>>.Remove(KeyValuePair<Member, Estimation> item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
