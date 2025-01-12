using GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphTest.BL;
using QuickGraph;
using System.Windows;

namespace GraphTest
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GraphColoringThreadedBL graphColoringThreadedBL = new GraphColoringThreadedBL();

            ApplyLayout(graphColoringThreadedBL.Graph);

            graphLayout.Graph = graphColoringThreadedBL.GetObjectGraph();
        }

        private void ApplyLayout(BidirectionalGraph<Vertex, IEdge<Vertex>> graph)
        {
            var vertexPositions = new Dictionary<Vertex, Point>();
            var vertexSizes = new Dictionary<Vertex, Size>();

            double xPosition = 0; 
            double yPosition = 0;  
            double vertexWidth = 20;
            double vertexHeight = 20;

            foreach (var vertex in graph.Vertices)
            {
                // Set the position of each vertex (e.g., in a line)
                vertexPositions[vertex] = new Point(xPosition, yPosition);

                // Set a default size for each vertex
                vertexSizes[vertex] = new Size(vertexWidth, vertexHeight);

                // Increment position for next vertex (move along the x-axis)
                xPosition += vertexWidth + 10; // Adding 10 as the gap between vertices
            }

            var layoutParameters = new SimpleTreeLayoutParameters(); // Initialize layout parameters
            var layout = new SimpleTreeLayoutAlgorithm<Vertex, IEdge<Vertex>, BidirectionalGraph<Vertex, IEdge<Vertex>>>(
                graph,
                vertexPositions,
                vertexSizes,
                layoutParameters 
            );

            layout.Compute();

            foreach (var vertex in vertexPositions)
            {
                Console.WriteLine($"Vertex {vertex.Key}: Position = {vertex.Value}");
            }
        }
    }
}