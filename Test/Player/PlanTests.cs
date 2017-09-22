using FluentAssertions;
using GW2_Live.Player;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test.Player
{
    [TestClass]
    public class PlanTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void Tri_ContainsPoint_Should_Work()
        {
            var tri = new Plan.Tri(new HashSet<Plan.Point>()
            {
                new Plan.Point(0, 0),
                new Plan.Point(4, 0),
                new Plan.Point(4, 2),
            });
            bool b0, b1, b2, b3, b4;

            b0 = tri.ContainsPoint(1, 0.1f);
            b1 = tri.ContainsPoint(3.5f, 1.5f);
            b2 = tri.ContainsPoint(1, 1.5f);
            b3 = tri.ContainsPoint(5, 1);
            b4 = tri.ContainsPoint(2, -0.1f);

            b0.Should().BeTrue();
            b1.Should().BeTrue();
            b2.Should().BeFalse();
            b3.Should().BeFalse();
            b4.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Tri_GetAdjacentTris_Should_Work()
        {
            var p0 = new Plan.Point(0, 0);
            var p1 = new Plan.Point(1, 0);
            var p2 = new Plan.Point(0, 1);
            var p3 = new Plan.Point(3, 0);
            var p4 = new Plan.Point(0, 3);
            var p5 = new Plan.Point(3, 3);
            var p6 = new Plan.Point(0, -1);
            var t0 = new Plan.Tri(new HashSet<Plan.Point>() { p0, p1, p2 });
            var t1 = new Plan.Tri(new HashSet<Plan.Point>() { p1, p2, p3 });
            var t2 = new Plan.Tri(new HashSet<Plan.Point>() { p2, p3, p4 });
            var t3 = new Plan.Tri(new HashSet<Plan.Point>() { p3, p4, p5 });
            var t4 = new Plan.Tri(new HashSet<Plan.Point>() { p0, p2, p6 });
            var expectedAdjacentToT0 = new HashSet<Plan.Tri>() { t1, t4 };
            var expectedAdjacentToT3 = new HashSet<Plan.Tri>() { t2 };
            HashSet<Plan.Tri> adjacentToT0, adjacentToT3;

            adjacentToT0 = t0.GetAdjacentTris();
            adjacentToT3 = t3.GetAdjacentTris();

            adjacentToT0.Should().BeEquivalentTo(expectedAdjacentToT0);
            adjacentToT3.Should().BeEquivalentTo(expectedAdjacentToT3);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Tri_GetSharedPoints_Should_Work()
        {
            var p0 = new Plan.Point(0, 0);
            var p1 = new Plan.Point(1, 0);
            var p2 = new Plan.Point(0, 1);
            var p3 = new Plan.Point(3, 0);
            var t0 = new Plan.Tri(new HashSet<Plan.Point>() { p0, p1, p2 });
            var t1 = new Plan.Tri(new HashSet<Plan.Point>() { p1, p2, p3 });
            var expectedSharedPoints = new Plan.Point[] { p1, p2 };
            Plan.Point[] sharedPoints;

            sharedPoints = t0.GetSharedPoints(t1);

            sharedPoints.Should().BeEquivalentTo(expectedSharedPoints);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Load_Should_PopulateProperties()
        {
            Plan plan = await Plan.Load(0, new TestPlanProvider());
            var pointsAt2_2 = plan.Points.Where(p => p.X == 2 && p.Y == 2);
            var trisAt2_2 = plan.Tris.Where(t => t.Points.Where(p => p.X == 2 && p.Y == 2).Count() > 0);

            plan.Points.Count.Should().Be(16);
            plan.Tris.Count.Should().Be(18);
            plan.Route.Count.Should().Be(6);

            pointsAt2_2.Count().Should().Be(1);
            trisAt2_2.Count().Should().Be(6);
            pointsAt2_2.First().Tris.ShouldBeEquivalentTo(trisAt2_2);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task GetTriContainingPoint_Should_Work()
        {
            Plan plan = await Plan.Load(0, new TestPlanProvider());

            var tri = plan.GetTriContainingPoint(3.5f, 3.5f);

            tri.Points.Should().BeEquivalentTo(new HashSet<Plan.Point>()
            {
                new Plan.Point(2, 4),
                new Plan.Point(4, 2),
                new Plan.Point(4, 4)
            });
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task SearchForPathBetweenTris_Should_Work()
        {
            Plan plan = await Plan.Load(0, new TestPlanProvider());
            var from = plan.GetTriContainingPoint(0.5f, 0.5f);
            var t1 = plan.GetTriContainingPoint(1.5f, 1.5f);
            var t2 = plan.GetTriContainingPoint(2.5f, 0.5f);
            var to = plan.GetTriContainingPoint(3.5f, 1.5f);

            var path = plan.SearchForPathBetweenTris(from, to);

            path.ShouldBeEquivalentTo(new Plan.Tri[] { from, t1, t2, to });
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Save_Should_Reserialize()
        {
            List<float[]> points = null;
            List<int[]> tris = null;
            List<float[]> route = null;
            int vendorPoint = -1;
            int mapId = -1;
            int indexOfPoint4_0 = -1;
            TestPlanProvider provider = new TestPlanProvider((serializablePlan, savedMapId) =>
            {
                points = serializablePlan.Points;
                tris = serializablePlan.Tris;
                route = serializablePlan.Route;
                vendorPoint = serializablePlan.VendorPoint;
                mapId = savedMapId;
                indexOfPoint4_0 = serializablePlan.Points.FindIndex(p => p[0] == 4 && p[1] == 0);
            });
            Plan plan = await Plan.Load(7, provider);

            plan.Save();

            points.Count().Should().Be(16);
            foreach (var p in provider.SerializablePlan.Points)
            {
                points.Where(q => q[0] == p[0] && q[1] == p[1]).Count().Should().Be(1);
            }

            tris.Count().Should().Be(18);
            tris.Where(t => t.Contains(indexOfPoint4_0)).Count().Should().Be(3);
            route.ShouldBeEquivalentTo(provider.SerializablePlan.Route);
            vendorPoint.Should().Be(provider.SerializablePlan.VendorPoint);
            mapId.Should().Be(7);
        }
    }
}
