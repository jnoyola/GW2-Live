using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GW2_Live.GameInterface;

namespace GW2_Live.Player
{
    public class LivePlayer
    {
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan SearchInterval = TimeSpan.FromMilliseconds(1000);
        private static readonly TimeSpan GatherDelay = TimeSpan.FromSeconds(3);
        private const float DistanceThresholdSquared = 4;
        private const float SameNodeDistanceToleranceSquared = 4;

        private IProcessHandler proc;
        private ICharacterStateProvider character;
        private IInputHandler input;
        private IPlanProvider planProvider;
        private Plan plan;

        public Node CurrentRouteNode { get; private set; }
        public Node CurrentDetourNode { get; private set; }
        public HashSet<Node> CurrentFoundNodes { get; private set; }
        private object detourLock;

        private IList<Plan.Point> targets;
        private IList<Plan.Tri> cachedTriPath;
        private int cachedStartIndex;
        private Plan.Point viaPoint;

        public LivePlayer(IProcessHandler proc, ICharacterStateProvider character, IInputHandler input) : this(proc, character, input, null)
        {
        }

        public LivePlayer(IProcessHandler proc, ICharacterStateProvider character, IInputHandler input, IPlanProvider planProvider)
        {
            this.proc = proc;
            this.character = character;
            this.planProvider = planProvider;
        }

        public async Task PlayAsync(CancellationToken cancellationToken)
        {
            await LoadAndGenerateRoute();

            // TODO: start background async task to search for gather nodes and update CurrentDetourNode.

            // Start by turning in place and heading towards the start.
            await TurnToFace(CurrentDetourNode ?? CurrentRouteNode, cancellationToken);
            input.MoveForward(true);

            // TODO: do something with the vendor node.

            while (!cancellationToken.IsCancellationRequested)
            {
                bool isRoute = CurrentDetourNode == null;
                Node currNode = isRoute ? CurrentRouteNode : CurrentDetourNode;

                // TODO: turn towards currNode.

                if (GetDistSqr(character.GetX(), character.GetY(), currNode.X, currNode.Y) < DistanceThresholdSquared)
                {
                    // Update the state of the path we're following.
                    if (isRoute)
                    {
                        CurrentRouteNode = CurrentRouteNode.Next;
                    }
                    else
                    {
                        lock (detourLock)
                        {
                            CurrentFoundNodes.Remove(currNode);
                            CurrentDetourNode = CurrentDetourNode.Next;
                        }

                        await GatherNode(currNode, cancellationToken);
                    }

                    // Turn in place if necessary.
                    if (currNode.ShouldTurnInPlace)
                    {
                        input.MoveForward(false);

                        Node nextNode = CurrentDetourNode ?? CurrentRouteNode;
                        await TurnToFace(nextNode, cancellationToken);

                        input.MoveForward(true);
                    }
                }

                await Task.Delay(UpdateInterval, cancellationToken);
            }




            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    Node currNode;
            //    Node foundNode;

            //    if (CurrentDetourNode != null)
            //    {
            //        // Continue along the detour.
            //        currNode = CurrentDetourNode;
            //        foundNode = await MoveAndSearch(currNode, cancellationToken);
            //        CurrentDetourNode = CurrentDetourNode.Next;
            //    }
            //    else
            //    {
            //        // Continue along the route.
            //        currNode = CurrentRouteNode;
            //        foundNode = await MoveAndSearch(currNode, cancellationToken);
            //        CurrentRouteNode = CurrentRouteNode.Next;
            //    }

            //    if (foundNode != null)
            //    {
            //        // Add the foundNode and a path leading to it to the detour.
            //        if (CurrentDetourNode == null)
            //        {
            //            FindPath(currNode, foundNode, out Node startNode, out Node endNode);

            //            if (startNode == null)
            //            {
            //                CurrentDetourNode = foundNode;
            //            }
            //            else
            //            {
            //                CurrentDetourNode = startNode;
            //                endNode.InsertAndGetNext(foundNode);
            //            }
            //        }
            //        else
            //        {
            //            CurrentDetourNode.InsertAndGetNext(foundNode);

            //            FindPath(foundNode.Previous, foundNode, out Node startNode, out Node endNode);

            //            if (startNode != null)
            //            {
            //                foundNode.Previous.InsertAndGetNext(startNode);
            //                endNode.InsertAndGetNext(foundNode);
            //            }
            //        }
            //    }
            //}
        }

