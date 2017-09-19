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
        private const double AngleTolerance = 0.1;

        private IProcessHandler proc;
        private ICharacterStateProvider character;
        private IInputHandler input;
        private IPlanProvider planProvider;
        private Plan plan;

        public int NextRouteIndex { get; private set; }
        public List<NNode> Route { get; private set; }
        public Queue<NNode> FoundNodes { get; private set; }
        public Queue<NNode> ViaRoute { get; private set; }
        private object foundNodeLock;

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

            // Fire and forget task to continuously search for gather nodes and update CurrentDetourNode.
            await Task.Run(() => SearchForGatherNodes(cancellationToken));

            // Start by turning in place and heading towards the start.
            await TurnToFace(CurrentDetourNode ?? CurrentRouteNode, cancellationToken);
            input.Move(1);

            // TODO: do something with the vendor node.

            while (!cancellationToken.IsCancellationRequested)
            {
                bool isRoute = CurrentDetourNode == null;
                Node currNode = isRoute ? CurrentRouteNode : CurrentDetourNode;

                // Turn towards the target.
                double angleDiff = MathUtils.GetAngleDiff(character.GetX(), character.GetY(), currNode.X, currNode.Y);
                if (angleDiff > AngleTolerance)
                {
                    input.Turn(1);
                }
                else if (angleDiff < -AngleTolerance)
                {
                    input.Turn(-1);
                }
                else
                {
                    input.Turn(0);
                }

                // Check if we've reached the target.
                if (MathUtils.GetDistSqr(character.GetX(), character.GetY(), currNode.X, currNode.Y) < DistanceThresholdSquared)
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
                        input.Move(0);

                        Node nextNode = CurrentDetourNode ?? CurrentRouteNode;
                        await TurnToFace(nextNode, cancellationToken);

                        input.Move(1);
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

            NextRouteIndex = 0;
            Route = plan.Route.Select(p => new NNode(p.X, p.Y, p.Tris.First())).ToList();
        }

        private async Task TurnToFace(Node target, CancellationToken cancellationToken)
        {
            double initialDiff = MathUtils.GetAngleDiff(character.GetX(), character.GetY(), target.X, target.Y);

            if (initialDiff > AngleTolerance)
            {
                input.Turn(1);
            }
            else if (initialDiff < -AngleTolerance)
            {
                input.Turn(-1);
            }
            else
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                double diff = MathUtils.GetAngleDiff(character.GetX(), character.GetY(), target.X, target.Y);

                if (initialDiff > 0)
                {
                    if (diff <= AngleTolerance)
                    {
                        input.Turn(0);
                        return;
                    }
                }
                else
                {
                    if (diff >= -AngleTolerance)
                    {
                        input.Turn(0);
                        return;
                    }
                }

                await Task.Delay(UpdateInterval, cancellationToken);
            }
        }

        private async void SearchForGatherNodes(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // TODO: use screenshots and cv to find gathering nodes, and convert the coordinates to mumble units.
                IEnumerable<NNode> newFoundNodes = null;

                if (newFoundNodes != null)
                {
                    // Add new found nodes to the list.
                    lock (foundNodeLock)
                    {
                        foreach (NNode newFoundNode in newFoundNodes)
                        {
                            bool alreadyFound = false;

                            foreach (NNode alreadyFoundNode in FoundNodes)
                            {
                                if (MathUtils.GetDistSqr(newFoundNode.X, newFoundNode.Y, alreadyFoundNode.X, alreadyFoundNode.Y) < SameNodeDistanceToleranceSquared)
                                {
                                    alreadyFound = true;
                                    break;
                                }
                            }
                            
                            if (!alreadyFound)
                            {
                                FoundNodes.Enqueue(newFoundNode);
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
    }
}
