using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    public static class ScrumTeamAsserts
    {
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Asserted using MS Test.")]
        public static void AssertScrumTeamsAreEqual(ScrumTeam expected, ScrumTeam actual)
        {
            if (expected == null)
            {
                Assert.IsNotNull(actual);
                return;
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.State, actual.State);
            AssertAvailableEstimations(expected.AvailableEstimations, actual.AvailableEstimations);
            AssertObserversAreEqual(expected.ScrumMaster, actual.ScrumMaster);

            Assert.AreEqual(expected.Members.Count(), actual.Members.Count());
            foreach (var expectedMember in expected.Members)
            {
                var actualMember = actual.Members.Single(m => m.Name == expectedMember.Name);
                AssertObserversAreEqual(expectedMember, actualMember);
            }

            Assert.AreEqual(expected.Observers.Count(), actual.Observers.Count());
            foreach (var expectedObserver in expected.Observers)
            {
                var actualObserver = actual.Observers.Single(m => m.Name == expectedObserver.Name);
                AssertObserversAreEqual(expectedObserver, actualObserver);
            }

            AssertEsimationParticipantsAreEqual(expected.EstimationParticipants, actual.EstimationParticipants);
            AssertEstimationResultsAreEqual(expected.EstimationResult, actual.EstimationResult);
        }

        private static void AssertAvailableEstimations(IEnumerable<Estimation> expected, IEnumerable<Estimation> actual)
        {
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        private static void AssertObserversAreEqual(Observer expected, Observer actual, bool basicPropertiesOnly = false)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Name, actual.Name);

                var team = expected.Team;
                var isConnected = team.Members.Concat(team.Observers).Contains(expected);

                // Verify extended properties only, when member is connected. Otherwise only member's name is recovered.
                if (isConnected)
                {
                    Assert.AreEqual(expected.GetType(), actual.GetType());
                    Assert.AreEqual(expected.LastActivity, actual.LastActivity);
                    Assert.AreEqual(expected.IsDormant, actual.IsDormant);

                    if (!basicPropertiesOnly)
                    {
                        Assert.AreEqual(expected.HasMessage, actual.HasMessage);
                        var expectedMessages = expected.Messages.ToList();
                        var actualMessages = actual.Messages.ToList();
                        Assert.AreEqual(expectedMessages.Count, actualMessages.Count);

                        for (int i = 0; i < expectedMessages.Count; i++)
                        {
                            AssertMessagesAreEqual(expectedMessages[i], actualMessages[i]);
                        }

                        if (expected is Member expectedMember)
                        {
                            AssertMembersAreEqual(expectedMember, (Member)actual);
                        }
                    }
                }
            }
        }

        private static void AssertMembersAreEqual(Member expected, Member actual)
        {
            Assert.AreEqual(expected.Estimation, actual.Estimation);
        }

        private static void AssertMessagesAreEqual(Message expected, Message actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.MessageType, actual.MessageType);
            Assert.AreEqual(expected.GetType(), actual.GetType());

            if (expected is MemberMessage expectedMemberMessage)
            {
                AssertMemberMessagesAreEqual(expectedMemberMessage, (MemberMessage)actual);
            }

            if (expected is EstimationResultMessage expectedEstimationResultMessage)
            {
                AssertEstimationResultMessagesAreEqual(expectedEstimationResultMessage, (EstimationResultMessage)actual);
            }
        }

        private static void AssertMemberMessagesAreEqual(MemberMessage expected, MemberMessage actual)
        {
            AssertObserversAreEqual(expected.Member, actual.Member, true);
        }

        private static void AssertEstimationResultMessagesAreEqual(EstimationResultMessage expected, EstimationResultMessage actual)
        {
            AssertEstimationResultsAreEqual(expected.EstimationResult, actual.EstimationResult);
        }

        private static void AssertEsimationParticipantsAreEqual(IEnumerable<EstimationParticipantStatus> expected, IEnumerable<EstimationParticipantStatus> actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedItem in expected)
                {
                    var actualItem = actual.Single(i => i.MemberName == expectedItem.MemberName);
                    Assert.AreEqual(expectedItem.MemberName, actualItem.MemberName);
                    Assert.AreEqual(expectedItem.Estimated, actualItem.Estimated);
                }
            }
        }

        private static void AssertEstimationResultsAreEqual(EstimationResult expected, EstimationResult actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.AreEqual(expected.Count, actual.Count);
                foreach (var expectedItem in expected)
                {
                    var actualItem = actual.Single(i => i.Key.Name == expectedItem.Key.Name);
                    AssertObserversAreEqual(expectedItem.Key, actualItem.Key, true);
                    Assert.AreEqual(expectedItem.Value, actualItem.Value);
                }
            }
        }
    }
}
