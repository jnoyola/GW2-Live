namespace GW2_Live.Player
{
    public abstract class Node
    {
        public Node Previous { get; set; }
        public Node Next { get; set; }
        protected abstract int Priority { get; }

        public float X { get; }
        public float Y { get; }
        public abstract bool ShouldTurnInPlace { get; }

        public Node(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Node RemoveAndGetNext()
        {
            Previous.Next = this.Next;
            Next.Previous = this.Previous;

            return this.Next;
        }

        public Node InsertAndGetNext(Node node)
        {
            if (node.Priority < this.Priority)
            {
                return InsertBefore(node);
            }

            if (this.Next == null)
            {
                return InsertAfter(node);
            }

            this.Next.InsertAndGetNext(node);
            return this;
        }

        private Node InsertBefore(Node node)
        {
            if (Previous != null)
            {
                Previous.Next = node;
            }
            node.Previous = this.Previous;
            node.Next = this;
            this.Previous = node;

            return node;
        }

        private Node InsertAfter(Node node)
        {
            if (Next != null)
            {
                Next.Previous = node;
            }
            node.Next = this.Next;
            this.Next = node;
            node.Previous = this;

            return this;
        }
    }

    public class PathNode : Node
    {
        protected override int Priority { get; } = 2;
        public override bool ShouldTurnInPlace { get; } = false;

        public PathNode(float x, float y) : base(x, y)
        {
        }
    }

    public class GatherNode : Node
    {
        protected override int Priority { get; } = 1;
        public override bool ShouldTurnInPlace { get; } = true;

        public GatherNode(float x, float y) : base(x, y)
        {
        }
    }

    public class ViaNode : Node
    {
        protected override int Priority { get; } = 0;
        public override bool ShouldTurnInPlace { get; } = false;

        public ViaNode(float x, float y) : base(x, y)
        {
        }
    }
}
