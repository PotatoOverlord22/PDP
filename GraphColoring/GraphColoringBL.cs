namespace GraphColoring
{
    public class GraphColoringBL
    {
        private readonly ColorGraph _graph;
        private readonly int _nColors;

        public GraphColoringBL(int nColors)
        {
            _graph = new ColorGraph();
            _nColors = nColors;
        }

        public GraphColoringBL(ColorGraph graph, int nColors)
        {
            _graph = graph;
            _nColors = nColors;
        }

        public void PrintGraph()
        {
            Console.WriteLine($"Graph:\n{_graph}");
        }

        public bool IsValidColoredGraph()
        {
            foreach (Node node in _graph.Nodes)
            {
                if (!_graph.GetAdjacentNodes(node).All(neighbor => node.Color != neighbor.Color))
                {
                    return false;
                }
            }

            return true;
        }

        public void ThreadedGraphColoring(int numThreads)
        {
            List<HashSet<Node>> partitions = PartitionGraph(numThreads);

            Parallel.For(0, numThreads, threadId =>
            {
                HashSet<Node> partition = partitions[threadId];
                HashSet<Node> boundaryVertices = IdentifyBoundaryVertices(partition);

                Console.WriteLine($"Thread {threadId} is coloring nodes: {string.Join(", ", partition)}");

                foreach (var node in partition.Except(boundaryVertices))
                {
                    node.Color = GetMinimumLegalColor(node);
                }

                foreach (Node boundaryNode in boundaryVertices)
                {
                    List<Node> adjBoundaryNodes = _graph.GetAdjacentNodes(boundaryNode)
                                                 .ToList();

                    Console.WriteLine($"Thread {threadId} is coloring boundary node {boundaryNode} with adjacent nodes: {string.Join(", ", adjBoundaryNodes)}");

                    Node[] nodesToLock = adjBoundaryNodes.Append(boundaryNode).ToArray();
                    LockNodes(nodesToLock);

                    try
                    {
                        boundaryNode.Color = GetMinimumLegalColor(boundaryNode);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        UnlockNodes(nodesToLock);
                    }
                }
            });
        }

        private List<HashSet<Node>> PartitionGraph(int numPartitions)
        {
            List<HashSet<Node>> partitions = new List<HashSet<Node>>();
            int partitionSize = (int)Math.Ceiling((double)_graph.Nodes.Count / numPartitions);

            List<Node> nodes = _graph.Nodes.OrderBy(n => n.Id).ToList();
            for (int i = 0; i < numPartitions; i++)
            {
                partitions.Add(new HashSet<Node>(nodes.Skip(i * partitionSize).Take(partitionSize)));
            }

            return partitions;
        }

        private HashSet<Node> IdentifyBoundaryVertices(HashSet<Node> partition)
        {
            HashSet<Node> boundaryVertices = new HashSet<Node>();

            foreach (Node node in partition)
            {
                if (_graph.GetAdjacentNodes(node).Any(neighbor => !partition.Contains(neighbor)))
                {
                    boundaryVertices.Add(node);
                }
            }

            return boundaryVertices;
        }

        private int GetMinimumLegalColor(Node node)
        {
            HashSet<int> usedColors = _graph.GetAdjacentNodes(node)
                                   .Where(n => n.Color.HasValue)
                                   .Select(n => n.Color.Value)
                                   .ToHashSet();

            for (int color = 1; color <= _nColors; color++)
            {
                if (!usedColors.Contains(color))
                {
                    return color;
                }
            }

            throw new InvalidOperationException("No legal color available. Increase the number of colors.");
        }

        private void LockNodes(params Node[] nodes)
        {
            Node[] orderedNodes = nodes.OrderBy(n => n.Id).ToArray();

            foreach (Node node in orderedNodes)
            {
                node.Lock();
            }
        }

        private void UnlockNodes(params Node[] nodes)
        {
            Node[] reverseOrderedNodes = nodes.OrderByDescending(n => n.Id).ToArray();

            foreach(Node node in reverseOrderedNodes)
            {
                node.Unlock();
            }
        }
    }
}