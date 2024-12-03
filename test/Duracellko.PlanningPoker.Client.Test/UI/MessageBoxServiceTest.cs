using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Client.Test.UI;

[TestClass]
public class MessageBoxServiceTest
{
    [TestMethod]
    public void ShowMessage_NoHandler_ReturnsCompletedTask()
    {
        var target = CreateMessageBoxService();

        var result = target.ShowMessage("My message", "Test");

        Assert.IsTrue(result.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task ShowMessage_Handler_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage("My message", "Test");

        Assert.AreEqual(1, handler.Counter);
        Assert.AreEqual("My message", handler.Message);
        Assert.AreEqual("Test", handler.Title);
        Assert.IsNull(handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_MessageIsNull_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage(null!, null);

        Assert.AreEqual(1, handler.Counter);
        Assert.IsNull(handler.Message);
        Assert.IsNull(handler.Title);
        Assert.IsNull(handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_MessageIsEmpty_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage(string.Empty, string.Empty);

        Assert.AreEqual(1, handler.Counter);
        Assert.AreEqual(string.Empty, handler.Message);
        Assert.AreEqual(string.Empty, handler.Title);
        Assert.IsNull(handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_HandlerIsNotCompleted_ReturnsNotCompletedTask()
    {
        var task = new TaskCompletionSource<bool>();
        var handler = new MessageHandler() { ResultTask = task.Task };
        var target = CreateMessageBoxService(messageHandler: handler);

        var result = target.ShowMessage("My message", "Test");

        Assert.IsFalse(result.IsCompleted);

        task.SetResult(true);

        Assert.IsTrue(result.IsCompleted);
        await result;
    }

    [TestMethod]
    public void ShowMessage_PrimaryButtonAndNoHandler_ReturnsCompletedTask()
    {
        var target = CreateMessageBoxService();

        var result = target.ShowMessage("My message", "Test", "Click me");

        Assert.IsTrue(result.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonAndHandler_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage("My message", "Test", "Click me");

        Assert.AreEqual(1, handler.Counter);
        Assert.AreEqual("My message", handler.Message);
        Assert.AreEqual("Test", handler.Title);
        Assert.AreEqual("Click me", handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonIsNull_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage("My message", "Test", null);

        Assert.AreEqual(1, handler.Counter);
        Assert.AreEqual("My message", handler.Message);
        Assert.AreEqual("Test", handler.Title);
        Assert.IsNull(handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonAndMessageIsNull_HandlerIsExecuted()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        await target.ShowMessage(null!, null, null);

        Assert.AreEqual(1, handler.Counter);
        Assert.IsNull(handler.Message);
        Assert.IsNull(handler.Title);
        Assert.IsNull(handler.PrimaryButton);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonAndHandlerReturnsTrue_ReturnsTrue()
    {
        var handler = new MessageHandler();
        var target = CreateMessageBoxService(messageHandler: handler);

        var result = await target.ShowMessage("My message", "Test", "Click me");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonAndHandlerReturnsFalse_ReturnsFalse()
    {
        var handler = new MessageHandler() { Result = false };
        var target = CreateMessageBoxService(messageHandler: handler);

        var result = await target.ShowMessage("My message", "Test", "Click me");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonIsNullAndHandlerReturnsFalse_ReturnsFalse()
    {
        var handler = new MessageHandler() { Result = false };
        var target = CreateMessageBoxService(messageHandler: handler);

        var result = await target.ShowMessage("My message", "Test", null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ShowMessage_PrimaryButtonAndHandlerIsNotCompleted_ReturnsNotCompletedTask()
    {
        var task = new TaskCompletionSource<bool>();
        var handler = new MessageHandler() { ResultTask = task.Task };
        var target = CreateMessageBoxService(messageHandler: handler);

        var result = target.ShowMessage("My message", "Test", "Click me");

        Assert.IsFalse(result.IsCompleted);

        task.SetResult(true);

        Assert.IsTrue(result.IsCompleted);
        await result;
    }

    private static MessageBoxService CreateMessageBoxService(MessageHandler? messageHandler = null)
    {
        var result = new MessageBoxService();
        result.SetMessageHandler(messageHandler != null ? messageHandler.HandleMessage : default);
        return result;
    }

    private sealed class MessageHandler
    {
        public bool Result { get; set; } = true;

        public Task<bool>? ResultTask { get; set; }

        public int Counter { get; private set; }

        public string? Message { get; private set; }

        public string? Title { get; private set; }

        public string? PrimaryButton { get; private set; }

        public Task<bool> HandleMessage(string message, string? title, string? primaryButton)
        {
            Counter++;
            Message = message;
            Title = title;
            PrimaryButton = primaryButton;
            return ResultTask ?? Task.FromResult(Result);
        }
    }
}
