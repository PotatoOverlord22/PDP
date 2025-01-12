using QuickGraph;

namespace GraphTest.BL
{
    public class GraphColoringThreadedBL
    {
        private BidirectionalGraph<Vertex, IEdge<Vertex>> _graph;

        public BidirectionalGraph<Vertex, IEdge<Vertex>> Graph { get { return _graph; } }

        public GraphColoringThreadedBL()
        {
            _graph = new BidirectionalGraph<Vertex, IEdge<Vertex>>();
            PopulateGraph();
        }

        public BidirectionalGraph<object, IEdge<object>> GetObjectGraph()
        {
            var objectGraph = new BidirectionalGraph<object, IEdge<object>>();

            foreach (var vertex in _graph.Vertices)
                objectGraph.AddVertex((object) vertex);

            foreach (var edge in _graph.Edges)
                objectGraph.AddEdge(new Edge<object>((object) edge.Source, (object) edge.Target));

            return objectGraph;
        }

        private void PopulateGraph()
        {
            Vertex A = new Vertex("A");
            Vertex B = new Vertex("B");
            Vertex C = new Vertex("C");

            _graph.AddVertex(A);
            _graph.AddVertex(B);
            _graph.AddVertex(C);

            _graph.AddEdge(new Edge<Vertex>(A, B));
            _graph.AddEdge(new Edge<Vertex>(B, C));
            _graph.AddEdge(new Edge<Vertex>(A, C));
        }
    }
}