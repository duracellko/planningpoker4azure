using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Collection of estimations of all members involved in planning poker.
/// </summary>
[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "EstimationResult is more than just a collection.")]
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Interface implemetnation members are grouped together.")]
public sealed class EstimationResult : ICollection<KeyValuePair<Member, Estimation?>>
{
    private readonly Dictionary<Member, Estimation?> _estimations = new Dictionary<Member, Estimation?>();
    private bool _isReadOnly;

    /// <summary>
    /// Initializes a new instance of the <see cref="EstimationResult"/> class.
    /// </summary>
    /// <param name="members">The members involved in planning poker.</param>
    public EstimationResult(IEnumerable<Member> members)
    {
        ArgumentNullException.ThrowIfNull(members);

        foreach (var member in members)
        {
            _estimations.Add(member, null);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EstimationResult"/> class.
    /// </summary>
    /// <param name="team">Scrum Team owning the estimation result.</param>
    /// <param name="estimationResult">Estimation result serialization data.</param>
    internal EstimationResult(ScrumTeam team, IDictionary<string, Estimation?> estimationResult)
    {
        foreach (var estimationResultItem in estimationResult)
        {
            var memberName = estimationResultItem.Key;
            var member = team.FindMemberOrObserver(memberName) as Member;
            if (member == null)
            {
                member = new Member(team, memberName);
            }

            _estimations.Add(member, estimationResultItem.Value);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Duracellko.PlanningPoker.Domain.Estimation"/> for the specified member.
    /// </summary>
    /// <param name="member">The member to get or set estimation for.</param>
    /// <returns>The estimation of the member.</returns>
    [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "Member is valid indexer of EstimationResult.")]
    [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "Key not found in indexer.")]
    public Estimation? this[Member member]
    {
        get
        {
            if (!ContainsMember(member))
            {
                throw new KeyNotFoundException(Resources.Error_MemberNotInResult);
            }

            return _estimations[member];
        }

        set
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException(Resources.Error_EstimationResultIsReadOnly);
            }

            if (!ContainsMember(member))
            {
                throw new KeyNotFoundException(Resources.Error_MemberNotInResult);
            }

            _estimations[member] = value;
        }
    }

    /// <summary>
    /// Determines whether the specified member contains member.
    /// </summary>
    /// <param name="member">The Scrum team member.</param>
    /// <returns>
    ///   <c>True</c> if the specified member contains member; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsMember(Member member)
    {
        return _estimations.ContainsKey(member);
    }

    /// <summary>
    /// Sets the collection as read only. Mostly used after all members picked their estimations.
    /// </summary>
    public void SetReadOnly()
    {
        _isReadOnly = true;
    }

    /// <summary>
    /// Gets the number of elements contained in the collection.
    /// </summary>
    /// <value>The number of elements.</value>
    public int Count
    {
        get { return _estimations.Count; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value>
    ///     <c>True</c> if this instance is read only; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadOnly
    {
        get { return _isReadOnly; }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>A <see cref="System.Collections.Generic.IEnumerator&lt;T&gt;"/> that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValuePair<Member, Estimation?>> GetEnumerator()
    {
        return _estimations.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool ICollection<KeyValuePair<Member, Estimation?>>.Contains(KeyValuePair<Member, Estimation?> item)
    {
        return ((ICollection<KeyValuePair<Member, Estimation?>>)_estimations).Contains(item);
    }

    void ICollection<KeyValuePair<Member, Estimation?>>.CopyTo(KeyValuePair<Member, Estimation?>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<Member, Estimation?>>)_estimations).CopyTo(array, arrayIndex);
    }

    void ICollection<KeyValuePair<Member, Estimation?>>.Add(KeyValuePair<Member, Estimation?> item)
    {
        throw new NotSupportedException();
    }

    void ICollection<KeyValuePair<Member, Estimation?>>.Clear()
    {
        throw new NotSupportedException();
    }

    bool ICollection<KeyValuePair<Member, Estimation?>>.Remove(KeyValuePair<Member, Estimation?> item)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Gets serialization data of the object.
    /// </summary>
    /// <returns>The serialization data.</returns>
    internal IDictionary<string, Estimation?> GetData()
    {
        var result = new Dictionary<string, Estimation?>();

        foreach (var estimation in _estimations)
        {
            result.Add(estimation.Key.Name, estimation.Value);
        }

        return result;
    }
}
