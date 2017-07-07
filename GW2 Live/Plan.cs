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

        public class JsonPlan
        {
            public List<float[]> Points { get; }
            public List<int[]> Tris { get; }
            public List<float[]> Route { get; }

            private Dictionary<Point, int> pointIndices;

            public JsonPlan(Plan plan)
            {
                Points = new List<float[]>();
                Tris = new List<int[]>();
                Route = new List<float[]>();

                pointIndices = new Dictionary<Point, int>();

                foreach (Point p in plan.Points)
                {
                    Points.Add(new float[2] { p.X, p.Y });
                    pointIndices[p] = Points.Count - 1;
                }

                foreach (Tri t in plan.Tris)
                {
                    Tris.Add(t.Points.Select(p => pointIndices[p]).ToArray());
                }

                foreach (Point p in plan.Route)
                {
                    Route.Add(new float[2] { p.X, p.Y });
                }
            }

            public class NoFormatArrayConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return objectType.IsArray;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    writer.WriteRawValue(JsonConvert.SerializeObject(value, Formatting.None));
                }
            }
        }

        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW2 Live", "plans");
        private static readonly string FilePathFormat = Path.Combine(FolderPath, "{0}.json");

        public int MapId { get; }
        public HashSet<Point> Points { get; }
        public HashSet<Tri> Tris { get; }
        public List<Point> Route { get; }

        public Plan(int mapId)
        {
            MapId = mapId;
            Points = new HashSet<Point>();
            Tris = new HashSet<Tri>();
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

        public void SaveToFile()
        {
            Directory.CreateDirectory(FolderPath);
            using (var writer = File.CreateText(String.Format(FilePathFormat, MapId)))
            {
                var serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Converters.Add(new JsonPlan.NoFormatArrayConverter());
                serializer.Serialize(writer, new JsonPlan(this));
            }
        }

        public static async Task<Plan> LoadFromFile(int mapId)
        {
            Plan plan = new Plan(mapId);
            List<Point> pointList = new List<Point>();

            using (var reader = new StreamReader(String.Format(FilePathFormat, mapId)))
            {
                string json = await reader.ReadToEndAsync();
                var jsonGraph = JsonConvert.DeserializeObject<JsonPlan>(json);

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

                    Tri t = new Tri(triPoints);
                    plan.Tris.Add(t);
                }

                foreach (var pointArray in jsonGraph.Route)
                {
                    Point p = new Point(pointArray[0], pointArray[1]);
                    plan.Route.Add(p);
                }
            }

            return plan;
        }
    }
}
