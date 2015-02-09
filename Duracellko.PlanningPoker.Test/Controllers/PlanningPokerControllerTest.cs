// <copyright>
// Copyright (c) 2012 Rasto Novotny
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Duracellko.PlanningPoker.Controllers;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Controllers.Test
{
    [TestClass]
    public class PlanningPokerControllerTest
    {
        #region Constructor

        [TestMethod]
        public void PlanningPokerController_Create_DefaultDateTimeProvider()
        {
            // Act
            var result = new PlanningPokerController();

            // Verify
            Assert.AreEqual<DateTimeProvider>(DateTimeProvider.Default, result.DateTimeProvider);
        }

        [TestMethod]
        public void PlanningPokerController_SpecificDateTimeProvider_DateTimeProviderIsSet()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();

            // Act
            var result = new PlanningPokerController(dateTimeProvider, null);

            // Verify
            Assert.AreEqual<DateTimeProvider>(dateTimeProvider, result.DateTimeProvider);
        }

        [TestMethod]
        public void PlanningPokerController_Configuration_ConfigurationIsSet()
        {
            // Arrange
            var configuration = new Duracellko.PlanningPoker.Configuration.PlanningPokerConfigurationElement();

            // Act
            var result = new PlanningPokerController(null, configuration);

            // Verify
            Assert.AreEqual(configuration, result.Configuration);
        }

        #endregion

        #region ScrumTeamNames

        [TestMethod]
        public void ScrumTeamNames_Get_ReturnsListOfTeamNames()
        {
            // Arrange
            var target = new PlanningPokerController();
            using (var teamLock1 = target.CreateScrumTeam("team1", "master1"))
            {
            }

            using (var teamLock2 = target.CreateScrumTeam("team2", "master1"))
            {
            }

            // Act
            var result = target.ScrumTeamNames;

            // Verify
            var expectedCollection = new string[] { "team1", "team2" };
            CollectionAssert.AreEquivalent(expectedCollection, result.ToList());
        }

        #endregion

        #region CreateScrumTeam

        [TestMethod]
        public void CreateScrumTeam_TeamName_CreatedTeamWithSpecifiedName()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.IsNotNull(teamLock);
                Assert.IsNotNull(teamLock.Team);
                Assert.AreEqual<string>("team", teamLock.Team.Name);
            }
        }

        [TestMethod]
        public void CreateScrumTeam_ScrumMasterName_CreatedTeamWithSpecifiedScrumMaster()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.IsNotNull(teamLock.Team.ScrumMaster);
                Assert.AreEqual<string>("master", teamLock.Team.ScrumMaster.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateScrumTeam_TeamNameAlreadyExists_ArgumentException()
        {
            // Arrange
            var target = new PlanningPokerController();
            var team = target.CreateScrumTeam("team", "master");
            team.Dispose();

            // Act
            target.CreateScrumTeam("team", "master2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScrumTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.CreateScrumTeam(string.Empty, "master");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateScrumTeam_ScrumMasterNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.CreateScrumTeam("test team", string.Empty);
        }

        [TestMethod]
        public void CreateScrumTeam_SpecificDateTimeProvider_CreatedTeamWithDateTimeProvider()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            var target = new PlanningPokerController(dateTimeProvider, null);

            // Act
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Verify
                Assert.AreEqual<DateTimeProvider>(dateTimeProvider, teamLock.Team.DateTimeProvider);
            }
        }

        #endregion

        #region AttachScrumTeam

        [TestMethod]
        public void AttachScrumTeam_ScrumTeam_TeamIsInCollection()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new PlanningPokerController();

            // Act
            target.AttachScrumTeam(team);

            // Verify
            Assert.IsNotNull(target.GetScrumTeam(team.Name));
        }

        [TestMethod]
        public void AttachScrumTeam_ScrumTeam_ReturnsSameTeam()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var target = new PlanningPokerController();

            // Act
            var result = target.AttachScrumTeam(team);

            // Verify
            Assert.AreEqual<ScrumTeam>(team, result.Team);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AttachScrumTeam_TeamNameAlreadyExists_ArgumentException()
        {
            // Arrange
            var target = new PlanningPokerController();
            var existingTeam = target.CreateScrumTeam("team", "master");
            existingTeam.Dispose();
            var team = new ScrumTeam("team");

            // Act
            target.AttachScrumTeam(team);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AttachScrumTeam_Null_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.AttachScrumTeam(null);
        }

        #endregion

        #region GetScrumTeam

        [TestMethod]
        public void GetScrumTeam_TeamNameExists_ReturnsExistingTeam()
        {
            // Arrange
            var target = new PlanningPokerController();
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            // Act
            using (var teamLock = target.GetScrumTeam("team"))
            {
                // Verify
                Assert.AreEqual<ScrumTeam>(team, teamLock.Team);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_TeamNameNotExists_ArgumentException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.GetScrumTeam("team");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetScrumTeam_TeamNameIsEmpty_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.GetScrumTeam(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetScrumTeam_AfterDisconnectingAllMembers_ArgumentException()
        {
            // Arrange
            var target = new PlanningPokerController();
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            team.Disconnect("master");

            // Act
            target.GetScrumTeam("team");
        }

        #endregion

        #region GetMessagesAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMessagesAsync_ObserverIsNull_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();

            // Act
            target.GetMessagesAsync(null, (f, o) => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetMessagesAsync_CallbackIsNull_ArgumentNullException()
        {
            // Arrange
            var target = new PlanningPokerController();
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                // Act
                target.GetMessagesAsync(teamLock.Team.ScrumMaster, null);
            }
        }

        #endregion

        #region DisconnectInactiveObservers

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveMembers_TeamIsUnchanged()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new PlanningPokerController(dateTimeProvider, null);
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("member", false);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (var teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(2, teamLock.Team.Members.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveMember_MemberIsDisconnected()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new PlanningPokerController(dateTimeProvider, null);
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("member", false);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (var teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(1, teamLock.Team.Members.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_NoInactiveObservers_TeamIsUnchanged()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new PlanningPokerController(dateTimeProvider, null);
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("observer", true);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 40));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (var teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(1, teamLock.Team.Observers.Count());
            }
        }

        [TestMethod]
        public void DisconnectInactiveObservers_InactiveObserver_ObserverIsDisconnected()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new PlanningPokerController(dateTimeProvider, null);
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
                team.Join("observer", true);
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));
            team.ScrumMaster.UpdateActivity();

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            using (var teamLock = target.GetScrumTeam("team"))
            {
                Assert.AreEqual<int>(0, teamLock.Team.Observers.Count());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DisconnectInactiveObservers_InactiveScrumMaster_TeamIsClosed()
        {
            // Arrange
            var dateTimeProvider = new DateTimeProviderMock();
            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 20));

            var target = new PlanningPokerController(dateTimeProvider, null);
            ScrumTeam team;
            using (var teamLock = target.CreateScrumTeam("team", "master"))
            {
                team = teamLock.Team;
            }

            dateTimeProvider.SetUtcNow(new DateTime(2012, 1, 1, 3, 2, 55));

            // Act
            target.DisconnectInactiveObservers(TimeSpan.FromSeconds(30.0));

            // Verify
            target.GetScrumTeam("team");
        }

        #endregion
    }
}
