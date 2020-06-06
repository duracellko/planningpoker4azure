using System;

namespace Duracellko.PlanningPoker.Web.Model
{
    [Flags]
    public enum ServerSideConditions
    {
        Never = 0,
        Always = -1,
        Mobile = 1
    }
}
