namespace GW2_Live.Player
{
    public class Node
    {
        public float X { get; }
        public float Y { get; }
        public Plan.Tri Tri { get; }
        public bool ShouldTurnInPlace { get; }

        public Node(float x, float y, Plan.Tri tri) : this(x, y, tri, false)
        {
        }

        public Node(float x, float y, Plan.Tri tri, bool shouldTurnInPlace)
        {
            X = x;
            Y = y;
            Tri = tri;
            ShouldTurnInPlace = shouldTurnInPlace;
        }
    }
}
