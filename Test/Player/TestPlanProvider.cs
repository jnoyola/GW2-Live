using GW2_Live.Player;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test.Player
{
    class TestPlanProvider : IPlanProvider
    {
        public Plan.Serializable SerializablePlan { get; }

        private Action<Plan.Serializable, int> saveValidator;

        public TestPlanProvider(Action<Plan.Serializable, int> saveValidator = null)
        {
            this.saveValidator = saveValidator;

            /* Points and Tris are row-major, starting at the bottom left.
             * Route points are in alphabetical order.
             * 
             * 6 +-------+-------+-------+
             *   | \     | \     | \     |
             *   |   \   |   \   |   \   |
             *   |     \ |     \ |     \ |
             * 4 +-------+-------+-------+
             *   | \     | \     | \     |
             *   |   \   |   \   |   \   |
             *   | F   \ |     \ | E   \ |
             * 2 +-------+-------+-------+
             *   | \   B | \   D | \     |
             *   |   \   |   \   |   \   |
             *   | A   \ | C   \ |     \ |
             * 0 +-------+-------+-------+
             *   0       2       4       6
             */

            var points = new List<float[]>();
            var tris = new List<int[]>();

            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    if (x < 3 && y < 3)
                    {
                        tris.Add(new int[] { points.Count, points.Count + 1, points.Count + 4 });
                        tris.Add(new int[] { points.Count + 1, points.Count + 4, points.Count + 5 });
                    }
                    points.Add(new float[] { 2 * x, 2 * y });
                }
            }

            SerializablePlan = new Plan.Serializable
            {
                Points = points,
                Tris = tris,
                Route = new List<float[]>
                {
                    new float[] { 0.5f, 0.5f },
                    new float[] { 1.5f, 1.5f },
                    new float[] { 2.5f, 0.5f },
                    new float[] { 3.5f, 1.5f },
                    new float[] { 4.5f, 2.5f },
                    new float[] { 0.5f, 2.5f }
                },
                VendorPoint = 0
            };
        }

        public Task<Plan.Serializable> Load(int mapId)
        {
            return Task.FromResult(SerializablePlan);
        }

        public void Save(Plan.Serializable plan, int mapId)
        {
            saveValidator?.Invoke(plan, mapId);
        }
    }
}
