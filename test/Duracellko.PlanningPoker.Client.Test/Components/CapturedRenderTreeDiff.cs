using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Structure is not supposed to be compared.")]
    public readonly struct CapturedRenderTreeDiff
    {
        public CapturedRenderTreeDiff(int componentId, IReadOnlyList<RenderTreeEdit> edits)
        {
            ComponentId = componentId;
            Edits = edits;
        }

        public int ComponentId { get; }

        public IReadOnlyList<RenderTreeEdit> Edits { get; }
    }
}
