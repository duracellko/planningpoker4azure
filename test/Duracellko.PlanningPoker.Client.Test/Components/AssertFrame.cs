using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Demonstration only.")]
    public static class AssertFrame
    {
        public static void Sequence(RenderTreeFrame frame, int? sequence = null)
        {
            if (sequence.HasValue)
            {
                Assert.AreEqual(sequence.Value, frame.Sequence);
            }
        }

        public static void Markup(RenderTreeFrame frame, string markupContent, int? sequence = null)
        {
            Assert.AreEqual(RenderTreeFrameType.Markup, frame.FrameType);
            Assert.AreEqual(markupContent, frame.MarkupContent);
            Assert.AreEqual(0, frame.ElementSubtreeLength);
            AssertFrame.Sequence(frame, sequence);
        }

        public static void Attribute(RenderTreeFrame frame, string attributeName, int? sequence = null)
        {
            Assert.AreEqual(RenderTreeFrameType.Attribute, frame.FrameType);
            Assert.AreEqual(attributeName, frame.AttributeName);
            AssertFrame.Sequence(frame, sequence);
        }

        public static void Attribute(RenderTreeFrame frame, string attributeName, string attributeValue, int? sequence = null)
        {
            AssertFrame.Attribute(frame, attributeName, sequence);
            Assert.AreEqual(attributeValue, frame.AttributeValue);
        }

        public static void Element(RenderTreeFrame frame, string elementName, int subtreeLength, int? sequence = null)
        {
            Assert.AreEqual(RenderTreeFrameType.Element, frame.FrameType);
            Assert.AreEqual(elementName, frame.ElementName);
            Assert.AreEqual(subtreeLength, frame.ElementSubtreeLength);
            AssertFrame.Sequence(frame, sequence);
        }

        public static void Text(RenderTreeFrame frame, string textContent, int? sequence = null)
        {
            Assert.AreEqual(RenderTreeFrameType.Text, frame.FrameType);
            Assert.AreEqual(textContent, frame.TextContent);
            Assert.AreEqual(0, frame.ElementSubtreeLength);
            AssertFrame.Sequence(frame, sequence);
        }
    }
}
