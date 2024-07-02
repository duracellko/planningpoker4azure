using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duracellko.PlanningPoker.Client.Controllers;
using Duracellko.PlanningPoker.Client.Service;
using Duracellko.PlanningPoker.Client.UI;
using Duracellko.PlanningPoker.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Client.Test.Controllers;

[TestClass]
public class PlanningPokerControllerTestEstimationSummary
{
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_SingleEstimation_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 5 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.AreEqual(5, target.EstimationSummary.Average);
        Assert.AreEqual(5, target.EstimationSummary.Median);
        Assert.AreEqual(5, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(true, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_11Estimations_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 13, 2, 5, 40, 8, 0, 1, 0.5, 20, 100, 3 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.AreEqual(17.5, target.EstimationSummary.Average);
        Assert.AreEqual(5, target.EstimationSummary.Median);
        Assert.AreEqual(192.5, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(!useEstimationEndedMessage, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_8Estimations_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 13, 2, 2, 0.5, 20, 0.5, 0.5, 0 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage);

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.IsNotNull(target.EstimationSummary.Average);
        Assert.AreEqual(4.8125, target.EstimationSummary.Average.Value, 1E-15);
        Assert.AreEqual(1.25, target.EstimationSummary.Median);
        Assert.AreEqual(38.5, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(false, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_EstimationsWithNonNumericValues_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { null, 0, 20, double.PositiveInfinity, 0.5, double.PositiveInfinity, null };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.IsNotNull(target.EstimationSummary.Average);
        Assert.AreEqual(6.833333333333333, target.EstimationSummary.Average.Value, 1E-15);
        Assert.AreEqual(0.5, target.EstimationSummary.Median);
        Assert.AreEqual(20.5, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(!useEstimationEndedMessage, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_MemberNotEstimated_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 20, double.NaN, 5, double.NaN };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.AreEqual(12.5, target.EstimationSummary.Average);
        Assert.AreEqual(12.5, target.EstimationSummary.Median);
        Assert.AreEqual(25, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(!useEstimationEndedMessage, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_EstimationsWithNegativeNumbers_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 13, -999509, 2, -999507, 8, -999505, 5, -999509, 20 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.AreEqual(9.6, target.EstimationSummary.Average);
        Assert.AreEqual(8, target.EstimationSummary.Median);
        Assert.AreEqual(48, target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(!useEstimationEndedMessage, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_EstimationsWithOnlyNegativeNumbers_CalculatesAverage(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { -999509, -999507, -999505, -999509 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage);

        target.ShowEstimationSummary();

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNotNull(target.EstimationSummary);
        Assert.IsNull(target.EstimationSummary.Average);
        Assert.IsNull(target.EstimationSummary.Median);
        Assert.IsNull(target.EstimationSummary.Sum);
        AssertEstimationSummaryGetValue(target);
        AssertCanPostEstimationResult(false, target);
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShowEstimationSummary_StartNewEstimation_EstimationSummaryIsNull(bool useEstimationEndedMessage)
    {
        var estimations = new double?[] { 13, 2, 2, 0.5, 8, 0.5, 5, 0.5, 20 };
        using var target = await CreateControllerWithEstimations(estimations, useEstimationEndedMessage, applicationCallback: CreateApplicationCallback());

        target.ShowEstimationSummary();

        var estimationStartedMessage = new Message
        {
            Id = 10,
            Type = MessageType.EstimationStarted
        };
        target.ProcessMessages(new Message[] { estimationStartedMessage });

        Assert.IsFalse(target.CanShowEstimationSummary);
        Assert.IsNull(target.EstimationSummary);
        AssertCanPostEstimationResult(false, target);
    }

    [DataTestMethod]
    [DataRow(EstimationSummaryFunction.Average, 17.5)]
    [DataRow(EstimationSummaryFunction.Median, 5)]
    [DataRow(EstimationSummaryFunction.Sum, 192.5)]
    public async Task PostEstimationResult_EstimationSummaryFunction_PostsEstimationSummaryValue(
        EstimationSummaryFunction function,
        double expectedEstimation)
    {
        var estimations = new double?[] { 13, 2, null, 5, 40, 8, 0, double.PositiveInfinity, 1, 0.5, 20, 100, 3, double.NaN };
        var applicationCallback = CreateApplicationCallback();
        var applicationIntegrationService = new Mock<IApplicationIntegrationService>();
        using var target = await CreateControllerWithEstimations(
            estimations,
            false,
            applicationCallback: applicationCallback,
            applicationIntegrationService: applicationIntegrationService.Object);
        target.ShowEstimationSummary();

        await target.PostEstimationResult(function);

        applicationIntegrationService.Verify(o => o.PostEstimationResult(expectedEstimation, applicationCallback));
    }

    private static async Task<PlanningPokerController> CreateControllerWithEstimations(
        IReadOnlyList<double?> estimations,
        bool useEstimationEndedMessage,
        ApplicationCallbackReference? applicationCallback = null,
        IApplicationIntegrationService? applicationIntegrationService = null)
    {
        if (useEstimationEndedMessage)
        {
            return await CreateControllerWithEstimations(estimations, applicationCallback, applicationIntegrationService);
        }
        else
        {
            return await CreateControllerWithEstimationEndedMessage(estimations, applicationCallback, applicationIntegrationService);
        }
    }

    private static async Task<PlanningPokerController> CreateControllerWithEstimations(
        IReadOnlyList<double?> estimations,
        ApplicationCallbackReference? applicationCallback,
        IApplicationIntegrationService? applicationIntegrationService)
    {
        var scrumTeam = GetScrumTeamWithEstimations(estimations);
        var teamResult = new TeamResult
        {
            ScrumTeam = scrumTeam,
            SessionId = Guid.NewGuid()
        };

        var memberName = PlanningPokerData.ScrumMasterName;
        if (scrumTeam.Members.Count > 0)
        {
            memberName = scrumTeam.Members[0].Name;
        }

        var result = CreateController(applicationIntegrationService);
        await result.InitializeTeam(teamResult, memberName, applicationCallback);
        return result;
    }

    private static async Task<PlanningPokerController> CreateControllerWithEstimationEndedMessage(
        IReadOnlyList<double?> estimations,
        ApplicationCallbackReference? applicationCallback,
        IApplicationIntegrationService? applicationIntegrationService)
    {
        var scrumTeam = PlanningPokerData.GetScrumTeam(estimations.Count - 1);
        var teamResult = new TeamResult
        {
            ScrumTeam = scrumTeam,
            SessionId = Guid.NewGuid()
        };

        var controller = CreateController(applicationIntegrationService);
        await controller.InitializeTeam(teamResult, PlanningPokerData.ScrumMasterName, applicationCallback);

        var estimationStartedMessage = new Message
        {
            Id = 1,
            Type = MessageType.EstimationStarted
        };
        controller.ProcessMessages(new Message[] { estimationStartedMessage });

        var estimationResultMessage = new EstimationResultMessage
        {
            Id = 2,
            Type = MessageType.EstimationEnded,
            EstimationResult = GetScrumTeamEstimations(scrumTeam, estimations)
        };
        controller.ProcessMessages(new Message[] { estimationResultMessage });

        return controller;
    }

    private static PlanningPokerController CreateController(IApplicationIntegrationService? applicationIntegrationService = null)
    {
        var planningPokerClient = new Mock<IPlanningPokerClient>();
        var busyIndicator = new Mock<IBusyIndicatorService>();
        var memberCredentialsStore = new Mock<IMemberCredentialsStore>();
        var timerFactory = new Mock<ITimerFactory>();
        var dateTimeProvider = new DateTimeProviderMock();
        var serviceTimeProvider = new Mock<IServiceTimeProvider>();
        var timerSettingsRepository = new Mock<ITimerSettingsRepository>();

        if (applicationIntegrationService == null)
        {
            var applicationIntegrationServiceMock = new Mock<IApplicationIntegrationService>();
            applicationIntegrationService = applicationIntegrationServiceMock.Object;
        }

        return new PlanningPokerController(
            planningPokerClient.Object,
            busyIndicator.Object,
            memberCredentialsStore.Object,
            timerFactory.Object,
            dateTimeProvider,
            serviceTimeProvider.Object,
            timerSettingsRepository.Object,
            applicationIntegrationService);
    }

    private static ScrumTeam GetScrumTeamWithEstimations(IReadOnlyList<double?> estimations)
    {
        var scrumTeam = PlanningPokerData.GetScrumTeam(estimations.Count - 1);
        scrumTeam.State = TeamState.EstimationFinished;
        scrumTeam.EstimationResult = GetScrumTeamEstimations(scrumTeam, estimations);
        return scrumTeam;
    }

    private static List<EstimationResultItem> GetScrumTeamEstimations(ScrumTeam scrumTeam, IReadOnlyList<double?> estimations)
    {
        var estimationResult = new List<EstimationResultItem>();
        var estimationResultItem = new EstimationResultItem
        {
            Member = new TeamMember
            {
                Name = scrumTeam.ScrumMaster!.Name,
                Type = scrumTeam.ScrumMaster.Type
            },
            Estimation = new Estimation
            {
                Value = estimations[0]
            }
        };
        estimationResult.Add(estimationResultItem);

        for (int i = 0; i < scrumTeam.Members.Count; i++)
        {
            var member = scrumTeam.Members[i];
            estimationResultItem = new EstimationResultItem
            {
                Member = new TeamMember
                {
                    Name = member.Name,
                    Type = member.Type
                },
                Estimation = new Estimation
                {
                    Value = estimations[i + 1]
                }
            };
            estimationResult.Add(estimationResultItem);
        }

        return estimationResult;
    }

    private static ApplicationCallbackReference CreateApplicationCallback()
    {
        return new ApplicationCallbackReference(new Uri("https://www.duracellko.net/"), "My estimation");
    }

    private static void AssertEstimationSummaryGetValue(PlanningPokerController controller)
    {
        var estimationSummary = controller.EstimationSummary;
        Assert.IsNotNull(estimationSummary);
        Assert.AreEqual(estimationSummary.Average, estimationSummary.GetValue(EstimationSummaryFunction.Average));
        Assert.AreEqual(estimationSummary.Median, estimationSummary.GetValue(EstimationSummaryFunction.Median));
        Assert.AreEqual(estimationSummary.Sum, estimationSummary.GetValue(EstimationSummaryFunction.Sum));
    }

    private static void AssertCanPostEstimationResult(bool expected, PlanningPokerController controller)
    {
        Assert.AreEqual(expected, controller.CanPostEstimationResult(EstimationSummaryFunction.Average));
        Assert.AreEqual(expected, controller.CanPostEstimationResult(EstimationSummaryFunction.Median));
        Assert.AreEqual(expected, controller.CanPostEstimationResult(EstimationSummaryFunction.Sum));
    }
}
