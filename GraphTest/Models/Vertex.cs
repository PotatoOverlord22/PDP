namespace GraphTest
{
    public class Vertex
    {
        public Vertex(string id)
        {
            Id = id;
        }
        public string Id { get; private set; }
        public int? Color { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }
}