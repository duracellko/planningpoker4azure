using System;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Web.Model;

[Flags]
[SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Never represents that condition is never true.")]
[SuppressMessage("Usage", "CA2217:Do not mark enums with FlagsAttribute", Justification = "Always (-1) has all bits set.")]
public enum ServerSideConditions
{
    Never = 0,
    Always = -1,
    Mobile = 1
}
