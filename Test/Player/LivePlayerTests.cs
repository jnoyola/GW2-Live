using FluentAssertions;
using GW2_Live.GameInterface;
using GW2_Live.Player;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.Player
{
    [TestClass]
    public class LivePlayerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public async Task LoadAndGenerateRoute_Should_Work()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
            var nodes = new List<Node>();
            var validators = new List<Action<Node>>()
            {
                n => { n.X.Should().Be(0.5f); n.Y.Should().Be(0.5f); },
                n => { n.X.Should().Be(1.5f); n.Y.Should().Be(1.5f); },
                n => { n.X.Should().Be(2.5f); n.Y.Should().Be(0.5f); },
                n => { n.X.Should().Be(3.5f); n.Y.Should().Be(1.5f); },
                n => { n.X.Should().Be(4.5f); n.Y.Should().Be(2.5f); },
                n => { n.X.Should().Be(0.5f); n.Y.Should().Be(2.5f); },
            };

            await livePlayer.LoadAndGenerateRoute();

            livePlayer.Route.Should().HaveSameCount(validators);
            for (int i = 0; i < livePlayer.Route.Count; ++i)
            {
                validators[i](livePlayer.Route[i]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GenerateViaAndTarget_Should_NotCreateViaForTargetInSameTri()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
        }
    }
}
