﻿namespace GW2_Live.Player
{
    abstract class Node
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
            if (node.Priority > this.Priority)
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
            Previous.Next = node;
            node.Previous = this.Previous;
            node.Next = this;
            this.Previous = node;

            return node;
        }

        private Node InsertAfter(Node node)
        {
            this.Next = node;
            node.Previous = this;
            node.Next = this.Next;
            Next.Previous = node;

            return this;
        }
    }

    class PathNode : Node
    {
        protected override int Priority { get; } = 2;
        public override bool ShouldTurnInPlace { get; } = false;

        public PathNode(float x, float y) : base(x, y)
        {
        }
    }

    class GatherNode : Node
    {
        protected override int Priority { get; } = 1;
        public override bool ShouldTurnInPlace { get; } = true;

        public GatherNode(float x, float y) : base(x, y)
        {
        }
    }

    class ViaNode : Node
    {
        protected override int Priority { get; } = 0;
        public override bool ShouldTurnInPlace { get; } = false;

        public ViaNode(float x, float y) : base(x, y)
        {
        }
    }
}
