using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GW2_Live
{
    class Plan
    {
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
        }

        public struct JsonPlan
        {
            public List<float[]> Points { get; set; }
            public List<int[]> Tris { get; set; }
            public List<float[]> Route { get; set; }
            public int VendorPoint { get; set; }
        }

        private static readonly string Folder = "plans";
        private static readonly string FileFormat = "{0}.json";

        public int MapId { get; }
        public HashSet<Point> Points { get; }
        public HashSet<Tri> Tris { get; }
        public List<Point> Route { get; }
        public Point VendorPoint { get; private set; }

        public Plan(int mapId)
        {
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
                p.Tris.Add(GetTriContainingPoint(p));
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

        public Tri GetTriContainingPoint(Point p)
        {
            foreach (Tri tri in Tris)
            {
                var ps = tri.Points.ToArray();
                float inverseOfTwiceArea = 1 / (-ps[1].Y * ps[2].X + ps[0].Y * (-ps[1].X + ps[2].X) + ps[0].X * (ps[1].Y - ps[2].Y) + ps[1].X * ps[2].Y);
                float s = inverseOfTwiceArea * (ps[0].Y * ps[2].X - ps[0].X * ps[2].Y + (ps[2].Y - ps[0].Y) * p.X + (ps[0].X - ps[2].X) * p.Y);
                float t = inverseOfTwiceArea * (ps[0].X * ps[1].Y - ps[0].Y * ps[1].X + (ps[0].Y - ps[1].Y) * p.X + (ps[1].X - ps[0].X) * p.Y);

                if (s >= 0 && t >= 0 && (1 - s - t) >= 0)
                {
                    return tri;
                }
            }

            return null;
        }

        public List<Tri> SearchForPathBetweenTris(Tri from, Tri to)
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

        public void SaveToFile()
        {
            var jsonPoints = new List<float[]>();
            var jsonTris = new List<int[]>();
            var jsonRoute = new List<float[]>();
            int jsonVendorPoint = -1;

            var pointIndices = new Dictionary<Point, int>();

            foreach (Point p in Points)
            {
                jsonPoints.Add(new float[2] { p.X, p.Y });
                pointIndices[p] = jsonPoints.Count - 1;
            }

            foreach (Tri t in Tris)
            {
                jsonTris.Add(t.Points.Select(p => pointIndices[p]).ToArray());
            }

            foreach (Point p in Route)
            {
                jsonRoute.Add(new float[2] { p.X, p.Y });

                if (p == VendorPoint)
                {
                    jsonVendorPoint = jsonRoute.Count - 1;
                }
            }

            var jsonPlan = new JsonPlan { Points = jsonPoints, Tris = jsonTris, Route = jsonRoute, VendorPoint = jsonVendorPoint };

            FileManager.SaveToFile(jsonPlan, Folder, String.Format(FileFormat, MapId));
        }

        public static async Task<Plan> LoadFromFile(int mapId)
        {
            Plan plan = new Plan(mapId);
            List<Point> pointList = new List<Point>();

            var jsonGraph = await FileManager.ReadFromFile<JsonPlan>(Folder, String.Format(FileFormat, mapId));

            foreach (var pointArray in jsonGraph.Points)
            {
                Point p = new Point(pointArray[0], pointArray[1]);
                plan.Points.Add(p);
                pointList.Add(p);
            }

            foreach (var triArray in jsonGraph.Tris)
            {
                HashSet<Point> triPoints = new HashSet<Point>();

                foreach (int pointIndex in triArray)
                {
                    triPoints.Add(pointList[pointIndex]);
                }

                plan.AddTri(triPoints);
            }

            foreach (var pointArray in jsonGraph.Route)
            {
                plan.AddRoutePoint(pointArray[0], pointArray[1], true);
            }

            return plan;
        }

        public static async Task<Plan> LoadOrCreate(int mapId)
        {
            try
            {
                return await LoadFromFile(mapId);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                return new Plan(mapId);
            }
        }
    }
}
