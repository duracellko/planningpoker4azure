using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Duracellko.PlanningPoker.Client.Components;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Components
{
    [TestClass]
    public sealed class GlobalMessagePanelTest : IDisposable
    {
        private Bunit.TestContext _context = new Bunit.TestContext();

        public void Dispose()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task ShowMessage_NoTitle_MessageIsDisplayed()
        {
            var messageBoxService = new MessageBoxService();
            InitializeContext(messageBoxService);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                messageBoxService.ShowMessage("This is test message", null);
            });

            var titleElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-header > h5.modal-title");
            Assert.AreEqual(string.Empty, titleElement.TextContent);
            var bodyElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-body > p");
            Assert.AreEqual("This is test message", bodyElement.TextContent);
            var footerElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-footer");
            var buttonElements = footerElement.GetElementsByTagName("button");
            Assert.AreEqual(1, buttonElements.Length);
            CollectionAssert.DoesNotContain(buttonElements[0].ClassList.ToList(), "btn-primary");
        }

        [TestMethod]
        public async Task ShowMessage_PrimaryButtonText_MessageAndButtonAreDisplayed()
        {
            var messageBoxService = new MessageBoxService();
            InitializeContext(messageBoxService);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                messageBoxService.ShowMessage("Test this again.", "Some information", "Click me");
            });

            var titleElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-header > h5.modal-title");
            Assert.AreEqual("Some information", titleElement.TextContent);
            var bodyElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-body > p");
            Assert.AreEqual("Test this again.", bodyElement.TextContent);
            var footerElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-footer");
            var buttonElements = footerElement.GetElementsByTagName("button");
            Assert.AreEqual(2, buttonElements.Length);
            var buttonElement = buttonElements[0];
            CollectionAssert.Contains(buttonElement.ClassList.ToList(), "btn-primary");
            Assert.AreEqual("Click me", buttonElement.TextContent);
        }

        [TestMethod]
        public async Task ShowMessage_RunShowMessageBoxFunction()
        {
            var messageBoxService = new MessageBoxService();
            var jsRuntime = new Mock<IJSRuntime>();
            InitializeContext(messageBoxService, jsRuntime: jsRuntime.Object);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                messageBoxService.ShowMessage("Test", null);
            });

            jsRuntime.Verify(o => o.InvokeAsync<object>("Duracellko.PlanningPoker.showMessageBox", It.Is<object?[]>(args => args.Length == 1 && args[0] is ElementReference)));
        }

        [TestMethod]
        public async Task ClickCloseDialogButton_ShowDialogTaskIsCompleted()
        {
            var messageBoxService = new MessageBoxService();
            var jsRuntime = new Mock<IJSRuntime>();
            InitializeContext(messageBoxService, jsRuntime: jsRuntime.Object);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            Task<bool>? showMessageTask = null;
            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                showMessageTask = messageBoxService.ShowMessage("Test", "T", "Confirm");
            });

            var buttonElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-footer > button.btn-secondary");
            buttonElement.Click();

            Assert.IsNotNull(showMessageTask);
            Assert.IsTrue(showMessageTask.IsCompleted);
            var result = await showMessageTask;
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ClickPrimaryDialogButton_ShowDialogTaskIsCompleted()
        {
            var messageBoxService = new MessageBoxService();
            var jsRuntime = new Mock<IJSRuntime>();
            InitializeContext(messageBoxService, jsRuntime: jsRuntime.Object);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            Task<bool>? showMessageTask = null;
            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                showMessageTask = messageBoxService.ShowMessage("Test", "T", "Confirm");
            });

            var buttonElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-footer > button.btn-primary");
            buttonElement.Click();

            Assert.IsNotNull(showMessageTask);
            Assert.IsTrue(showMessageTask.IsCompleted);
            var result = await showMessageTask;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ClickCloseDialogButton_RunHideFunction()
        {
            var messageBoxService = new MessageBoxService();
            var jsRuntime = new Mock<IJSRuntime>();
            InitializeContext(messageBoxService, jsRuntime: jsRuntime.Object);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                messageBoxService.ShowMessage("Test", null);
            });

            var buttonElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-header > button.close");
            buttonElement.Click();

            jsRuntime.Verify(o => o.InvokeAsync<object>("Duracellko.PlanningPoker.hide", It.Is<object?[]>(args => args.Length == 1 && args[0] is ElementReference)));
        }

        [TestMethod]
        public async Task ClickPrimaryDialogButton_RunHideFunction()
        {
            var messageBoxService = new MessageBoxService();
            var jsRuntime = new Mock<IJSRuntime>();
            InitializeContext(messageBoxService, jsRuntime: jsRuntime.Object);

            using var target = _context.RenderComponent<GlobalMessagePanel>();

            await target.InvokeAsync(() =>
            {
                // Do not wait for ShowMessage to finish. It is finished only after closing message box.
                messageBoxService.ShowMessage("Test", "T", "Confirm");
            });

            var buttonElement = target.Find("div#messageBox > div.modal-dialog > div.modal-content > div.modal-footer > button.btn-primary");
            buttonElement.Click();

            jsRuntime.Verify(o => o.InvokeAsync<object>("Duracellko.PlanningPoker.hide", It.Is<object?[]>(args => args.Length == 1 && args[0] is ElementReference)));
        }

        private void InitializeContext(MessageBoxService messageBoxService, IJSRuntime? jsRuntime = null)
        {
            if (jsRuntime == null)
            {
                var jsRuntimeMock = new Mock<IJSRuntime>();
                jsRuntime = jsRuntimeMock.Object;
            }

            _context.Services.AddSingleton(messageBoxService);
            _context.Services.AddSingleton(jsRuntime);
        }
    }
}
