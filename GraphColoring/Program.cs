using System.Diagnostics;

namespace GraphColoring
{
    public class Program
    {
        private static readonly int nColors = 20;
        private static readonly int numThreads = 10;
        private static readonly int numNodes = 5;
        private static readonly int numEdges = 1;

        static void Main(string[] args)
        {
            /*while (!Debugger.IsAttached)
            {
                Console.WriteLine("Waiting for debugger");
                Thread.Sleep(2000);
            }*/

            ColorGraph graph = new ColorGraph();

            for (int i = 0; i < numNodes; i++)
            {
                Node node = new Node(i);
                graph.AddNode(node);
            }

            Random rand = new Random();
            for (int i = 0; i < numEdges; i++)
            {
                List<Node> nodesList = graph.Nodes.ToList();

                Node node1 = nodesList[rand.Next(nodesList.Count)];
                Node node2 = nodesList[rand.Next(nodesList.Count)];

                while (node1 == node2)
                {
                    node2 = nodesList[rand.Next(nodesList.Count)];
                }

                graph.AddEdge(node1, node2);
            }

            GraphColoringBL graphBL = new GraphColoringBL(graph, nColors);

            graphBL.MPIGraphColoring(ref args);

            /*graphBL.ThreadedGraphColoring(numThreads);*/
        }
    }
}