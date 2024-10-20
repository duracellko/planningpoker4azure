using System;
using Bunit;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.Test.Controllers;
using Duracellko.PlanningPoker.Client.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using PlanningPokerPage = Duracellko.PlanningPoker.Client.Pages.PlanningPoker;

namespace Duracellko.PlanningPoker.Client.Test.Pages;

[TestClass]
public sealed class PlanningPokerTest : IDisposable
{
    private const string BaseUrl = "http://planningpoker.duracellko.net/PlanningPoker?";

    private readonly Bunit.TestContext _context = new Bunit.TestContext();

    public void Dispose()
    {
        _context.Dispose();
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254")]
    public void Initialized_NoTeamAndNoMember_OpensIndexPage(string? queryString)
    {
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(BaseUrl + queryString);
        InitializeContext(navigationManager.Object);

        using var target = _context.RenderComponent<PlanningPokerPage>();

        var expectedUri = "Index";
        navigationManager.Verify(o => o.NavigateTo(expectedUri));
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254")]
    public void Initialized_TeamNameProvided_OpensIndexPage(string? queryString)
    {
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(BaseUrl + queryString);
        InitializeContext(navigationManager.Object);

        using var target = _context.RenderComponent<PlanningPokerPage>(
            ComponentParameter.CreateParameter(nameof(PlanningPokerPage.TeamName), PlanningPokerData.TeamName));

        var expectedUri = "Index/Test%20team";
        navigationManager.Verify(o => o.NavigateTo(expectedUri));
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254")]
    [DataRow("CallbackReference=ID%23254")]
    public void Initialized_TeamNameAndMemberNameProvided_OpensIndexPage(string? queryString)
    {
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(BaseUrl);
        InitializeContext(navigationManager.Object);

        using var target = _context.RenderComponent<PlanningPokerPage>(
            ComponentParameter.CreateParameter(nameof(PlanningPokerPage.TeamName), PlanningPokerData.TeamName),
            ComponentParameter.CreateParameter(nameof(PlanningPokerPage.MemberName), PlanningPokerData.MemberName));

        var expectedUri = "Index/Test%20team/Test%20member";
        navigationManager.Verify(o => o.NavigateTo(expectedUri));
    }

    [TestMethod]
    [DataRow("AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254")]
    [DataRow("callbackReference=ID%23254&callbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254")]
    public void Initialized_CallbackReferenceInUrl_OpensIndexPage(string queryString)
    {
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(BaseUrl + queryString);
        InitializeContext(navigationManager.Object);

        using var target = _context.RenderComponent<PlanningPokerPage>(
            ComponentParameter.CreateParameter(nameof(PlanningPokerPage.TeamName), PlanningPokerData.TeamName),
            ComponentParameter.CreateParameter(nameof(PlanningPokerPage.MemberName), PlanningPokerData.MemberName));

        var expectedUri = "Index/Test%20team/Test%20member?CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254";
        navigationManager.Verify(o => o.NavigateTo(expectedUri));
    }

    private static PlanningPokerController CreatePlanningPokerController()
    {
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var busyIndicatorService = new Mock<IBusyIndicatorService>();
        var messageBoxService = new Mock<IMessageBoxService>();
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        var timerFactory = new Mock<ITimerFactory>();
        var dateTimeProvider = new DateTimeProviderMock();
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var timerSettingsRepository = new Mock<ITimerSettingsRepository>();
        var applicationIntegrationService = new Mock<IApplicationIntegrationService>();
        return new PlanningPokerController(
            planningPokerClient.Object,
            busyIndicatorService.Object,
            messageBoxService.Object,
            memberCredentialsStore.Object,
            timerFactory.Object,
            dateTimeProvider,
            serviceTimeProvider.Object,
            timerSettingsRepository.Object,
            applicationIntegrationService.Object);
    }

    private static MessageReceiver CreateMessageReceiver()
    {
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        return new MessageReceiver(planningPokerClient.Object, serviceTimeProvider.Object);
    }

    private void InitializeContext(INavigationManager? navigationManager = null)
    {
        if (navigationManager == null)
        {
            var navigationManagerMock = new Mock<INavigationManager>();
            navigationManager = navigationManagerMock.Object;
        }

        _context.Services.AddSingleton(navigationManager);
        _context.Services.AddScoped(_ => CreatePlanningPokerController());
        _context.Services.AddScoped(_ => CreateMessageReceiver());
    }
}
