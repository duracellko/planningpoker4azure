using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging.Abstractions;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Demonstration only.")]
    public class TestRenderer : Renderer
    {
        public TestRenderer(IServiceProvider serviceProvider)
            : base(serviceProvider, NullLoggerFactory.Instance)
        {
        }

        public IList<CapturedBatch> Batches { get; } = new List<CapturedBatch>();

        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

        public new int AssignRootComponentId(IComponent component)
        {
            return base.AssignRootComponentId(component);
        }

        public T InstantiateComponent<T>()
            where T : IComponent
        {
            return (T)InstantiateComponent(typeof(T));
        }

        public new Task RenderRootComponentAsync(int componetId)
        {
            return Dispatcher.InvokeAsync(() => base.RenderRootComponentAsync(componetId));
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
        {
            var capturedBatch = new CapturedBatch();
            Batches.Add(capturedBatch);

            for (int i = 0; i < renderBatch.UpdatedComponents.Count; i++)
            {
                ref var renderTreeDiff = ref renderBatch.UpdatedComponents.Array[i];
                capturedBatch.AddDiff(renderTreeDiff);
            }

            capturedBatch.ReferenceFrames = renderBatch.ReferenceFrames.AsEnumerable().ToArray();
            capturedBatch.DisposedComponentIDs = renderBatch.DisposedComponentIDs.AsEnumerable().ToList();

            return Task.CompletedTask;
        }

        protected override void HandleException(Exception exception)
        {
            throw exception;
        }
    }
}
