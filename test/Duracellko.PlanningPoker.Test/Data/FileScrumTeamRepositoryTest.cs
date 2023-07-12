using System;
using System.IO;
using System.Linq;
using Duracellko.PlanningPoker.Configuration;
using Duracellko.PlanningPoker.Data;
using Duracellko.PlanningPoker.Domain;
using Duracellko.PlanningPoker.Domain.Serialization;
using Duracellko.PlanningPoker.Domain.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Duracellko.PlanningPoker.Test.Data
{
    [TestClass]
    public class FileScrumTeamRepositoryTest
    {
        private DirectoryInfo? _rootFolder;

        // This property checks if '\' is invalid character and that is more important.
        // '\' is invalid on Windows and IsWindows seems more readable than IsBackslashInvalid.
        private static bool IsWindows => Path.GetInvalidFileNameChars().Contains('\\');

        [TestInitialize]
        public void Initialize()
        {
            var rootFolder = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            _rootFolder = Directory.CreateDirectory(rootFolder);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_rootFolder != null)
            {
                _rootFolder.Delete(true);
                _rootFolder = null;
            }
        }

        [TestMethod]
        public void Folder_Get_IsRootFolder()
        {
            var target = CreateFileScrumTeamRepository();

            var result = target.Folder;

            Assert.AreEqual(_rootFolder!.FullName, result);
        }

        [TestMethod]
        public void ScrumTeamNames_2Files_2ScrumTeams()
        {
            CreateTextFile("The Team.json");
            CreateTextFile("Team 2.json");
            var target = CreateFileScrumTeamRepository();

            var result = target.ScrumTeamNames.ToList();

            var expectedScrumTeamNames = new[] { "The Team", "Team 2" };
            CollectionAssert.AreEquivalent(expectedScrumTeamNames, result);
        }

        [TestMethod]
        public void ScrumTeamNames_FileWithSpecialCharacters_ReturnsScrumTeam()
        {
            CreateTextFile("My %005C%003F.%002F Team%0025 😎 %002A.json");
            var target = CreateFileScrumTeamRepository();

            var result = target.ScrumTeamNames.ToList();

            var expectedScrumTeamNames = new[] { "My \\?./ Team% \ud83d\ude0e *" };
            CollectionAssert.AreEquivalent(expectedScrumTeamNames, result);
        }

        [TestMethod]
        public void ScrumTeamNames_FileWithInvalidEscape_EmptyCollection()
        {
            CreateTextFile("My Team%0025 %002.json");
            var target = CreateFileScrumTeamRepository();

            var result = target.ScrumTeamNames.ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ScrumTeamNames_TxtFile_ReturnsScrumTeam()
        {
            CreateTextFile("The Team.txt");
            CreateTextFile("Team 2.json");
            var target = CreateFileScrumTeamRepository();

            var result = target.ScrumTeamNames.ToList();

            var expectedScrumTeamNames = new[] { "Team 2" };
            CollectionAssert.AreEquivalent(expectedScrumTeamNames, result);
        }

        [TestMethod]
        public void ScrumTeamNames_NoFiles_EmptyCollection()
        {
            var target = CreateFileScrumTeamRepository();

            var result = target.ScrumTeamNames.ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void SaveScrumTeam_ScrumTeam_FileIsCreated()
        {
            var team = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("The team.json", files[0].Name);
        }

        [TestMethod]
        public void SaveScrumTeam_SavedTwice_FileIsOverwritten()
        {
            var team = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);

            var member = team.Members.First(m => m.GetType() == typeof(Member));
            member.Estimation = team.AvailableEstimations.First(e => e.Value == 2);

            target.SaveScrumTeam(team);

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("The team.json", files[0].Name);
        }

        [TestMethod]
        public void SaveScrumTeam_2ScrumTeams_2FilesAreCreated()
        {
            var team1 = CreateScrumTeam();
            var team2 = CreateScrumTeam("Team 2");
            team2.Join("PO", true);
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team1);
            target.SaveScrumTeam(team2);

            var fileNames = _rootFolder!.GetFiles().Select(f => f.Name).ToList();
            var expectedFileNames = new[] { "The team.json", "Team 2.json" };
            CollectionAssert.AreEquivalent(expectedFileNames, fileNames);
        }

        [TestMethod]
        public void SaveScrumTeam_SpecialCharactersInName_FileIsCreated()
        {
            var team = CreateScrumTeam("My \\?./ Team% 😎 *");
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);

            var member = team.Members.First(m => m.GetType() == typeof(Member));
            member.Estimation = team.AvailableEstimations.First(e => e.Value == 2);

            target.SaveScrumTeam(team);

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            var expectedFileName = IsWindows ?
                "My %005C%003F.%002F Team%0025 \ud83d\ude0e %002A.json" :
                "My \\?.%002F Team%0025 \ud83d\ude0e *.json";
            Assert.AreEqual(expectedFileName, files[0].Name);
        }

        [TestMethod]
        public void LoadScrumTeam_SavedScrumTeam_ScrumTeamsAreSame()
        {
            var team = CreateScrumTeam();
            var team2 = CreateScrumTeam("Team 2");
            team2.Join("PO", true);
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team2);
            target.SaveScrumTeam(team);
            var result = target.LoadScrumTeam(team.Name);

            ScrumTeamAsserts.AssertScrumTeamsAreEqual(team, result);
        }

        [TestMethod]
        public void LoadScrumTeam_NotExistingScrumTeam_ReturnNull()
        {
            var team = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);
            var result = target.LoadScrumTeam("Team 2");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadScrumTeam_NoScrumTeams_ReturnNull()
        {
            var target = CreateFileScrumTeamRepository();

            var result = target.LoadScrumTeam("Team 2");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadScrumTeam_InvalidFile_ReturnNull()
        {
            CreateTextFile("Team 2.json");
            var target = CreateFileScrumTeamRepository();

            var result = target.LoadScrumTeam("Team 2");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void LoadScrumTeam_SpecialCharactersInName_ScrumTeamsAreSame()
        {
            var team = CreateScrumTeam("My \\?./ Team% 😎 *");
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);
            var result = target.LoadScrumTeam("My \\?./ Team% \ud83d\ude0e *");

            ScrumTeamAsserts.AssertScrumTeamsAreEqual(team, result);
        }

        [TestMethod]
        public void DeleteScrumTeam_ExistingScrumTeam_FileIsDeleted()
        {
            var team = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);

            target.DeleteScrumTeam(team.Name);

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(0, files.Length);
        }

        [TestMethod]
        public void DeleteScrumTeam_SpecialCharactersInName_FileIsDeleted()
        {
            var team1 = CreateScrumTeam("My \\?./ Team% 😎 *");
            var team2 = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team1);
            target.SaveScrumTeam(team2);

            target.DeleteScrumTeam("My \\?./ Team% \ud83d\ude0e *");

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("The team.json", files[0].Name);
        }

        [TestMethod]
        public void DeleteScrumTeam_NotExistingScrumTeam_NoChanges()
        {
            var team = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team);

            target.DeleteScrumTeam("Team 2");

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("The team.json", files[0].Name);
        }

        [TestMethod]
        public void DeleteScrumTeam_EmptyFolder_NoChanges()
        {
            var target = CreateFileScrumTeamRepository();

            target.DeleteScrumTeam("Team 2");

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(0, files.Length);
        }

        [TestMethod]
        public void DeleteAll_2Files_EmptyFolder()
        {
            var team1 = CreateScrumTeam("My \\?./ Team% 😎 *");
            var team2 = CreateScrumTeam();
            var target = CreateFileScrumTeamRepository();

            target.SaveScrumTeam(team1);
            target.SaveScrumTeam(team2);

            target.DeleteAll();

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(0, files.Length);
        }

        [TestMethod]
        public void DeleteAll_TxtFile_TxtFileIsNotDeleted()
        {
            CreateTextFile("Team 1.txt");
            CreateTextFile("Team 2.json");

            var target = CreateFileScrumTeamRepository();
            target.DeleteAll();

            var files = _rootFolder!.GetFiles();
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("Team 1.txt", files[0].Name);
        }

        private static ScrumTeam CreateScrumTeam(string name = "The team")
        {
            var team = new ScrumTeam(name);
            team.SetScrumMaster("master");
            team.Join("member", false);
            team.ScrumMaster!.StartEstimation();
            team.ScrumMaster.Estimation = team.AvailableEstimations.First(e => e.Value == 5);
            return team;
        }

        private FileScrumTeamRepository CreateFileScrumTeamRepository()
        {
            var settings = new Mock<IFileScrumTeamRepositorySettings>();
            settings.SetupGet(o => o.Folder).Returns(_rootFolder!.FullName);

            var configuration = new Mock<IPlanningPokerConfiguration>();
            configuration.SetupGet(o => o.RepositoryTeamExpiration).Returns(TimeSpan.FromMinutes(1));

            var serializer = new ScrumTeamSerializer(DateTimeProvider.Default, GuidProvider.Default);

            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<FileScrumTeamRepository>>();

            return new FileScrumTeamRepository(settings.Object, configuration.Object, serializer, DateTimeProvider.Default, GuidProvider.Default, logger.Object);
        }

        private FileInfo CreateTextFile(string name)
        {
            var path = Path.Join(_rootFolder!.FullName, name);
            var result = new FileInfo(path);
            using (var writer = result.CreateText())
            {
                writer.Write(Guid.NewGuid().ToString());
            }

            return result;
        }
    }
}
