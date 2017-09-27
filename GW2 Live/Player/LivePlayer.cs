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
        public List<Node> Route { get; private set; }
        public Queue<Node> FoundNodes { get; private set; }
        public Queue<Node> ViaRoute { get; private set; }
        public bool IsTurningInPlace { get; private set; }

        public LivePlayer(IProcessHandler proc, ICharacterStateProvider character, IInputHandler input) : this(proc, character, input, null)
        {
        }

        public LivePlayer(IProcessHandler proc, ICharacterStateProvider character, IInputHandler input, IPlanProvider planProvider)
        {
            this.proc = proc;
            this.character = character;
            this.input = input;
            this.planProvider = planProvider;

            FoundNodes = new Queue<Node>();
            ViaRoute = new Queue<Node>();
        }

        public async Task PlayAsync(CancellationToken cancellationToken)
        {
            await LoadAndGenerateRoute();

            // Fire and forget task to continuously search for gather nodes and update CurrentDetourNode.
            await Task.Run(() => SearchForGatherNodes(cancellationToken));

            // Start by facing the target before moving.
            IsTurningInPlace = true;

            while (!cancellationToken.IsCancellationRequested)
            {
                var x = character.GetX();
                var y = character.GetY();

                // Acquire target node.
                var target = GenerateViaAndTarget(x, y);

                // Turn towards the target.
                TurnToward(x, y, target);

                // Move towards the target.
                if (!IsTurningInPlace)
                {
                    MoveToward(x, y, target);
                }

                // Wait for movement.
                await Task.Delay(UpdateInterval, cancellationToken);

                // Check completion.
                await CheckCompletion(target, cancellationToken);
            }
        }

        public async Task LoadAndGenerateRoute()
        {
            plan = await Plan.Load(character.GetIdentity().map_id, planProvider);

            NextRouteIndex = 0;
            Route = plan.Route.Select(p => new Node(p.X, p.Y, p.Tris.First())).ToList();
        }

        public Node GenerateViaAndTarget(float x, float y)
        {
            var target = ViaRoute.PeekOrDefault() ?? FoundNodes.PeekOrDefault() ?? Route[NextRouteIndex];
            if (!target.Tri.ContainsPoint(x, y))
            {
                // Recalculate ViaRoute.
                target = FoundNodes.PeekOrDefault() ?? Route[NextRouteIndex];
                GenerateViaRoute(x, y, target);

                if (ViaRoute.Count != 0)
                {
                    target = ViaRoute.PeekOrDefault();
                }
            }

            return target;
        }

        public void TurnToward(float x, float y, Node target)
        {
            double angleDiff = MathUtils.GetAngleDiff(x, y, target.X, target.Y);
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
                IsTurningInPlace = false;
            }
        }

        public void MoveToward(float x, float y, Node target)
        {
            // Assume we're already facing the target.
            input.Move(1);
        }

        public async Task CheckCompletion(Node target, CancellationToken cancellationToken)
        {
            var x = character.GetX();
            var y = character.GetY();
 
            if (MathUtils.GetDistSqr(x, y, target.X, target.Y) < DistanceThresholdSquared)
            {
                // Check if we should turn in place at this node.
                IsTurningInPlace = target.ShouldTurnInPlace;
                if (IsTurningInPlace)
                {
                    input.Move(0);
                }

                // Check which target list to advance.
                if (target == ViaRoute.PeekOrDefault())
                {
                    ViaRoute.Dequeue();
                }
                else if (target == FoundNodes.PeekOrDefault())
                {
                    // Gather this node.
                    await Gather(cancellationToken);

                    lock (FoundNodes)
                    {
                        FoundNodes.Dequeue();
                    }
                }
                else if (target == Route[NextRouteIndex])
                {
                    ++NextRouteIndex;
                    if (NextRouteIndex == Route.Count)
                    {
                        NextRouteIndex = 0;
                    }
                }
                else
                {
                    throw new Exception("Completed node was not found in any list.");
                }
            }
        }

        private async void SearchForGatherNodes(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // TODO: use screenshots and cv to find gathering nodes, and convert the coordinates to mumble units.
                IEnumerable<Node> newFoundNodes = null;

                if (newFoundNodes != null)
                {
                    // Add new found nodes to the list.
                    lock (FoundNodes)
                    {
                        foreach (Node newFoundNode in newFoundNodes)
                        {
                            bool alreadyFound = false;

                            foreach (Node alreadyFoundNode in FoundNodes)
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

        private async Task Gather(CancellationToken cancellationToken)
        {
            input.Interact();
            await Task.Delay(GatherDelay, cancellationToken);
        }

        private void GenerateViaRoute(float x, float y, Node target)
        {
            ViaRoute.Clear();

            var fromTri = plan.GetTriContainingPoint(x, y);
            var triPath = plan.SearchForPathBetweenTris(fromTri, target.Tri);

            for (int i = 0; i < triPath.Count - 1; ++i)
            {
                var sharedPoints = triPath[i].GetSharedPoints(triPath[i + 1]);
                GetPointBetween(sharedPoints[0], sharedPoints[1], out float xVia, out float yVia);
                ViaRoute.Enqueue(new Node(xVia, yVia, triPath[i]));
            }
        }

        private void GetPointBetween(Plan.Point a, Plan.Point b, out float x, out float y)
        {
            // TODO: add an element of randomness.
            x = (a.X + b.X) / 2;
            y = (a.Y + b.Y) / 2;
        }
    }
}
