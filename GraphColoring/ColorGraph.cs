using System.Text;

namespace GraphColoring
{
    // this is an undirected graph where the nodes can be labeled (colored)
    [Serializable]
    public class ColorGraph
    {
        private HashSet<Node> _nodes;
        private Dictionary<Node, HashSet<Node>> _adjacency;

        public HashSet<Node> Nodes { get { return _nodes; } }

        public ColorGraph()
        {
            _nodes = new HashSet<Node>();
            _adjacency = new Dictionary<Node, HashSet<Node>>();
        }

        public void AddNode(Node node)
        {
            if (_nodes.Add(node))
                _adjacency[node] = new HashSet<Node>();
        }

        public void AddEdge(Node node1, Node node2)
        {
            if (!_nodes.Contains(node1) || !_nodes.Contains(node2))
                throw new ArgumentException("Both nodes must be added to the graph before adding an edge between them.");

            _adjacency[node1].Add(node2);
            _adjacency[node2].Add(node1);
        }

        public HashSet<Node> GetAdjacentNodes(Node node)
        {
            return _adjacency[node];
        }

        public Node? GetNode(int id)
        {
            return _nodes.FirstOrDefault(n => n.Id == id);
        }

        public Node? GetNode(Node node)
        {
            return _nodes.FirstOrDefault(n => n.Id == node.Id);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Node node in _nodes)
            {
                sb.AppendLine($"Node {node.Id} (Color: {node.Color?.ToString() ?? "Uncolored"}): {string.Join(", ", _adjacency[node].Select(n => n.Id))}");
            }

            return sb.ToString();
        }

        public ColorGraph DeepCopy()
        {
            ColorGraph newGraph = new ColorGraph();

            foreach (Node node in _nodes)
            {
                newGraph.AddNode(node.DeepCopy());
            }

            foreach (Node node in _nodes)
            {
                foreach (Node neighbor in _adjacency[node])
                {
                    newGraph.AddEdge(newGraph.GetNode(node)!, newGraph.GetNode(neighbor)!);
                }
            }

            return newGraph;
        }
    }
}