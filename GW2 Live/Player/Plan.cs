using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GW2_Live.Player
{
    public class Plan
    {
        public struct Serializable
        {
            public List<float[]> Points { get; set; }
            public List<int[]> Tris { get; set; }
            public List<float[]> Route { get; set; }
            public int VendorPoint { get; set; }
        }

        public class Point
        {
            public float X { get; }
            public float Y { get; }
            public HashSet<Tri> Tris { get; }

            public Point(float x, float y)
            {
                X = x;
                Y = y;
                Tris = new HashSet<Tri>();
            }

            public override bool Equals(object obj)
            {
                Point other = obj as Point;
                return other.X == X && other.Y == Y;
            }

            public override int GetHashCode()
            {
                return (int)(X * Y);
            }
        }

        public class Tri
        {
            public HashSet<Point> Points { get; }

            public Tri(HashSet<Point> points)
            {
                Points = new HashSet<Point>(points);

                foreach (Point p in Points)
                {
                    p.Tris.Add(this);
                }
            }

            public bool ContainsPoint(float x, float y)
            {
                var ps = Points.ToArray();
                float inverseOfTwiceArea = 1 / (-ps[1].Y * ps[2].X + ps[0].Y * (-ps[1].X + ps[2].X) + ps[0].X * (ps[1].Y - ps[2].Y) + ps[1].X * ps[2].Y);
                float s = inverseOfTwiceArea * (ps[0].Y * ps[2].X - ps[0].X * ps[2].Y + (ps[2].Y - ps[0].Y) * x + (ps[0].X - ps[2].X) * y);
                float t = inverseOfTwiceArea * (ps[0].X * ps[1].Y - ps[0].Y * ps[1].X + (ps[0].Y - ps[1].Y) * x + (ps[1].X - ps[0].X) * y);

                if (s >= 0 && t >= 0 && (1 - s - t) >= 0)
                {
                    return true;
                }

                return false;
            }

            public HashSet<Tri> GetAdjacentTris()
            {
                // Find all tris that share 2 points.
                // Since tris only have 3 points total, we only need to iterate over 2.

                HashSet<Tri> adjacentTris = new HashSet<Tri>();

                int i = 0;
                foreach (Point myPoint in Points)
                {
                    foreach (Tri otherTri in myPoint.Tris)
                    {
                        foreach (Point otherPoint in otherTri.Points)
                        {
                            // Skip the point we're already looking at that we know is shared.
                            if (myPoint == otherPoint)
                            {
                                continue;
                            }

                            // If another point is shared, these tris are adjacent.
                            if (Points.Contains(otherPoint))
                            {
                                adjacentTris.Add(otherTri);
                                break;
                            }
                        }
                    }

                    // Break out after testing 2 of my 3 points.
                    ++i;
                    if (i == 2)
                    {
                        break;
                    }
                }

                return adjacentTris;
            }

            public Point[] GetSharedPoints(Tri other)
            {
                // Find the two shared points between the 2 tris as the interesection of their Points sets.
                Point[] shared = new Point[2];
                int j = 0;

                foreach (Point pA in Points)
                {
                    foreach (Point pB in other.Points)
                    {
                        if (pA == pB)
                        {
                            shared[j] = pA;
                            ++j;
                        }
                    }
                }

                return shared;
            }
        }

        public int MapId { get; }
        public HashSet<Point> Points { get; }
        public HashSet<Tri> Tris { get; }
        public List<Point> Route { get; }
        public Point VendorPoint { get; private set; }

        private IPlanProvider planProvider;

        public Plan(int mapId) : this(mapId, PlanProvider.Instance)
        {
        }

        public Plan(int mapId, IPlanProvider planProvider)
        {
            this.planProvider = planProvider;
            MapId = mapId;
            Points = new HashSet<Point>();
            Tris = new HashSet<Tri>();
            Route = new List<Point>();
        }

        public void AddPoint(float x, float y)
        {
            Points.Add(new Point(x, y));
        }

        public void AddTri(HashSet<Point> points)
        {
            Tris.Add(new Tri(points));
        }

        public void AddRoutePoint(float x, float y, bool shouldIncludeTri = false)
        {
            Point p = new Point(x, y);

            if (shouldIncludeTri)
            {
                p.Tris.Add(GetTriContainingPoint(x, y));
            }

            Route.Add(p);
        }

        public void SetVendorPoint(Point p)
        {
            VendorPoint = p;
        }

        public void RemovePoint(Point p)
        {
            // First remove all tris containing this point.
            foreach (Tri tri in p.Tris.ToList())
            {
                RemoveTri(tri);
            }

            Points.Remove(p);
        }

        public void RemoveTri(Tri t)
        {
            // First remove all references to this tri from its points.
            foreach (Point p in t.Points)
            {
                p.Tris.Remove(t);
            }

            Tris.Remove(t);
        }

        public void RemoveAllRoutePoints()
        {
            Route.Clear();
        }

        public Tri GetTriContainingPoint(float x, float y)
        {
            foreach (Tri tri in Tris)
            {
                if (tri.ContainsPoint(x, y))
                {
                    return tri;
                }
            }

            return null;
        }

        public IList<Tri> SearchForPathBetweenTris(Tri from, Tri to)
        {
            var alreadySeen = new HashSet<Tri>();
            var queue = new Queue<List<Tri>>();

            var list = new List<Tri>();
            list.Add(from);
            queue.Enqueue(list);

            while (queue.Count > 0)
            {
                var curList = queue.Dequeue();
                var endTri = curList.Last();

                if (endTri == to)
                {
                    return curList;
                }

                foreach (var nextTri in endTri.GetAdjacentTris())
                {
                    if (!alreadySeen.Contains(nextTri))
                    {
                        var nextList = new List<Tri>(curList);
                        nextList.Add(nextTri);
                        queue.Enqueue(nextList);
                    }
                }
            }

            return null;
        }

        public IList<Point> SearchForPointsConnectingTris(Tri from, Tri to)
        {
            var triPath = SearchForPathBetweenTris(from, to);

            if (triPath.Count == 1)
            {
                // There is no via point required. We can go directly to the destination.
                return null;
            }

            var list = new List<Point>();

            for (int i = 0; i < triPath.Count - 1; ++i)
            {
                // Find the two shared points between the 2 tris as the interesection of their Points sets.
                float[] xShared = new float[2];
                float[] yShared = new float[2];
                int j = 0;

                foreach (Point pA in triPath[i].Points)
                {
                    foreach (Point pB in triPath[i + 1].Points)
                    {
                        if (pA.Equals(pB))
                        {
                            xShared[j] = pA.X;
                            yShared[j] = pA.Y;
                            ++j;
                        }
                    }
                }

                if (j != 2)
                {
                    throw new Exception($"Via point calculation couldn't find 2 points shared among adjacent tris. {i} shared points were found instead.");
                }

                // TODO: add some element of randomness here.
                float x = (xShared[0] + xShared[1]) / 2;
                float y = (yShared[0] + yShared[1]) / 2;

                list.Add(new Point(x, y));
            }

            return list;
        }

        public void Save()
        {
            var pointList = new List<float[]>();
            var triList = new List<int[]>();
            var routeList = new List<float[]>();
            int vendorPointIndex = -1;

            var pointIndices = new Dictionary<Point, int>();

            foreach (Point p in Points)
            {
                pointList.Add(new float[2] { p.X, p.Y });
                pointIndices[p] = pointList.Count - 1;
            }

            foreach (Tri t in Tris)
            {
                triList.Add(t.Points.Select(p => pointIndices[p]).ToArray());
            }

            foreach (Point p in Route)
            {
                routeList.Add(new float[2] { p.X, p.Y });

                if (p == VendorPoint)
                {
                    vendorPointIndex = routeList.Count - 1;
                }
            }

            var serializablePlan = new Plan.Serializable { Points = pointList, Tris = triList, Route = routeList, VendorPoint = vendorPointIndex };

            planProvider.Save(serializablePlan, MapId);
        }

        public static async Task<Plan> Load(int mapId, IPlanProvider planProvider = null)
        {
            if (planProvider == null)
            {
                planProvider = PlanProvider.Instance;
            }

            Plan plan = new Plan(mapId, planProvider);
            List<Point> pointList = new List<Point>();

            var serializablePlan = await planProvider.Load(mapId);

            foreach (var pointArray in serializablePlan.Points)
            {
                Point p = new Point(pointArray[0], pointArray[1]);
                plan.Points.Add(p);
                pointList.Add(p);
            }

            foreach (var triArray in serializablePlan.Tris)
            {
                HashSet<Point> triPoints = new HashSet<Point>();

                foreach (int pointIndex in triArray)
                {
                    triPoints.Add(pointList[pointIndex]);
                }

                plan.AddTri(triPoints);
            }

            foreach (var pointArray in serializablePlan.Route)
            {
                plan.AddRoutePoint(pointArray[0], pointArray[1], true);
            }

            plan.VendorPoint = plan.Route[serializablePlan.VendorPoint];

            return plan;
        }

        public static async Task<Plan> LoadOrCreate(int mapId)
        {
            try
            {
                return await Load(mapId);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return new Plan(mapId);
            }
        }
    }
}
