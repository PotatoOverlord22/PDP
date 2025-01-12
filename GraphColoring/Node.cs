namespace GraphColoring
{
    [Serializable]
    public class Node
    {
        private int? _color;

        public int Id { get; private set; }

        public int? Color
        {
            get => _color;
            set => _color = value;
        }

        [NonSerialized]
        private object _mutex = new object();

        public Node(int id)
        {
            Id = id;
        }

        public void Lock()
        {
            Monitor.Enter(_mutex);
        }

        public void Unlock()
        {
            Monitor.Exit(_mutex);
        }

        public override bool Equals(object obj)
        {
            if (obj is Node otherNode)
            {
                return Id == otherNode.Id;
            }

            return false;
        }

        public ref int? GetColorRef()
        {
            return ref _color;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public Node DeepCopy()
        {
            Node newNode = new Node(Id)
            {
                Color = this.Color
            };

            return newNode;
        }
    }
}