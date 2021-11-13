using System;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.UI
{
    [TestClass]
    public class BusyIndicatorServiceTest
    {
        [TestMethod]
        public void Show_NoHandler_ReturnsNotNull()
        {
            var target = CreateBusyIndicatorService();

            var result = target.Show();

            Assert.IsNotNull(result);
            result.Dispose();
        }

        [TestMethod]
        public void Show_Handler_ReturnsNotNull()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            var result = target.Show();

            Assert.IsNotNull(result);
            result.Dispose();
        }

        [TestMethod]
        public void Show_Handler_SetVisibility()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            target.Show();

            Assert.AreEqual(1, handler.Counter);
            Assert.IsTrue(handler.IsVisible);
        }

        [TestMethod]
        public void Show_Twice_SetVisibilityOnce()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            target.Show();
            target.Show();

            Assert.AreEqual(1, handler.Counter);
            Assert.IsTrue(handler.IsVisible);
        }

        [TestMethod]
        public void Show_Dispose_SetVisibilityToFalse()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            var result = target.Show();
            result.Dispose();

            Assert.AreEqual(2, handler.Counter);
            Assert.IsFalse(handler.IsVisible);
        }

        [TestMethod]
        public void Show_ShowTwiceDisposeTwice_SetVisibilityToFalse()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            var result1 = target.Show();
            var result2 = target.Show();
            result1.Dispose();
            result2.Dispose();

            Assert.AreEqual(2, handler.Counter);
            Assert.IsFalse(handler.IsVisible);
        }

        [TestMethod]
        public void Show_ShowTwiceDisposeOnce_SetVisibilityToTrue()
        {
            var handler = new BusyIndicatorHandler();
            var target = CreateBusyIndicatorService(handler: handler);

            var result1 = target.Show();
            var result2 = target.Show();
            result1.Dispose();

            Assert.AreEqual(1, handler.Counter);
            Assert.IsTrue(handler.IsVisible);

            result2.Dispose();
        }

        private static BusyIndicatorService CreateBusyIndicatorService(BusyIndicatorHandler? handler = null)
        {
            var result = new BusyIndicatorService();
            result.SetBusyIndicatorHandler(handler != null ? handler.SetVisibility : default(Action<bool>));
            return result;
        }

        private class BusyIndicatorHandler
        {
            public bool IsVisible { get; private set; }

            public int Counter { get; private set; }

            public void SetVisibility(bool value)
            {
                Counter++;
                IsVisible = value;
            }
        }
    }
}
