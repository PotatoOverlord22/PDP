﻿using MPI;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace GraphColoring
{
    public class GraphColoringBL
    {
        private const int PARTITION_TAG = 0;
        private const int REQUEST_TAG = 1;
        private const int SEND_TAG = 2;
        private const int END_TAG = 3;
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
                if (!node.Color.HasValue)
                {
                    return false;
                }

                if (!_graph.GetAdjacentNodes(node).All(neighbor => node.Color != neighbor.Color))
                {
                    return false;
                }
            }

            return true;
        }

        public void MPIGraphColoring(ref string[] cmdArgs)
        {
            using (new MPI.Environment(ref cmdArgs))
            {
                Intracommunicator comm = Communicator.world;
                int rank = comm.Rank;
                int size = comm.Size;

                HashSet<Node> localPartition = null;

                if (rank == 0)
                {
                    List<HashSet<Node>> partitions = PartitionGraph(size - 1);

                    for (int i = 1; i < size; i++)
                    {
                        comm.Send<HashSet<Node>>(partitions[i - 1], i, PARTITION_TAG);
                    }

                    Listen(comm);

                    PrintGraph();
                    Console.WriteLine($"\nGraph is colored correctly: {IsValidColoredGraph()}");

                    return;
                }

                comm.Receive<HashSet<Node>>(0, 0, out localPartition);
                HashSet<Node> boundaryVertices = IdentifyBoundaryVertices(localPartition);

                Console.WriteLine($"{rank}: Coloring: {string.Join(", ", localPartition)}.");

                foreach (Node node in localPartition.Except(boundaryVertices))
                {
                    node.Color = GetMinimumLegalColor(node);
                }

                SendNodeUpdates(comm, localPartition.Except(boundaryVertices).ToArray());

                foreach (Node boundaryNode in boundaryVertices)
                {
                    List<Node> adjBoundaryNodes = _graph.GetAdjacentNodes(boundaryNode)
                                                .Where(n => !localPartition.Contains(n)).ToList();

                    Node[] nodesToSync = adjBoundaryNodes.Append(boundaryNode).ToArray();
                    RequestNodeUpdates(comm, nodesToSync);

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
                        SendNodeUpdates(comm, boundaryNode);
                    }
                }

                Console.WriteLine($"{rank}: FINISHED COLORING.\n");
                comm.Send<int>(END_TAG, 0, END_TAG);
            }
        }

        public void ThreadedGraphColoring(int numThreads)
        {
            List<HashSet<Node>> partitions = PartitionGraph(numThreads);

            Parallel.For(0, numThreads, threadId =>
            {
                HashSet<Node> partition = partitions[threadId];
                HashSet<Node> boundaryVertices = IdentifyBoundaryVertices(partition);

                foreach (var node in partition.Except(boundaryVertices))
                {
                    node.Color = GetMinimumLegalColor(node);
                }

                foreach (Node boundaryNode in boundaryVertices)
                {
                    List<Node> adjBoundaryNodes = _graph.GetAdjacentNodes(boundaryNode)
                                                .Where(n => !partition.Contains(n)).ToList();

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

            foreach (Node node in reverseOrderedNodes)
            {
                node.Unlock();
            }
        }

        private void RequestNodeUpdates(Intracommunicator comm, params Node[] nodesToSync)
        {
            Console.WriteLine($"{comm.Rank}: Requesting node updates.");
            comm.Send<MPISynchronizationMessage>(new MPISynchronizationMessage
            {
                Nodes = nodesToSync,
                ProcessId = comm.Rank,
            }, 0, REQUEST_TAG);

            comm.Receive<Node[]>(0, REQUEST_TAG, out Node[] receivedNodes);
            foreach (Node updatedNode in receivedNodes)
            {
                Node localNode = _graph.GetNode(updatedNode);
                localNode.Color = updatedNode.Color;
            }
        }

        private void SendNodeUpdates(Intracommunicator comm, params Node[] nodesToSync)
        {
            Console.WriteLine($"{comm.Rank}: Sending node updates.");
            comm.Send<MPISynchronizationMessage>(new MPISynchronizationMessage
            {
                Nodes = nodesToSync,
                ProcessId = comm.Rank,
            }, 0, SEND_TAG);
        }

        private void Listen(Communicator comm)
        {
            int completedProcesses = 0;
            bool isRunning = true;

            while (isRunning)
            {
                Status status = comm.Probe(Communicator.anySource, Communicator.anyTag);

                switch (status.Tag)
                {
                    case END_TAG:
                        int endMessage;
                        comm.Receive<int>(status.Source, END_TAG, out endMessage);
                        if (endMessage == END_TAG)
                        {
                            completedProcesses++;
                            if (completedProcesses >= comm.Size - 1)
                            {
                                isRunning = false;
                            }
                        }
                        break;

                    case REQUEST_TAG:
                        comm.Receive<MPISynchronizationMessage>(status.Source, REQUEST_TAG, out MPISynchronizationMessage receivedRequestMessage);
                        Console.WriteLine("0: Received request message from rank " + receivedRequestMessage.ProcessId);

                        List<Node> localNodes = new List<Node>();
                        foreach (Node node in receivedRequestMessage.Nodes)
                        {
                            localNodes.Add(_graph.GetNode(node));
                        }

                        comm.Send<Node[]>(localNodes.ToArray(), receivedRequestMessage.ProcessId, REQUEST_TAG);
                        break;

                    case SEND_TAG:
                        comm.Receive<MPISynchronizationMessage>(status.Source, SEND_TAG, out MPISynchronizationMessage receivedMessage);
                        /* Console.WriteLine($"0: Got send message from rank {receivedMessage.ProcessId}, COLORED: {string.Join(", ", receivedMessage.Nodes.ToList())}");*/
                        Console.WriteLine($"0: Got the following colors from {receivedMessage.ProcessId}: " + string.Join(", ", receivedMessage.Nodes.Select(n => $"Node {n.Id}: {n.Color}")));


                        foreach (Node node in receivedMessage.Nodes)
                        {
                            _graph.GetNode(node).Color = node.Color;
                        }
                        break;
                }
            }
        }
    }
}