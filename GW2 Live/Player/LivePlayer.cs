using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GW2_Live.GameInterface;

namespace GW2_Live.Player
{
    class LivePlayer
    {
        private ProcessHandler proc;
        private MumbleHandler mumble;
        private Plan plan;

        private Node currRouteNode;
        private Node currDetourNode;

        private IList<Plan.Point> targets;
        private IList<Plan.Tri> cachedTriPath;
        private int cachedStartIndex;
        private Plan.Point viaPoint;

        public LivePlayer(ProcessHandler proc, MumbleHandler mumble)
        {
            this.proc = proc;
            this.mumble = mumble;
        }

        public async Task PlayAsync(CancellationToken cancellationToken)
        {
            plan = await Plan.LoadFromFile(mumble.GetIdentity().map_id);
            GenerateRoute();

            while (!cancellationToken.IsCancellationRequested)
            {
                Node currNode;
                Node foundNode;

                if (currDetourNode != null)
                {
                    // Continue along the detour.
                    currNode = currDetourNode;
                    foundNode = null;// await MoveAndSearch(currNode, cancellationToken);
                    currDetourNode = currDetourNode.Next;
                }
                else
                {
                    // Continue along the route.
                    currNode = currRouteNode;
                    foundNode = null;// await MoveAndSearch(currNode, cancellationToken);
                    currRouteNode = currRouteNode.Next;
                }

                if (foundNode != null)
                {
                    // Add the foundNode and a path leading to it to the detour.
                    if (currDetourNode == null)
                    {
                        FindPath(currNode, foundNode, out Node startNode, out Node endNode);

                        if (startNode == null)
                        {
                            currDetourNode = foundNode;
                        }
                        else
                        {
                            currDetourNode = startNode;
                            endNode.InsertAndGetNext(foundNode);
                        }
                    }
                    else
                    {
                        currDetourNode.InsertAndGetNext(foundNode);

                        FindPath(foundNode.Previous, foundNode, out Node startNode, out Node endNode);

                        if (startNode != null)
                        {
                            foundNode.Previous.InsertAndGetNext(startNode);
                            endNode.InsertAndGetNext(foundNode);
                        }
                    }
                }
            }




            //foreach (var point in plan.Route)
            //{
            //    var cancellationTokenSource = new CancellationTokenSource();
            //    while (true)
            //    {
            //        var gatheringNode = await MoveAndSearch(point, cancellationTokenSource.Token);

            //        if (gatheringNode == null)
            //        {
            //            // We've arrived at the target.
            //            break;
            //        }
            //        else
            //        {
            //            // We've found a node to gather on a detour.
            //        }
            //    }
            //}
        }

        private void GenerateRoute()
        {
            var startPoint = plan.Route[0];
            plan.Route.Add(startPoint);
            var root = new PathNode(startPoint.X, startPoint.Y);
            currRouteNode = root;

            for (int i = 1; i < plan.Route.Count; ++i)
            {
                Node startNode, endNode;

                // Find the path between the Tris containing the route nodes.
                FindPath(plan.Route[i - 1].Tris.First(), plan.Route[i].Tris.First(), out startNode, out endNode);

                if (startNode == null || endNode == null)
                {
                    endNode = currRouteNode;
                }
                else
                {
                    // Stitch in the startNode.
                    currRouteNode.InsertAndGetNext(startNode);
                }

                // Stitch in the endNode and next route node.
                currRouteNode = new PathNode(plan.Route[i].X, plan.Route[i].Y);
                endNode.InsertAndGetNext(currRouteNode);
            }

            currRouteNode = root;
        }

        //private async Task<Plan.Point> MoveAndSearch(float xTarget, float yTarget, CancellationToken cancellationToken, double distanceThreshold = 2)
        //{
        //    viaPoint = null;

        //    while (true)
        //    {
        //        float x = mumble.GetX();
        //        float y = mumble.GetY();

        //        if (GetDistSqr(x, y, xTarget, yTarget) < distanceThreshold)
        //        {
        //            viaPoint = null;
        //        }
        //    }

        //    return null;
        //}

        //private async Task<Node> FindGatherNode()
        //{
        //    return null;
        //}

        private void FindPath(Node nodeFrom, Node nodeTo, out Node startNode, out Node endNode)
        {
            Plan.Tri triFrom = plan.GetTriContainingPoint(nodeFrom.X, nodeFrom.Y);
            Plan.Tri triTo = plan.GetTriContainingPoint(nodeTo.X, nodeTo.Y);

            FindPath(triFrom, triTo, out startNode, out endNode);
        }

        private void FindPath(Plan.Tri triFrom, Plan.Tri triTo, out Node startNode, out Node endNode)
        {
            var path = plan.SearchForPointsConnectingTris(triFrom, triTo);

            if (path == null)
            {
                startNode = null;
                endNode = null;
                return;
            }

            startNode = new PathNode(path[0].X, path[0].Y);
            endNode = startNode;

            for (int i = 1; i < path.Count; ++i)
            {
                var newNode = new PathNode(path[i].X, path[i].Y);
                endNode.InsertAndGetNext(newNode);
                endNode = newNode;
            }
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
