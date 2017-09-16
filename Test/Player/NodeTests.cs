using FluentAssertions;
using GW2_Live.Player;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Player
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Insert_Should_RespectPriority()
        {
            PathNode pathNode1 = new PathNode(1, 11);
            PathNode pathNode2 = new PathNode(2, 22);
            GatherNode gatherNode1 = new GatherNode(11, 111);
            GatherNode gatherNode2 = new GatherNode(22, 222);
            ViaNode viaNode1 = new ViaNode(111, 1111);
            ViaNode viaNode2 = new ViaNode(222, 2222);

            Node n1 = viaNode1.InsertAndGetNext(gatherNode1);
            Node n2 = viaNode1.InsertAndGetNext(pathNode1);
            Node n3 = pathNode1.InsertAndGetNext(gatherNode2);
            Node n4 = gatherNode2.InsertAndGetNext(viaNode2);
            Node n5 = pathNode1.InsertAndGetNext(pathNode2);

            n1.Should().Be(viaNode1);
            n2.Should().Be(viaNode1);
            n3.Should().Be(gatherNode2);
            n4.Should().Be(viaNode2);
            n5.Should().Be(pathNode1);

            viaNode1.Previous.Should().BeNull();
            viaNode1.Next.Should().Be(gatherNode1);
            viaNode1.Next.Next.Should().Be(viaNode2);
            viaNode1.Next.Next.Next.Should().Be(gatherNode2);
            viaNode1.Next.Next.Next.Next.Should().Be(pathNode1);
            viaNode1.Next.Next.Next.Next.Next.Should().Be(pathNode2);
        }
    }
}

