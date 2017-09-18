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
        public async Task SearchForPointsConnectingTris_Should_Work()
        {
            Plan plan = await Plan.Load(0, new TestPlanProvider());
            var from = plan.GetTriContainingPoint(0.5f, 0.5f);
            var t1 = plan.GetTriContainingPoint(1.5f, 1.5f);
            var t2 = plan.GetTriContainingPoint(2.5f, 0.5f);
            var to = plan.GetTriContainingPoint(3.5f, 1.5f);

            var path = plan.SearchForPointsConnectingTris(from, to);

            path.Count.Should().Be(3);
            path[0].X.Should().Be(2 - path[0].Y);
            path[0].Y.Should().BeInRange(0, 2);
            path[1].X.Should().Be(2);
            path[1].Y.Should().BeInRange(0, 2);
            path[2].X.Should().Be(4 - path[2].Y);
            path[2].Y.Should().BeInRange(0, 2);
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
