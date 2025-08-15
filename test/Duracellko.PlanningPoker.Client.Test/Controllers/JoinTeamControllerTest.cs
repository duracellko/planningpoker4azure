﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers;

[TestClass]
public class JoinTeamControllerTest
{
    private const string PageUrl = "http://planningpoker/Path/Should/Not/Matter";
    private const string ErrorMessage = "Planning Poker Error";
    private const string ReconnectErrorMessage = "Member or observer named 'Test member' already exists in the team.";
    private const string AutoConnectQueryString = "AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254";

    private CultureInfo? _originalCultureInfo;
    private CultureInfo? _originalUICultureInfo;

    [TestInitialize]
    public void TestInitialize()
    {
        _originalCultureInfo = CultureInfo.CurrentCulture;
        _originalUICultureInfo = CultureInfo.CurrentUICulture;
        var enCulture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.CurrentCulture = enCulture;
        CultureInfo.CurrentUICulture = enCulture;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (_originalCultureInfo != null)
        {
            CultureInfo.CurrentCulture = _originalCultureInfo;
            _originalCultureInfo = null;
        }

        if (_originalUICultureInfo != null)
        {
            CultureInfo.CurrentUICulture = _originalUICultureInfo;
            _originalUICultureInfo = null;
        }
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("Something=true", false)]
    [DataRow("CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254", false)]
    [DataRow("AutoConnect=False&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254", false)]
    [DataRow("AutoConnect=TrueX&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254", false)]
    [DataRow("AutoConnect=True&CallbackUri=&CallbackReference=ID%23254", false)]
    [DataRow("AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=", false)]
    [DataRow("AutoConnect=True&CallbackReference=ID%23254", false)]
    [DataRow("AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254", false)]
    [DataRow("AutoConnect=TrueX&CallbackUri=localhost&CallbackReference=MyTest", false)]
    [DataRow(AutoConnectQueryString, true)]
    [DataRow("CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&AutoConnect=TRUE&CallbackReference=ID%23254", true)]
    [DataRow("callbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&callbackReference=ID%23254&autoConnect=true", true)]
    [DataRow("AutoConnect=True&CallbackUri=http%3A%2F%2Flocalhost&CallbackReference=My%20Test", true)]
    public void JoinAutomatically_UrlQueryString_ReturnsExpectedResult(string queryString, bool expectedResult)
    {
        var target = CreateController(urlQueryString: queryString);

        var result = target.JoinAutomatically;

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public async Task GetCredentials_CredentialsAreStored_ReturnsMemberCredentials()
    {
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        memberCredentialsStore.Setup(o => o.GetCredentialsAsync(true)).ReturnsAsync(memberCredentials);
        var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

        var result = await target.GetCredentials();

        memberCredentialsStore.Verify(o => o.GetCredentialsAsync(true));
        Assert.AreEqual(memberCredentials, result);
    }

    [TestMethod]
    public async Task GetCredentials_NoCredentialsAreStored_ReturnsNull()
    {
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        memberCredentialsStore.Setup(o => o.GetCredentialsAsync(true)).ReturnsAsync(default(MemberCredentials?));
        var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

        var result = await target.GetCredentials();

        memberCredentialsStore.Verify(o => o.GetCredentialsAsync(true));
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
    [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
    public async Task JoinTeam_MemberName_JoinTeamOnService(string memberName, bool asObserver)
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamResult);
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(planningPokerService: planningPokerService.Object, serviceTimeProvider: serviceTimeProvider.Object);

        await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

        planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver, It.IsAny<CancellationToken>()));
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
    [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
    public async Task JoinTeam_MemberName_ReturnsTrue(string memberName, bool asObserver)
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var target = CreateController(teamResult: teamResult);

        var result = await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow("", PlanningPokerData.MemberName, false, DisplayName = "Team name is Empty and not observer")]
    [DataRow(null, PlanningPokerData.MemberName, false, DisplayName = "Team name is Null and not observer")]
    [DataRow(PlanningPokerData.TeamName, "", false, DisplayName = "Member name is Empty and not observer")]
    [DataRow(PlanningPokerData.TeamName, null, false, DisplayName = "Member name is Null and not observer")]
    [DataRow("", PlanningPokerData.MemberName, true, DisplayName = "Team name is Empty and observer")]
    [DataRow(null, PlanningPokerData.MemberName, true, DisplayName = "Team name is Null and observer")]
    [DataRow(PlanningPokerData.TeamName, "", true, DisplayName = "Member name is Empty and observer")]
    [DataRow(PlanningPokerData.TeamName, null, true, DisplayName = "Member name is Null and observer")]
    public async Task JoinTeam_TeamNameOrMemberNameIsEmpty_ReturnsFalse(string teamName, string memberName, bool asObserver)
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var target = CreateController(planningPokerService: planningPokerService.Object);

