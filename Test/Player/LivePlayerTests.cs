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
        public async Task GeneratePath_Should_Work()
        {
            var proc = new Mock<IProcessHandler>().Object;
            var character = new Mock<ICharacterStateProvider>().Object;
            var planProvider = new TestPlanProvider();
            var livePlayer = new LivePlayer(proc, character, planProvider);
            var nodes = new List<Node>();
            var validators = new List<Action<Node>>()
            {
                n => { n.X.Should().Be(0.5f); n.Y.Should().Be(0.5f); },
                n => { n.X.Should().Be(2 - n.Y); n.Y.Should().BeInRange(0, 2); },
                n => { n.X.Should().Be(1.5f); n.Y.Should().Be(1.5f); },
                n => { n.X.Should().Be(2); n.Y.Should().BeInRange(0, 2); },
                n => { n.X.Should().Be(2.5f); n.Y.Should().Be(0.5f); },
                n => { n.X.Should().Be(4 - n.Y); n.Y.Should().BeInRange(0, 2); },
                n => { n.X.Should().Be(3.5f); n.Y.Should().Be(1.5f); },
                n => { n.X.Should().Be(4); n.Y.Should().BeInRange(0, 2); },
                n => { n.X.Should().Be(6 - n.Y); n.Y.Should().BeInRange(0, 2); },
                n => { n.X.Should().BeInRange(4, 6); n.Y.Should().Be(2); },
                n => { n.X.Should().Be(4.5f); n.Y.Should().Be(2.5f); },
                n => { n.X.Should().Be(4); n.Y.Should().BeInRange(2, 4); },
                n => { n.X.Should().Be(6 - n.Y); n.Y.Should().BeInRange(2, 4); },
                n => { n.X.Should().Be(2); n.Y.Should().BeInRange(2, 4); },
                n => { n.X.Should().Be(4 - n.Y); n.Y.Should().BeInRange(2, 4); },
                n => { n.X.Should().Be(0.5f); n.Y.Should().Be(2.5f); },
                n => { n.X.Should().BeInRange(0, 2); n.Y.Should().Be(2); },
                n => { n.X.Should().Be(2 - n.Y); n.Y.Should().BeInRange(0, 2); }
            };

            await livePlayer.LoadAndGenerateRoute();
            var node = livePlayer.CurrentRouteNode;
            nodes.Add(node);
            for (int i = 0; i < 17; ++i)
            {
                node = node.Next;
                nodes.Add(node);
            }

            nodes[nodes.Count - 1].Next.Should().BeSameAs(nodes[0]);
            for (int i = 0; i < validators.Count; ++i)
            {
                validators[i](nodes[i]);
            }
        }
    }
}
