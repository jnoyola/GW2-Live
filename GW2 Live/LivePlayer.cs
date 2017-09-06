using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GW2_Live
{
    class LivePlayer
    {
        private ProcessHandler proc;
        private MumbleHandler mumble;
        private Plan plan;

        private IList<Plan.Point> targets;
        private IList<Plan.Tri> cachedTriPath;
        private Plan.Point viaPoint;


        private class Target
        {
            public float X { get; }
            public float Y { get; }
            public bool ShouldTurnInPlace { get; }

            public Target(float x, float y, bool shouldTurnInPlace)
            {
                X = x;
                Y = y;
                ShouldTurnInPlace = shouldTurnInPlace;
            }
        }

        public LivePlayer(ProcessHandler proc, MumbleHandler mumble)
        {
            this.proc = proc;
            this.mumble = mumble;
        }

        public async Task PlayAsync()
        {
            plan = await Plan.LoadFromFile(mumble.GetIdentity().map_id);

            foreach (var point in plan.Route)
            {
                var cancellationTokenSource = new CancellationTokenSource();
                while (true)
                {
                    var gatheringNode = await MoveAndSearch(point, cancellationTokenSource.Token);

                    if (gatheringNode == null)
                    {
                        // We've arrived at the target.
                        break;
                    }
                    else
                    {
                        // We've found a node to gather on a detour.
                    }
                }
            }
        }

        private async Task<Plan.Point> MoveAndSearch(double xTarget, double yTarget, CancellationToken cancellationToken, double distanceThreshold = 2, bool shouldTurnInPlace = true)
        {
            viaPoint = null;

            while (true)
            {
                float x = mumble.GetX();
                float y = mumble.GetY();

                if (GetDistSqr(x, y, xTarget, yTarget) < distanceThreshold)
                {
                    viaPoint = null;
                }
            }

            return null;
        }

        private async Task<Plan.Point> FindGatherNode()
        {
            return null;
        }

        private Target FindViaPoint(float xFrom, float yFrom, float xTo, float yTo)
        {
            if (cachedTriPath == null)
            {
                Plan.Tri triFrom = plan.GetTriContainingPoint(xFrom, yFrom);
                Plan.Tri triTo = plan.GetTriContainingPoint(xTo, yTo);
                cachedTriPath = plan.SearchForPathBetweenTris(triFrom, triTo);
            }

            if (cachedTriPath.Count == 1)
            {
                return null;
            }

            // Find the two shared points between the first 2 tris as the interesection of their Points sets.
            float[] xShared = new float[2];
            float[] yShared = new float[2];
            int i = 0;

            foreach (Plan.Point pA in cachedTriPath[0].Points)
            {
                foreach (Plan.Point pB in cachedTriPath[1].Points)
                {
                    if (pA.Equals(pB))
                    {
                        xShared[i] = pA.X;
                        yShared[i] = pA.Y;
                        ++i;
                    }
                }
            }
            
            if (i != 2)
            {
                throw new Exception($"Via point calculation couldn't find 2 points shared among adjacent tris. {i} shared points were found instead.");
            }

            // TODO: add some element of randomness here.
            float x = (xShared[0] + xShared[1]) / 2;
            float y = (yShared[0] + yShared[1]) / 2;
            return new Target(x, y, false);
        }

        private double GetAngleDiff(float xFrom, float yFrom, float xTo, float yTo)
        {
            double aFrom = Math.Atan2(yFrom, xFrom);
            double aTo = Math.Atan2(yTo, xTo);

            double da = aTo - aFrom;
            if (da < 0)
            {
                da += 2 * Math.PI;
            }

            return Math.Min(da, 2 * Math.PI - da);
        }

        private double GetDistSqr(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}
