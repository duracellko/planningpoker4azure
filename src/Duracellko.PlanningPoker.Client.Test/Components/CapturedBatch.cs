using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    public class CapturedBatch
    {
        public List<RenderTreeDiff> UpdatedComponents { get; } = new List<RenderTreeDiff>();

        public List<RenderTreeFrame> ReferenceFrames { get; } = new List<RenderTreeFrame>();

        public List<int> DisposedComponentIDs { get; } = new List<int>();

        public List<int> DisposedEventHandlerIDs { get; } = new List<int>();
    }
}