        public async Task LoadAndGenerateRoute()
        {
            plan = await Plan.Load(character.GetIdentity().map_id, planProvider);

            var startPoint = plan.Route[0];
            plan.Route.Add(startPoint);
            var root = new PathNode(startPoint.X, startPoint.Y);
            CurrentRouteNode = root;

            for (int i = 1; i < plan.Route.Count; ++i)
            {
                Node startNode, endNode;

                // Find the path between the Tris containing the route nodes.
                FindPath(plan.Route[i - 1].Tris.First(), plan.Route[i].Tris.First(), out startNode, out endNode);

                if (startNode == null || endNode == null)
                {
                    endNode = CurrentRouteNode;
                }
                else
                {
                    // Stitch in the startNode.
                    CurrentRouteNode.InsertAndGetNext(startNode);
                }

                // Stitch in the endNode and next route node.
                CurrentRouteNode = new PathNode(plan.Route[i].X, plan.Route[i].Y);
                endNode.InsertAndGetNext(CurrentRouteNode);
            }

            // Remove the duplicate end/start node and finish the loop.
            plan.Route.RemoveAt(plan.Route.Count - 1);
            CurrentRouteNode.Previous.Next = root;
            root.Previous = CurrentRouteNode.Previous;

            CurrentRouteNode = root;
        }

        private async Task TurnToFace(Node target, CancellationToken cancellationToken)
        {
            // TODO
        }

        private async Task SearchForGatherNodes(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // TODO: use screenshots and cv to find gathering nodes, and convert the coordinates to mumble units.
                List<Node> foundNodes = null;

                if (foundNodes != null)
                {
                    // Add the foundNode and a path leading to it to the detour.
                    lock (detourLock)
                    {
                        foreach (Node foundNode in foundNodes)
                        {
                            if (!CurrentFoundNodes.Contains(foundNode))
                            {
                                if (CurrentDetourNode == null)
                                {
                                    FindPath(character.GetX(), character.GetY(), foundNode.X, foundNode.Y, out Node startNode, out Node endNode);

                                    if (startNode == null)
                                    {
                                        CurrentDetourNode = foundNode;
                                    }
                                    else
                                    {
                                        CurrentDetourNode = startNode;
                                        endNode.InsertAndGetNext(foundNode);
                                    }
                                }
                                else
                                {
                                    CurrentDetourNode.InsertAndGetNext(foundNode);

                                    FindPath(foundNode.Previous.X, foundNode.Previous.Y, foundNode.X, foundNode.Y, out Node startNode, out Node endNode);

                                    if (startNode != null)
                                    {
                                        foundNode.Previous.InsertAndGetNext(startNode);
                                        endNode.InsertAndGetNext(foundNode);
                                    }
                                }
                            }
                        }
                    }
                }

                await Task.Delay(SearchInterval, cancellationToken);
            }
        }

        private async Task GatherNode(Node target, CancellationToken cancellationToken)
        {
            input.Interact();
            await Task.Delay(GatherDelay, cancellationToken);
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

        private void FindPath(float xFrom, float yFrom, float xTo, float yTo, out Node startNode, out Node endNode)
        {
            Plan.Tri triFrom = plan.GetTriContainingPoint(xFrom, yFrom);
            Plan.Tri triTo = plan.GetTriContainingPoint(xTo, yTo);

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

        private float GetDistSqr(float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            return dx * dx + dy * dy;
        }
    }
}
