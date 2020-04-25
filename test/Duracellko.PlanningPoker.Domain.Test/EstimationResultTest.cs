using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimationResultTest
    {
        [TestMethod]
        public void Constructor_ScrumMasterAndMember_MembersHasNoEstimation()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);

            // Act
            var result = new EstimationResult(new Member[] { master, member });

            // Verify
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(member, null),
                new KeyValuePair<Member, Estimation>(master, null),
            };
            CollectionAssert.AreEquivalent(expectedResult, result.ToList());
        }

        [TestMethod]
        public void Constructor_EmptyCollection_EmptyCollection()
        {
            // Arrange
            var members = Enumerable.Empty<Member>();

            // Act
            var result = new EstimationResult(members);

            // Verify
            var expectedResult = Array.Empty<KeyValuePair<Member, Estimation>>();
            CollectionAssert.AreEquivalent(expectedResult, result.ToList());
        }

        [TestMethod]
        public void Constructor_Null_ArgumentNullException()
        {
            // Act
            Assert.ThrowsException<ArgumentNullException>(() => new EstimationResult(null));
        }

        [TestMethod]
        public void Constructor_DuplicateMember_InvalidOperationException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);

            // Act
            Assert.ThrowsException<ArgumentException>(() => new EstimationResult(new Member[] { master, member, master }));
        }

        [TestMethod]
        public void IndexerSet_SetScrumMasterEstimation_EstimationOfScrumMasterIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master, member });
            var estimation = new Estimation();

            // Act
            target[master] = estimation;

            // Verify
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(member, null),
                new KeyValuePair<Member, Estimation>(master, estimation),
            };
            CollectionAssert.AreEquivalent(expectedResult, target.ToList());
        }

        [TestMethod]
        public void IndexerSet_SetMemberEstimation_EstimationOfMemberIsSet()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master, member });
            var estimation = new Estimation();

            // Act
            target[member] = estimation;

            // Verify
            var expectedResult = new KeyValuePair<Member, Estimation>[]
            {
                new KeyValuePair<Member, Estimation>(master, null),
                new KeyValuePair<Member, Estimation>(member, estimation),
            };
            CollectionAssert.AreEquivalent(expectedResult, target.ToList());
        }

        [TestMethod]
        public void IndexerSet_MemberNotInResult_KeyNotFoundException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master });
            var estimation = new Estimation();

            // Act
            Assert.ThrowsException<KeyNotFoundException>(() => target[member] = estimation);
        }

        [TestMethod]
        public void IndexerSet_IsReadOnly_InvalidOperationException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master, member });
            var estimation = new Estimation();

            // Act
            target.SetReadOnly();
            Assert.ThrowsException<InvalidOperationException>(() => target[member] = estimation);
        }

        [TestMethod]
        public void IndexerGet_MemberNotInResult_KeyNotFoundException()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master });

            // Act
            Assert.ThrowsException<KeyNotFoundException>(() => target[member]);
        }

        [TestMethod]
        public void ContainsMember_MemberIsInResult_ReturnsTrue()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master, member });

            // Act
            var result = target.ContainsMember(member);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsMember_MemberIsNotInResult_ReturnsFalse()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master });

            // Act
            var result = target.ContainsMember(member);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Count_InitializesBy2Members_Returns2()
        {
            // Arrange
            var team = new ScrumTeam("test team");
            var master = team.SetScrumMaster("master");
            var member = (Member)team.Join("member", false);
            var target = new EstimationResult(new Member[] { master, member });

            // Act
            var result = target.Count;

            // Verify
            Assert.AreEqual<int>(2, result);
        }

        [TestMethod]
        public void SetReadOnly_Execute_SetsIsReadOnly()
        {
            // Arrange
            var target = new EstimationResult(Enumerable.Empty<Member>());

            // Act
            target.SetReadOnly();

            // Verify
            Assert.IsTrue(target.IsReadOnly);
        }

        [TestMethod]
        public void SetReadOnly_GetAfterConstruction_ReturnsFalse()
        {
            // Arrange
            var target = new EstimationResult(Enumerable.Empty<Member>());

            // Act
            var result = target.IsReadOnly;

            // Verify
            Assert.IsFalse(result);
        }
    }
}
