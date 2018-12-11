using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    public class TestRenderer : Renderer
    {
        public TestRenderer(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public List<CapturedBatch> Batches { get; } = new List<CapturedBatch>();

        public new int AssignRootComponentId(IComponent component)
        {
            return base.AssignRootComponentId(component);
        }

        public T InstantiateComponent<T>()
            where T : IComponent
        {
            return (T)InstantiateComponent(typeof(T));
        }

        public new void RenderRootComponent(int componetId)
        {
            base.RenderRootComponent(componetId);
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
        {
            var capturedBatch = new CapturedBatch();
            capturedBatch.UpdatedComponents.AddRange(renderBatch.UpdatedComponents);
            capturedBatch.ReferenceFrames.AddRange(renderBatch.ReferenceFrames);
            capturedBatch.DisposedComponentIDs.AddRange(renderBatch.DisposedComponentIDs);
            capturedBatch.DisposedEventHandlerIDs.AddRange(renderBatch.DisposedEventHandlerIDs);

            Batches.Add(capturedBatch);
            return Task.CompletedTask;
        }
    }
}
