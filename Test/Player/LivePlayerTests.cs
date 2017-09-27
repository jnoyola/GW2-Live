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
            Node target;

            await livePlayer.LoadAndGenerateRoute();
            target = livePlayer.GenerateViaAndTarget(0.1f, 0.1f);

            livePlayer.ViaRoute.Should().HaveCount(0);
            target.Should().Be(livePlayer.Route[0]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GenerateViaAndTarget_Should_CreateViaForTargetInDifferentTri()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
            Node target;
            Node via0;
            Node via1;

            await livePlayer.LoadAndGenerateRoute();
            target = livePlayer.GenerateViaAndTarget(2.1f, 0.1f);
            via0 = livePlayer.ViaRoute.Dequeue();
            via1 = livePlayer.ViaRoute.Dequeue();

            livePlayer.ViaRoute.Should().HaveCount(0);
            via0.X.Should().Be(2);
            via0.Y.Should().BeInRange(0, 2);
            via1.X.Should().Be(2 - via1.Y);
            via1.Y.Should().BeInRange(0, 2);
            target.Should().Be(via0);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GenerateViaAndTarget_ShouldNot_RecreateViaWhenAccurate()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
            Node firstTarget;
            Node target;
            Node via0;
            Node via1;

            await livePlayer.LoadAndGenerateRoute();
            firstTarget = livePlayer.GenerateViaAndTarget(2.2f, 0.1f);
            target = livePlayer.GenerateViaAndTarget(2.1f, 0.1f);
            via0 = livePlayer.ViaRoute.Dequeue();
            via1 = livePlayer.ViaRoute.Dequeue();

            firstTarget.Should().Be(target);
            livePlayer.ViaRoute.Should().HaveCount(0);
            via0.X.Should().Be(2);
            via0.Y.Should().BeInRange(0, 2);
            via1.X.Should().Be(2 - via1.Y);
            via1.Y.Should().BeInRange(0, 2);
            target.Should().Be(via0);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GenerateViaAndTarget_Should_RecreateViaWhenInaccurate()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
            Node badTarget;
            Node target;
            Node via0;
            Node via1;

            await livePlayer.LoadAndGenerateRoute();
            badTarget = livePlayer.GenerateViaAndTarget(0.1f, 2.1f);
            target = livePlayer.GenerateViaAndTarget(2.1f, 0.1f);
            via0 = livePlayer.ViaRoute.Dequeue();
            via1 = livePlayer.ViaRoute.Dequeue();

            badTarget.X.Should().BeInRange(0, 2);
            badTarget.Y.Should().Be(2);
            livePlayer.ViaRoute.Should().HaveCount(0);
            via0.X.Should().Be(2);
            via0.Y.Should().BeInRange(0, 2);
            via1.X.Should().Be(2 - via1.Y);
            via1.Y.Should().BeInRange(0, 2);
            target.Should().Be(via0);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GenerateViaAndTarget_Should_DeleteViaWhenInaccurate()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var input = new Mock<IInputHandler>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, input, planProvider);
            Node badTarget;
            Node target;

            await livePlayer.LoadAndGenerateRoute();
            badTarget = livePlayer.GenerateViaAndTarget(0.1f, 2.1f);
            target = livePlayer.GenerateViaAndTarget(0.1f, 0.1f);

            badTarget.X.Should().BeInRange(0, 2);
            badTarget.Y.Should().Be(2);
            livePlayer.ViaRoute.Should().HaveCount(0);
            target.Should().Be(livePlayer.Route[0]);
        }
    }
}
