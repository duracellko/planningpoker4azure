using System;
using System.Diagnostics.CodeAnalysis;

namespace Duracellko.PlanningPoker.Web.Model
{
    [Flags]
    [SuppressMessage("Design", "CA1008:Enums devem ter valor zero", Justification = "Nunca representa que a condição nunca é verdadeira.")]
    [SuppressMessage("Usage", "CA2217:Não marque enums com FlagsAttribute", Justification = "Sempre (-1) tem todos os bits definidos.")]
    public enum ServerSideConditions
    {
        Never = 0,
        Always = -1,
        Mobile = 1
    }
}
