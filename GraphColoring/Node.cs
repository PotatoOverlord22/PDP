namespace GraphColoring
{
    public class Node
    {
        public int Id { get; private set; }
        public int? Color { get; set; }
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

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}