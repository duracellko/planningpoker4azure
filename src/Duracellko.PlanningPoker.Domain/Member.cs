using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Duracellko.PlanningPoker.Domain;

/// <summary>
/// Represents member of a Scrum team. Member can vote in planning poker and can receive messages about planning poker game.
/// </summary>
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Fields are placed near properties.")]
public class Member : Observer
{
    private static readonly CompositeFormat _errorEstimationIsNotAvailableInTeam = CompositeFormat.Parse(Resources.Error_EstimationIsNotAvailableInTeam);

    private Estimation? _estimation;

    /// <summary>
    /// Initializes a new instance of the <see cref="Member"/> class.
    /// </summary>
    /// <param name="team">The Scrum team, the member is joining.</param>
    /// <param name="name">The member name.</param>
    public Member(ScrumTeam team, string name)
        : base(team, name)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Member"/> class.
    /// </summary>
    /// <param name="team">The Scrum team the observer is joining.</param>
    /// <param name="memberData">The member serialization data.</param>
    internal Member(ScrumTeam team, Serialization.MemberData memberData)
        : base(team, memberData)
    {
        _estimation = memberData.Estimation;
    }

    /// <summary>
    /// Gets or sets the estimation, the member is picking in planning poker.
    /// </summary>
    /// <value>
    /// The estimation.
    /// </value>
    public Estimation? Estimation
    {
        get => _estimation;
        set
        {
            if (_estimation != value)
            {
                if (value != null && !Team.AvailableEstimations.Contains(value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, _errorEstimationIsNotAvailableInTeam, value.Value), nameof(value));
                }

                _estimation = value;
                if (Team.State == TeamState.EstimationInProgress)
                {
                    Team.OnMemberEstimated(this);
                }
            }
        }
    }

    /// <summary>
    /// Starts countdown timer for team with specified duration.
    /// </summary>
    /// <param name="duration">The duration of countdown.</param>
    public void StartTimer(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, Resources.Error_InvalidTimerDuraction);
        }

        Team.StartTimer(duration);
    }

    /// <summary>
    /// Stops active countdown timer.
    /// </summary>
    public void CancelTimer()
    {
        Team.CancelTimer();
    }

    /// <summary>
    /// Resets the estimation to unselected.
    /// </summary>
    internal void ResetEstimation()
    {
        _estimation = null;
    }

    /// <summary>
    /// Gets serialization data of the object.
    /// </summary>
    /// <returns>The serialization data.</returns>
    protected internal override Serialization.MemberData GetData()
    {
        var result = base.GetData();
        result.MemberType = Serialization.MemberType.Member;
        result.Estimation = Estimation;
        return result;
    }
}
