using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    public class CapturedBatch
    {
        public IDictionary<int, List<CapturedRenderTreeDiff>> DiffsByComponentId { get; } = new Dictionary<int, List<CapturedRenderTreeDiff>>();

        public IList<CapturedRenderTreeDiff> DiffsInOrder { get; } = new List<CapturedRenderTreeDiff>();

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Property is captured during rendering.")]
        public IList<int> DisposedComponentIDs { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Property is captured during rendering.")]
        public IList<RenderTreeFrame> ReferenceFrames { get; set; }

        internal void AddDiff(RenderTreeDiff diff)
        {
            var componentId = diff.ComponentId;
            if (!DiffsByComponentId.ContainsKey(componentId))
            {
                DiffsByComponentId.Add(componentId, new List<CapturedRenderTreeDiff>());
            }

            var renderTreeEdits = diff.Edits.ToArray();
            var capturedTreeDiff = new CapturedRenderTreeDiff(diff.ComponentId, renderTreeEdits);
            DiffsByComponentId[componentId].Add(capturedTreeDiff);
            DiffsInOrder.Add(capturedTreeDiff);
        }
    }
}