        var result = await target.JoinTeam(teamName, memberName, asObserver);

        Assert.IsFalse(result);
        planningPokerService.Verify(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    public async Task JoinTeam_ServiceReturnsTeam_InitializePlanningPokerController()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, teamResult: teamResult);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(teamResult, PlanningPokerData.MemberName, null));
    }

    [TestMethod]
    public async Task JoinTeam_ServiceReturnsTeamAndUrlHasCallback_InitializePlanningPokerController()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        ApplicationCallbackReference? applicationCallbackReference = null;
        planningPokerInitializer.Setup(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()))
            .Callback<TeamResult, string, ApplicationCallbackReference?>((_, _, r) => applicationCallbackReference = r);
        var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, teamResult: teamResult, urlQueryString: AutoConnectQueryString);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(teamResult, PlanningPokerData.MemberName, It.IsAny<ApplicationCallbackReference?>()));
        Assert.IsNotNull(applicationCallbackReference);
        Assert.AreEqual(new Uri("https://www.testweb.net/some/item?id=254"), applicationCallbackReference.Url);
        Assert.AreEqual("ID#254", applicationCallbackReference.Reference);
    }

    [TestMethod]
    public async Task JoinTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl);
        var target = CreateController(navigationManager: navigationManager.Object, teamResult: teamResult);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer"));
    }

    [TestMethod]
    public async Task JoinTeam_ServiceReturnsTeamAndUrlHasCallback_NavigatesToPlanningPoker()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(navigationManager: navigationManager.Object, teamResult: teamResult);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        var navigateToUri = "PlanningPoker/Test%20team/Test%20observer?CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254";
        navigationManager.Verify(o => o.NavigateTo(navigateToUri));
    }

    [TestMethod]
    public async Task JoinTeam_ServiceThrowsException_ReturnsFalse()
    {
        var target = CreateController(exception: new PlanningPokerException(ErrorMessage));

        var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task JoinTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
    {
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()), Times.Never());
    }

    [TestMethod]
    public async Task JoinTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
    {
        var navigationManager = new Mock<INavigationManager>();
        var target = CreateController(navigationManager: navigationManager.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task JoinTeam_ServiceThrowsExceptionWithErrorCode_ShowsMessage()
    {
        var exception = new PlanningPokerException(ErrorMessage, ErrorCodes.ScrumTeamNotExist, PlanningPokerData.TeamName);
        var messageBoxService = new Mock<IMessageBoxService>();

        var target = CreateController(messageBoxService: messageBoxService.Object, exception: exception);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        messageBoxService.Verify(o => o.ShowMessage("Scrum Team 'Test team' does not exist.", "Error"));
    }

    [TestMethod]
    public async Task JoinTeam_ServiceThrowsExceptionWithoutErrorCode_ShowsMessage()
    {
        var messageBoxService = new Mock<IMessageBoxService>();

        var target = CreateController(messageBoxService: messageBoxService.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        messageBoxService.Verify(o => o.ShowMessage(ErrorMessage, "Error"));
    }

    [TestMethod]
    public async Task JoinTeam_TeamName_ShowsBusyIndicator()
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var busyIndicatorService = new Mock<IBusyIndicatorService>();
        var joinTeamTask = new TaskCompletionSource<TeamResult>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(joinTeamTask.Task);
        var busyIndicatorInstance = new Mock<IDisposable>();
        busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance.Object);
        var target = CreateController(planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

        var result = target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        busyIndicatorService.Verify(o => o.Show());
        busyIndicatorInstance.Verify(o => o.Dispose(), Times.Never());

        joinTeamTask.SetResult(PlanningPokerData.GetTeamResult());
        await result;

        busyIndicatorInstance.Verify(o => o.Dispose());
    }

    [TestMethod]
    public async Task JoinTeam_MemberAlreadyExistsButUserDoesNotReconnect_DoesNotReconnect()
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage, ErrorCodes.MemberAlreadyExists, PlanningPokerData.MemberName));
        var messageBoxService = new Mock<IMessageBoxService>();
        SetupReconnectMessageBox(messageBoxService, false);

        var target = CreateController(planningPokerService: planningPokerService.Object, messageBoxService: messageBoxService.Object);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        var userMessage = ReconnectErrorMessage + Environment.NewLine + "Do you want to reconnect?";
        messageBoxService.Verify(o => o.ShowMessage(userMessage, "Reconnect", "Reconnect"));
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
    [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
    public async Task ReconnectTeam_MemberName_JoinTeamOnService(string memberName, bool asObserver)
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage, ErrorCodes.MemberAlreadyExists, memberName));
        planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconnectTeamResult);
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(memberExistsError: true, planningPokerService: planningPokerService.Object, serviceTimeProvider: serviceTimeProvider.Object);

        await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

        planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver, It.IsAny<CancellationToken>()));
        planningPokerService.Verify(o => o.ReconnectTeam(PlanningPokerData.TeamName, memberName, It.IsAny<CancellationToken>()));
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    [DataRow(PlanningPokerData.MemberName, false, DisplayName = "Member name")]
    [DataRow(PlanningPokerData.ObserverName, true, DisplayName = "Observer name")]
    public async Task ReconnectTeam_MemberName_ReturnsTrue(string memberName, bool asObserver)
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var target = CreateController(memberExistsError: true, reconnectTeamResult: reconnectTeamResult);

        var result = await target.JoinTeam(PlanningPokerData.TeamName, memberName, asObserver);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceReturnsTeam_InitializePlanningPokerController()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(
            memberExistsError: true,
            planningPokerInitializer: planningPokerInitializer.Object,
            reconnectTeamResult: reconnectTeamResult,
            urlQueryString: "CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254");

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, null));
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceReturnsTeamAndUrlHasCallback_InitializePlanningPokerController()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        ApplicationCallbackReference? applicationCallbackReference = null;
        planningPokerInitializer.Setup(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()))
            .Callback<TeamResult, string, ApplicationCallbackReference?>((_, _, r) => applicationCallbackReference = r);
        var target = CreateController(
            memberExistsError: true,
            planningPokerInitializer: planningPokerInitializer.Object,
            reconnectTeamResult: reconnectTeamResult,
            urlQueryString: "CallbackUri=https%3A%2F%2Fwww.testweb.net&Param2=3&CallbackReference=My%20Test");

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, It.IsAny<ApplicationCallbackReference>()));
        Assert.IsNotNull(applicationCallbackReference);
        Assert.AreEqual(new Uri("https://www.testweb.net"), applicationCallbackReference.Url);
        Assert.AreEqual("My Test", applicationCallbackReference.Reference);
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(memberExistsError: true, navigationManager: navigationManager.Object, reconnectTeamResult: reconnectTeamResult);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer"));
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceReturnsTeamAndUrlHasCallback_NavigatesToPlanningPoker()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?CallbackUri=https%3A%2F%2Fwww.testweb.net&Param2=3&CallbackReference=My%20Test";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(memberExistsError: true, navigationManager: navigationManager.Object, reconnectTeamResult: reconnectTeamResult);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer?CallbackUri=https%3A%2F%2Fwww.testweb.net%2F&CallbackReference=My%20Test"));
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceThrowsException_ReturnsFalse()
    {
        var target = CreateController(memberExistsError: true, exception: new PlanningPokerException(ErrorMessage));

        var result = await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
    {
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(memberExistsError: true, planningPokerInitializer: planningPokerInitializer.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ReconnectTeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()), Times.Never());
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
    {
        var navigationManager = new Mock<INavigationManager>();
        var target = CreateController(memberExistsError: true, navigationManager: navigationManager.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceThrowsExceptionWithErrorCode_ShowsMessage()
    {
        var exception = new PlanningPokerException(ErrorMessage, ErrorCodes.ScrumTeamNotExist, PlanningPokerData.TeamName);
        var messageBoxService = new Mock<IMessageBoxService>();
        SetupReconnectMessageBox(messageBoxService, true);

        var target = CreateController(memberExistsError: true, messageBoxService: messageBoxService.Object, exception: exception);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        messageBoxService.Verify(o => o.ShowMessage("Scrum Team 'Test team' does not exist.", "Error"));
    }

    [TestMethod]
    public async Task ReconnectTeam_ServiceThrowsExceptionWithoutErrorCode_ShowsMessage()
    {
        var messageBoxService = new Mock<IMessageBoxService>();
        SetupReconnectMessageBox(messageBoxService, true);

        var target = CreateController(memberExistsError: true, messageBoxService: messageBoxService.Object, exception: new PlanningPokerException(ErrorMessage));

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        messageBoxService.Verify(o => o.ShowMessage(ErrorMessage, "Error"));
    }

    [TestMethod]
    public async Task ReconnectTeam_TeamName_ShowsBusyIndicator()
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var busyIndicatorService = new Mock<IBusyIndicatorService>();
        var joinTeamTask = new TaskCompletionSource<TeamResult>();
        var reconnectTeamTask = new TaskCompletionSource<ReconnectTeamResult>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(joinTeamTask.Task);
        planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(reconnectTeamTask.Task);
        var target = CreateController(memberExistsError: true, planningPokerService: planningPokerService.Object, busyIndicatorService: busyIndicatorService.Object);

        var busyIndicatorInstance1 = new Mock<IDisposable>();
        busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance1.Object);

        var result = target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName, true);

        busyIndicatorService.Verify(o => o.Show());
        busyIndicatorInstance1.Verify(o => o.Dispose(), Times.Never());

        var busyIndicatorInstance2 = new Mock<IDisposable>();
        busyIndicatorService.Setup(o => o.Show()).Returns(busyIndicatorInstance2.Object);

        joinTeamTask.SetException(new PlanningPokerException(ReconnectErrorMessage, ErrorCodes.MemberAlreadyExists, PlanningPokerData.ObserverName));

        busyIndicatorService.Verify(o => o.Show(), Times.Exactly(2));
        busyIndicatorInstance1.Verify(o => o.Dispose());
        busyIndicatorInstance2.Verify(o => o.Dispose(), Times.Never());

        reconnectTeamTask.SetResult(PlanningPokerData.GetReconnectTeamResult());
        await result;

        busyIndicatorInstance2.Verify(o => o.Dispose());
    }

    [TestMethod]
    public async Task TryReconnectTeam_TeamNameAndMemberName_LoadsMemberCredentialsFromStore()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        memberCredentialsStore.Setup(o => o.GetCredentialsAsync(false)).ReturnsAsync(memberCredentials);
        var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object, memberExistsError: true, reconnectTeamResult: reconnectTeamResult);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        memberCredentialsStore.Verify(o => o.GetCredentialsAsync(false));
    }

    [TestMethod]
    [DataRow(null, PlanningPokerData.MemberName, DisplayName = "TeamName is null.")]
    [DataRow("", PlanningPokerData.MemberName, DisplayName = "TeamName is empty.")]
    [DataRow(PlanningPokerData.TeamName, null, DisplayName = "MemberName is null.")]
    [DataRow(PlanningPokerData.TeamName, "", DisplayName = "MemberName is empty.")]
    public async Task TryReconnectTeam_TeamNameOrMemberNameIsEmptyOrNull_DoesNotLoadMemberCredentialsFromStore(string teamName, string memberName)
    {
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        var target = CreateController(memberCredentialsStore: memberCredentialsStore.Object);

        var result = await target.TryAutoConnectTeam(teamName, memberName);

        Assert.IsFalse(result);
        memberCredentialsStore.Verify(o => o.GetCredentialsAsync(It.IsAny<bool>()), Times.Never());
    }

    [TestMethod]
    [DataRow(PlanningPokerData.TeamName, PlanningPokerData.MemberName, DisplayName = "Test team, Test member")]
    [DataRow("test team", "test member", DisplayName = "test team, test member")]
    [DataRow("TEST TEAM", "TEST MEMBER", DisplayName = "TEST TEAM, TEST MEMBER")]
    [DataRow("tEST tEAM", "tEST mEMBER", DisplayName = "TEST TEAM, TEST MEMBER")]
    public async Task TryReconnectTeam_CredentialsAreStored_ReconnectTeamOnService(string teamName, string memberName)
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconnectTeamResult);
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(planningPokerService: planningPokerService.Object, memberCredentials: memberCredentials, serviceTimeProvider: serviceTimeProvider.Object);

        await target.TryAutoConnectTeam(teamName, memberName);

        planningPokerService.Verify(o => o.ReconnectTeam(teamName, memberName, It.IsAny<CancellationToken>()));
        planningPokerService.Verify(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    public async Task TryReconnectTeam_ReconnectTeamIsSuccessful_ReturnsTrue()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var target = CreateController(memberExistsError: true, reconnectTeamResult: reconnectTeamResult, memberCredentials: memberCredentials);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsTrue(result);
    }

    [TestMethod]
    [DataRow("CallbackUri=&CallbackReference=2")]
    [DataRow("AutoConnect=Yes&CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254")]
    public async Task TryReconnectTeam_ReconnectTeamIsSuccessful_InitializePlanningPokerController(string queryString)
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(
            planningPokerInitializer: planningPokerInitializer.Object,
            memberExistsError: true,
            reconnectTeamResult: reconnectTeamResult,
            memberCredentials: memberCredentials,
            urlQueryString: queryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, null));
    }

    [TestMethod]
    public async Task TryReconnectTeam_ReconnectTeamIsSuccessfulAndUrlHasCallback_InitializePlanningPokerController()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        ApplicationCallbackReference? applicationCallbackReference = null;
        planningPokerInitializer.Setup(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()))
            .Callback<TeamResult, string, ApplicationCallbackReference?>((_, _, r) => applicationCallbackReference = r);
        var target = CreateController(
            planningPokerInitializer: planningPokerInitializer.Object,
            memberExistsError: true,
            reconnectTeamResult: reconnectTeamResult,
            memberCredentials: memberCredentials,
            urlQueryString: "CallbackReference=My%20Test&CallbackUri=https%3A%2F%2Fwww.testweb.net%2FIndex%3Fa%3D1%26b%3D2&Param2=3");

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, It.IsAny<ApplicationCallbackReference?>()));
        Assert.IsNotNull(applicationCallbackReference);
        Assert.AreEqual(new Uri("https://www.testweb.net/Index?a=1&b=2"), applicationCallbackReference.Url);
        Assert.AreEqual("My Test", applicationCallbackReference.Reference);
    }

    [TestMethod]
    public async Task TryReconnectTeam_ReconnectTeamIsSuccessful_NavigatesToPlanningPoker()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?CallbackUri=&CallbackReference=";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(
            navigationManager: navigationManager.Object,
            memberExistsError: true,
            reconnectTeamResult: reconnectTeamResult,
            memberCredentials: memberCredentials);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20member"));
    }

    [TestMethod]
    public async Task TryReconnectTeam_ReconnectTeamIsSuccessfulAndUrlHasCallback_NavigatesToPlanningPoker()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?CallbackReference=My%20Test&CallbackUri=https%3A%2F%2Fwww.testweb.net%2FIndex%3Fa%3D1%26b%3D2&Param2=3";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(
            navigationManager: navigationManager.Object,
            memberExistsError: true,
            reconnectTeamResult: reconnectTeamResult,
            memberCredentials: memberCredentials);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20member?CallbackUri=https%3A%2F%2Fwww.testweb.net%2FIndex%3Fa%3D1%26b%3D2&CallbackReference=My%20Test"));
    }

    [TestMethod]
    public async Task TryReconnectTeam_ServiceThrowsException_ReturnsFalse()
    {
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var target = CreateController(memberExistsError: true, exception: new PlanningPokerException(ErrorMessage), memberCredentials: memberCredentials);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TryReconnectTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
    {
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(
            planningPokerInitializer: planningPokerInitializer.Object,
            memberExistsError: true,
            exception: new PlanningPokerException(ErrorMessage),
            memberCredentials: memberCredentials);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ReconnectTeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()), Times.Never());
    }

    [TestMethod]
    public async Task TryReconnectTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
    {
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var navigationManager = new Mock<INavigationManager>();
        var target = CreateController(
            navigationManager: navigationManager.Object,
            memberExistsError: true,
            exception: new PlanningPokerException(ErrorMessage),
            memberCredentials: memberCredentials);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task TryReconnectTeam_ServiceThrowsException_DoesNotShowUserMessage()
    {
        var memberCredentials = PlanningPokerData.GetMemberCredentials();
        var messageBoxService = new Mock<IMessageBoxService>();
        var target = CreateController(
            messageBoxService: messageBoxService.Object,
            memberExistsError: true,
            exception: new PlanningPokerException(ErrorMessage),
            memberCredentials: memberCredentials);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        messageBoxService.Verify(o => o.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        messageBoxService.Verify(o => o.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task TryReconnectTeam_NoMemberCredentialsStored_DoesNotReconnectTeamOnService()
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var target = CreateController(planningPokerService: planningPokerService.Object);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsFalse(result);
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    [DataRow("Test team 2", PlanningPokerData.MemberName, DisplayName = "Test team 2, Test member")]
    [DataRow("Tesu team", PlanningPokerData.MemberName, DisplayName = "Tesu team, Test member")]
    [DataRow("", PlanningPokerData.MemberName, DisplayName = "[empty], Test member")]
    [DataRow(null, PlanningPokerData.MemberName, DisplayName = "[null], Test member")]
    [DataRow(PlanningPokerData.TeamName, "Test member 2", DisplayName = "Test team, Test member 2")]
    [DataRow(PlanningPokerData.TeamName, "Tesu member", DisplayName = "Test team, Tesu member")]
    [DataRow(PlanningPokerData.TeamName, "", DisplayName = "Test team, [empty]")]
    [DataRow(PlanningPokerData.TeamName, null, DisplayName = "Test team, [null]")]
    public async Task TryReconnectTeam_MemberCredentialsDoNotMatch_DoesNotReconnectTeamOnService(string teamName, string memberName)
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var memberCredentials = new MemberCredentials
        {
            TeamName = teamName,
            MemberName = memberName
        };
        var target = CreateController(planningPokerService: planningPokerService.Object, memberCredentials: memberCredentials);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsFalse(result);
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    [DataRow(null, PlanningPokerData.MemberName, DisplayName = "TeamName is null.")]
    [DataRow("", PlanningPokerData.MemberName, DisplayName = "TeamName is empty.")]
    [DataRow(PlanningPokerData.TeamName, null, DisplayName = "MemberName is null.")]
    [DataRow(PlanningPokerData.TeamName, "", DisplayName = "MemberName is empty.")]
    public async Task AutoJoinTeam_TeamNameOrMemberNameIsEmptyOrNull_DoesNotJoinTeam(string teamName, string memberName)
    {
        var planningPokerService = new Mock<IPlanningPokerClient>();
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(planningPokerService: planningPokerService.Object, serviceTimeProvider: serviceTimeProvider.Object, urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(teamName, memberName);

        Assert.IsFalse(result);
        planningPokerService.Verify(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberName_JoinTeamOnService()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(teamResult);
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(planningPokerService: planningPokerService.Object, serviceTimeProvider: serviceTimeProvider.Object, urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, false, It.IsAny<CancellationToken>()));
        planningPokerService.Verify(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberName_ReturnsTrue()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var target = CreateController(teamResult: teamResult, urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AutoJoinTeam_ServiceReturnsTeam_InitializePlanningPokerController()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        ApplicationCallbackReference? applicationCallbackReference = null;
        planningPokerInitializer.Setup(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()))
            .Callback<TeamResult, string, ApplicationCallbackReference?>((_, _, r) => applicationCallbackReference = r);
        var urlQueryString = "CallbackUri=https%3A%2F%2Fwww.testweb.net&NoValue&CallbackReference=ID%3D254&AutoConnect=True";
        var target = CreateController(planningPokerInitializer: planningPokerInitializer.Object, teamResult: teamResult, urlQueryString: urlQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(teamResult, PlanningPokerData.MemberName, It.IsAny<ApplicationCallbackReference?>()));
        Assert.IsNotNull(applicationCallbackReference);
        Assert.AreEqual(new Uri("https://www.testweb.net/"), applicationCallbackReference.Url);
        Assert.AreEqual("ID=254", applicationCallbackReference.Reference);
    }

    [TestMethod]
    public async Task AutoJoinTeam_ServiceReturnsTeam_NavigatesToPlanningPoker()
    {
        var teamResult = PlanningPokerData.GetTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        var urlQueryString = "?CallbackUri=https%3A%2F%2Fwww.testweb.net&AutoConnect=True&CallbackReference=ID%3D254";
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + urlQueryString);
        var target = CreateController(navigationManager: navigationManager.Object, teamResult: teamResult);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName);

        var navigateToUri = "PlanningPoker/Test%20team/Test%20observer?CallbackUri=https%3A%2F%2Fwww.testweb.net%2F&CallbackReference=ID%3D254";
        navigationManager.Verify(o => o.NavigateTo(navigateToUri));
    }

    [TestMethod]
    public async Task AutoJoinTeam_ServiceThrowsException_ReturnsFalse()
    {
        var target = CreateController(exception: new PlanningPokerException(ErrorMessage), urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AutoJoinTeam_ServiceThrowsException_DoesNotInitializePlanningPokerController()
    {
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(
            planningPokerInitializer: planningPokerInitializer.Object,
            exception: new PlanningPokerException(ErrorMessage),
            urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()), Times.Never());
    }

    [TestMethod]
    public async Task AutoJoinTeam_ServiceThrowsException_DoesNotNavigateToPlanningPoker()
    {
        var navigationManager = new Mock<INavigationManager>();
        var target = CreateController(
            navigationManager: navigationManager.Object,
            exception: new PlanningPokerException(ErrorMessage),
            urlQueryString: AutoConnectQueryString);

        await target.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName, false);

        navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExists_ReconnectTeamOnService()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerService = new Mock<IPlanningPokerClient>();
        planningPokerService.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage, ErrorCodes.MemberAlreadyExists, PlanningPokerData.ScrumMasterName));
        planningPokerService.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reconnectTeamResult);
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var target = CreateController(
            memberExistsError: true,
            planningPokerService: planningPokerService.Object,
            serviceTimeProvider: serviceTimeProvider.Object,
            urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        planningPokerService.Verify(o => o.JoinTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, false, It.IsAny<CancellationToken>()));
        planningPokerService.Verify(o => o.ReconnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName, It.IsAny<CancellationToken>()));
        serviceTimeProvider.Verify(o => o.UpdateServiceTimeOffset(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExists_ReturnsTrue()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var target = CreateController(memberExistsError: true, reconnectTeamResult: reconnectTeamResult, urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExists_InitializePlanningPokerController()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        ApplicationCallbackReference? applicationCallbackReference = null;
        planningPokerInitializer.Setup(o => o.InitializeTeam(It.IsAny<TeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()))
            .Callback<TeamResult, string, ApplicationCallbackReference?>((_, _, r) => applicationCallbackReference = r);
        var target = CreateController(
            memberExistsError: true,
            planningPokerInitializer: planningPokerInitializer.Object,
            reconnectTeamResult: reconnectTeamResult,
            urlQueryString: "FirstParam&CallbackReference=My%20Test&AutoConnect=True&CallbackUri=https%3A%2F%2Fwww.testweb.net&Param2=3");

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(reconnectTeamResult, PlanningPokerData.MemberName, It.IsAny<ApplicationCallbackReference>()));
        Assert.IsNotNull(applicationCallbackReference);
        Assert.AreEqual(new Uri("https://www.testweb.net"), applicationCallbackReference.Url);
        Assert.AreEqual("My Test", applicationCallbackReference.Reference);
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExists_NavigatesToPlanningPoker()
    {
        var reconnectTeamResult = PlanningPokerData.GetReconnectTeamResult();
        var navigationManager = new Mock<INavigationManager>();
        navigationManager.SetupGet(o => o.Uri).Returns(PageUrl + '?' + AutoConnectQueryString);
        var target = CreateController(memberExistsError: true, navigationManager: navigationManager.Object, reconnectTeamResult: reconnectTeamResult);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ObserverName);

        navigationManager.Verify(o => o.NavigateTo("PlanningPoker/Test%20team/Test%20observer?CallbackUri=https%3A%2F%2Fwww.testweb.net%2Fsome%2Fitem%3Fid%3D254&CallbackReference=ID%23254"));
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExistsAndReconnectThrowsException_ReturnsFalse()
    {
        var target = CreateController(memberExistsError: true, exception: new PlanningPokerException(ErrorMessage), urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExistsAndReconnectThrowsException_DoesNotInitializePlanningPokerController()
    {
        var planningPokerInitializer = new Mock<IPlanningPokerInitializer>();
        var target = CreateController(
            memberExistsError: true,
            planningPokerInitializer: planningPokerInitializer.Object,
            exception: new PlanningPokerException(ErrorMessage),
            urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        planningPokerInitializer.Verify(o => o.InitializeTeam(It.IsAny<ReconnectTeamResult>(), It.IsAny<string>(), It.IsAny<ApplicationCallbackReference?>()), Times.Never());
    }

    [TestMethod]
    public async Task AutoJoinTeam_MemberAlreadyExistsAndReconnectThrowsException_DoesNotNavigateToPlanningPoker()
    {
        var navigationManager = new Mock<INavigationManager>();
        var target = CreateController(
            memberExistsError: true,
            navigationManager: navigationManager.Object,
            exception: new PlanningPokerException(ErrorMessage),
            urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.MemberName);

        navigationManager.Verify(o => o.NavigateTo(It.IsAny<string>()), Times.Never());
    }

    [TestMethod]
    public async Task AutoJoinTeam_ScrumTeamDoesNotExist_ShowsMessage()
    {
        var exception = new PlanningPokerException(ErrorMessage, ErrorCodes.ScrumTeamNotExist, PlanningPokerData.TeamName);
        var messageBoxService = new Mock<IMessageBoxService>();

        var target = CreateController(messageBoxService: messageBoxService.Object, exception: exception, urlQueryString: AutoConnectQueryString);

        await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        messageBoxService.Verify(o => o.ShowMessage($"Scrum Team 'Test team' does not exist.{Environment.NewLine}Please, create new Scrum Team by confirming Create form.", "Error"));
    }

    [TestMethod]
    public async Task AutoJoinTeam_ScrumTeamDoesNotExist_ReturnsFalse()
    {
        var exception = new PlanningPokerException(ErrorMessage, ErrorCodes.ScrumTeamNotExist, PlanningPokerData.TeamName);
        var messageBoxService = new Mock<IMessageBoxService>();

        var target = CreateController(messageBoxService: messageBoxService.Object, exception: exception, urlQueryString: AutoConnectQueryString);

        var result = await target.TryAutoConnectTeam(PlanningPokerData.TeamName, PlanningPokerData.ScrumMasterName);

        Assert.IsFalse(result);
    }

    private static JoinTeamController CreateController(
        IPlanningPokerInitializer? planningPokerInitializer = null,
        IPlanningPokerClient? planningPokerService = null,
        IMessageBoxService? messageBoxService = null,
        IBusyIndicatorService? busyIndicatorService = null,
        INavigationManager? navigationManager = null,
        IMemberCredentialsStore? memberCredentialsStore = null,
        IServiceTimeProvider? serviceTimeProvider = null,
        bool memberExistsError = false,
        TeamResult? teamResult = null,
        ReconnectTeamResult? reconnectTeamResult = null,
        PlanningPokerException? exception = null,
        MemberCredentials? memberCredentials = null,
        string? urlQueryString = null)
    {
        if (planningPokerInitializer == null)
        {
            var planningPokerInitializerMock = new Mock<IPlanningPokerInitializer>();
            planningPokerInitializer = planningPokerInitializerMock.Object;
        }

        if (planningPokerService == null)
        {
            var planningPokerServiceMock = new Mock<IPlanningPokerClient>();
            var joinSetup = planningPokerServiceMock.Setup(o => o.JoinTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()));
            var reconnectSetup = planningPokerServiceMock.Setup(o => o.ReconnectTeam(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));
            if (memberExistsError)
            {
                joinSetup.ThrowsAsync(new PlanningPokerException(ReconnectErrorMessage, ErrorCodes.MemberAlreadyExists, PlanningPokerData.MemberName));
                if (exception == null)
                {
                    reconnectSetup!.ReturnsAsync(reconnectTeamResult);
                }
                else
                {
                    reconnectSetup.ThrowsAsync(exception);
                }
            }
            else
            {
                if (exception == null)
                {
                    joinSetup!.ReturnsAsync(teamResult);
                }
                else
                {
                    joinSetup.ThrowsAsync(exception);
                }
            }

            planningPokerService = planningPokerServiceMock.Object;
        }

        if (messageBoxService == null)
        {
            var messageBoxServiceMock = new Mock<IMessageBoxService>();
            if (memberExistsError)
            {
                SetupReconnectMessageBox(messageBoxServiceMock, true);
            }

            messageBoxService = messageBoxServiceMock.Object;
        }

        if (busyIndicatorService == null)
        {
            var busyIndicatorServiceMock = new Mock<IBusyIndicatorService>();
            busyIndicatorService = busyIndicatorServiceMock.Object;
        }

        if (navigationManager == null)
        {
            var navigationManagerMock = new Mock<INavigationManager>();
            var url = PageUrl;
            if (!string.IsNullOrEmpty(urlQueryString))
            {
                url += '?' + urlQueryString;
            }

            navigationManagerMock.SetupGet(o => o.Uri).Returns(url);
            navigationManager = navigationManagerMock.Object;
        }

        if (memberCredentialsStore == null)
        {
            var memberCredentialsStoreMock = new Mock<IMemberCredentialsStore>();
            memberCredentialsStoreMock.Setup(o => o.GetCredentialsAsync(false)).ReturnsAsync(memberCredentials);
            memberCredentialsStore = memberCredentialsStoreMock.Object;
        }

        if (serviceTimeProvider == null)
        {
            var serviceTimeProviderMock = new Mock<IServiceTimeProvider>();
            serviceTimeProvider = serviceTimeProviderMock.Object;
        }

        return new JoinTeamController(
            planningPokerService,
            planningPokerInitializer,
            messageBoxService,
            busyIndicatorService,
            navigationManager,
            memberCredentialsStore,
            serviceTimeProvider);
    }

    private static void SetupReconnectMessageBox(Mock<IMessageBoxService> messageBoxService, bool result)
    {
        messageBoxService.Setup(o => o.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(result);
    }
}
