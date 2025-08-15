﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Domain.Test;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Controllers;

[TestClass]
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Single use of arrays in tests.")]
public class PlanningPokerControllerWithRepositoryTest
{
    [TestMethod]
    public void ScrumTeamNames_2TeamsInRepository_Returns2Teams()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.SetupGet(r => r.ScrumTeamNames).Returns(["team1", "team2"]);
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        var result = target.ScrumTeamNames;

        // Verify
        repository.VerifyGet(r => r.ScrumTeamNames);
        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(new string[] { "team1", "team2" }, result.ToList());
    }

    [TestMethod]
    public void ScrumTeamNames_2TeamsInRepositoryAnd2TeamCreated_Returns2Teams()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.SetupGet(r => r.ScrumTeamNames).Returns(["team1", "team2"]);
        repository.Setup(r => r.LoadScrumTeam("team1")).Returns((ScrumTeam?)null);
        repository.Setup(r => r.LoadScrumTeam("team3")).Returns((ScrumTeam?)null);
        var target = CreatePlanningPokerController(repository: repository.Object);
        using (target.CreateScrumTeam("team1", "master", Deck.Standard))
        {
            // Create team only
        }

        using (target.CreateScrumTeam("team3", "master", Deck.Standard))
        {
            // Create team only
        }

        // Act
        var result = target.ScrumTeamNames;

        // Verify
        repository.VerifyGet(r => r.ScrumTeamNames);
        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(new string[] { "team1", "team2", "team3" }, result.ToList());
    }

    [TestMethod]
    public void ScrumTeamNames_AllEmpty_ReturnsZeroTeams()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.SetupGet(r => r.ScrumTeamNames).Returns([]);
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        var result = target.ScrumTeamNames;

        // Verify
        repository.VerifyGet(r => r.ScrumTeamNames);
        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(Array.Empty<string>(), result.ToList());
    }

    [TestMethod]
    public void CreateScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);

        // Verify
        Assert.IsNotNull(teamLock);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void CreateScrumTeam_TeamInRepository_DoesNotCreateNewTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.CreateScrumTeam("team", "master", Deck.Standard));

        // Verify
        Assert.AreEqual("ScrumTeamAlreadyExists", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void CreateScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        try
        {
            target.CreateScrumTeam("team", "master", Deck.Standard);
        }
        catch (PlanningPokerException)
        {
            // expected exception when adding same team
        }

        // Verify
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
    }

    [TestMethod]
    public void CreateScrumTeam_EmptyTeamInRepository_CreatesNewTeamAndDeletesOldOne()
    {
        // Arrange
        var team = new ScrumTeam("team");
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);

        // Verify
        Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void CreateScrumTeam_ExpiredTeamInRepository_CreatesNewTeamAndDeletesOldOne()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var availableEstimations = DeckProvider.Default.GetDefaultDeck();
        var team = new ScrumTeam("team", availableEstimations, timeProvider, null);
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);

        // Verify
        Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void CreateScrumTeam_TeamAlreadyLoaded_NotLoadingAgain()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);
        var target = CreatePlanningPokerController(repository: repository.Object);

        using (target.CreateScrumTeam("team", "master", Deck.Standard))
        {
            // Create team only
        }

        // Act
        try
        {
            target.CreateScrumTeam("team", "master", Deck.Standard);
        }
        catch (PlanningPokerException)
        {
            // expected exception when adding same team
        }

        // Verify
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void CreateScrumTeam_TeamCreatedWhileLoading_DoesNotCreateNewTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var firstLoad = true;
        var firstReturn = true;
        PlanningPokerController? target = null;

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team"))
            .Callback<string>(n =>
            {
                Assert.IsNotNull(target);
                if (firstLoad)
                {
                    firstLoad = false;
                    try
                    {
                        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);
                        Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
                    }
                    catch (ArgumentException)
                    {
                        // if ArgumentException is here, test should fail
                    }
                }
            }).Returns<string>(n =>
            {
                if (firstReturn)
                {
                    firstReturn = false;
                    return null;
                }
                else
                {
                    return team;
                }
            });

        target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.CreateScrumTeam("team", "master", Deck.Standard));

        // Verify
        Assert.AreEqual("ScrumTeamAlreadyExists", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void AttachScrumTeam_TeamNotInRepository_TriedToLoadFromRepository()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);

        var availableEstimations = DeckProvider.Default.GetDeck(Deck.Fibonacci);
        var team = new ScrumTeam("team", availableEstimations, null, null);
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.AttachScrumTeam(team);

        // Verify
        Assert.IsNotNull(teamLock);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void AttachScrumTeam_TeamInRepository_DoesNotCreateNewTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        var inputTeam = new ScrumTeam("team");

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.AttachScrumTeam(inputTeam));

        // Verify
        Assert.AreEqual("ScrumTeamAlreadyExists", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void AttachScrumTeam_TeamInRepository_DoesNotDeleteOldTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        var inputTeam = new ScrumTeam("team");

        // Act
        try
        {
            target.AttachScrumTeam(inputTeam);
        }
        catch (PlanningPokerException)
        {
            // expected exception when adding same team
        }

        // Verify
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
    }

    [TestMethod]
    public void GetScrumTeam_TeamInRepository_ReturnsTeamFromRepository()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        using var teamLock = target.GetScrumTeam("team");

        // Verify
        Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
    }

    [TestMethod]
    public void GetScrumTeam_TeamNotInRepository_ThrowsException()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);

        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.GetScrumTeam("team"));

        // Verify
        Assert.AreEqual("ScrumTeamNotExist", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void GetScrumTeam_EmptyTeamInRepository_ThrowsException()
    {
        // Arrange
        var team = new ScrumTeam("team");
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.GetScrumTeam("team"));

        // Verify
        Assert.AreEqual("ScrumTeamNotExist", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void GetScrumTeam_EmptyTeamInRepository_DeletesOldTeam()
    {
        // Arrange
        var team = new ScrumTeam("team");
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        try
        {
            target.GetScrumTeam("team");
        }
        catch (PlanningPokerException)
        {
            // expected exception
        }

        // Verify
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void GetScrumTeam_ExpiredTeamInRepository_ThrowsException()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var availableEstimations = DeckProvider.Default.GetDefaultDeck();
        var team = new ScrumTeam("team", availableEstimations, timeProvider, null);
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        var exception = Assert.ThrowsExactly<PlanningPokerException>(() => target.GetScrumTeam("team"));

        // Verify
        Assert.AreEqual("ScrumTeamNotExist", exception.Error);
        Assert.AreEqual("team", exception.Argument);
    }

    [TestMethod]
    public void GetScrumTeam_ExpiredTeamInRepository_DeletesOldTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var availableEstimations = DeckProvider.Default.GetDefaultDeck();
        var team = new ScrumTeam("team", availableEstimations, timeProvider, null);
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 16, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        try
        {
            target.GetScrumTeam("team");
        }
        catch (PlanningPokerException)
        {
            // expected exception
        }

        // Verify
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void GetScrumTeam_TeamAlreadyLoaded_NotLoadingAgain()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        using (target.GetScrumTeam("team"))
        {
            // Obtain team only
        }

        using var teamLock = target.GetScrumTeam("team");

        // Verify
        Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
    }

    [TestMethod]
    public void GetScrumTeam_TeamCreatedWhileLoading_DoesNotCreateNewTeam()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var firstLoad = true;
        var firstReturn = true;
        PlanningPokerController? target = null;
        ScrumTeam? createdTeam = null;

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team"))
            .Callback<string>(n =>
            {
                Assert.IsNotNull(target);
                if (firstLoad)
                {
                    firstLoad = false;

                    using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);
                    createdTeam = teamLock.Team;
                }
            }).Returns<string>(n =>
            {
                if (firstReturn)
                {
                    firstReturn = false;
                    return null;
                }
                else
                {
                    return team;
                }
            });

        target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        using (var teamLock = target.GetScrumTeam("team"))
        {
            // Verify
            Assert.AreNotEqual<ScrumTeam?>(team, createdTeam);
            Assert.AreNotEqual<ScrumTeam>(team, teamLock.Team);
            Assert.AreEqual<ScrumTeam?>(createdTeam, teamLock.Team);
            repository.Verify(r => r.LoadScrumTeam("team"), Times.Exactly(2));
            repository.Verify(r => r.DeleteScrumTeam("team"), Times.Never());
        }
    }

    [TestMethod]
    public void GetScrumTeam_DisconnectAfterwards_TeamIsRemovedFromRepository()
    {
        // Arrange
        var timeProvider = new DateTimeProviderMock();
        var configuration = new Mock<IPlanningPokerConfiguration>(MockBehavior.Strict);
        configuration.SetupGet(c => c.ClientInactivityTimeout).Returns(TimeSpan.FromMinutes(15));

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 0, 0, DateTimeKind.Utc));
        var team = new ScrumTeam("team");
        var master = team.SetScrumMaster("master");
        master.UpdateActivity();

        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns(team);
        repository.Setup(r => r.DeleteScrumTeam("team"));
        repository.SetupGet(r => r.ScrumTeamNames).Returns([]);

        timeProvider.SetUtcNow(new DateTime(2015, 1, 1, 10, 14, 0, DateTimeKind.Utc));
        var target = CreatePlanningPokerController(timeProvider, configuration.Object, repository.Object);

        // Act
        using var teamLock = target.GetScrumTeam("team");
        teamLock.Team.Disconnect(master.Name);
        var result = target.ScrumTeamNames;

        // Verify
        Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
        Assert.IsFalse(result.Any());
        repository.Verify(r => r.LoadScrumTeam("team"), Times.Once());
        repository.Verify(r => r.DeleteScrumTeam("team"), Times.Once());
    }

    [TestMethod]
    public void PlanningPokerController_ObserverUpdateActivity_ScrumTeamSavedToRepository()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);
        repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);
        teamLock.Team.ScrumMaster!.UpdateActivity();

        // Verify
        repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
    }

    [TestMethod]
    public void PlanningPokerController_JoinMember_ScrumTeamSavedToRepository()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);
        repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);
        teamLock.Team.Join("member", false);

        // Verify
        repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
    }

    [TestMethod]
    public void PlanningPokerController_StartEstimation_ScrumTeamSavedToRepository()
    {
        // Arrange
        var repository = new Mock<IScrumTeamRepository>(MockBehavior.Strict);
        repository.Setup(r => r.LoadScrumTeam("team")).Returns((ScrumTeam?)null);
        repository.Setup(r => r.SaveScrumTeam(It.IsAny<ScrumTeam>()));
        var target = CreatePlanningPokerController(repository: repository.Object);

        // Act
        using var teamLock = target.CreateScrumTeam("team", "master", Deck.Standard);
        teamLock.Team.ScrumMaster!.StartEstimation();

        // Verify
        repository.Verify(r => r.SaveScrumTeam(teamLock.Team), Times.Once());
    }

    private static PlanningPokerController CreatePlanningPokerController(
        DateTimeProvider? dateTimeProvider = null,
        IPlanningPokerConfiguration? configuration = null,
        IScrumTeamRepository? repository = null,
        GuidProvider? guidProvider = null,
        DeckProvider? deckProvider = null,
        TaskProvider? taskProvider = null,
        ILogger<PlanningPokerController>? logger = null)
    {
        logger ??= Mock.Of<ILogger<PlanningPokerController>>();
        return new PlanningPokerController(dateTimeProvider, guidProvider, deckProvider, configuration, repository, taskProvider, logger);
    }
}
