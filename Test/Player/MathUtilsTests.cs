using FluentAssertions;
using GW2_Live.Player;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test.Player
{
    [TestClass]
    public class MathUtilsTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void GetAngleDiff_Should_Work()
        {
            var a0 = MathUtils.GetAngleDiff(1, 0, 5, 5);
            var a1 = MathUtils.GetAngleDiff(5, 5, 1, 0);
            var a2 = MathUtils.GetAngleDiff(4, -2, 2, 4);
            var a3 = MathUtils.GetAngleDiff(2, 4, 4, -2);
            var a4 = MathUtils.GetAngleDiff(-3, -3, 3, 3);
            var a5 = MathUtils.GetAngleDiff(3, 3, -3, -3);

            a0.Should().Be(Math.PI / 4);
            a1.Should().Be(-Math.PI / 4);
            a2.Should().Be(Math.PI / 2);
            a3.Should().Be(-Math.PI / 2);
            a4.Should().Be(Math.PI);
            a5.Should().Be(-Math.PI);
        }
    }
}
