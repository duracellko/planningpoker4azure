namespace Duracellko.PlanningPoker.Client.Controllers
{
    public class MemberEstimation
    {
        public MemberEstimation(string memberName)
        {
            MemberName = memberName;
        }

        public MemberEstimation(string memberName, double? estimation)
            : this(memberName)
        {
            Estimation = estimation;
            HasEstimation = true;
        }

        public string MemberName { get; }

        public bool HasEstimation { get; }

        public double? Estimation { get; }
    }
}
