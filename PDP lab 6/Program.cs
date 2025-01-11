using System.Diagnostics;

namespace PDP_lab_6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[,] graph = {
                { 0, 1, 1, 1, 0, 0, 0, 0, 0, 0 }, 
                { 1, 0, 1, 0, 1, 0, 0, 0, 0, 0 },
                { 1, 1, 0, 1, 0, 1, 0, 0, 0, 0 },
                { 1, 0, 1, 0, 1, 0, 1, 0, 0, 0 },
                { 0, 1, 0, 1, 0, 1, 0, 1, 0, 0 },
                { 0, 0, 1, 0, 1, 0, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 0, 1, 0, 1, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 1, 0, 1, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 1 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 }
            };


            int[,] graphWithCycle = {
                { 0, 1, 0, 0, 1, 0, 0, 0, 0, 1 },
                { 1, 0, 1, 0, 0, 1, 0, 0, 0, 0 },
                { 0, 1, 0, 1, 0, 0, 1, 0, 0, 0 },
                { 0, 0, 1, 0, 1, 0, 0, 1, 0, 0 },
                { 1, 0, 0, 1, 0, 0, 0, 0, 1, 0 },
                { 0, 1, 0, 0, 0, 0, 1, 0, 0, 1 },
                { 0, 0, 1, 0, 0, 1, 0, 1, 0, 0 },
                { 0, 0, 0, 1, 0, 0, 1, 0, 1, 0 },
                { 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 },
                { 1, 0, 0, 0, 0, 1, 0, 0, 1, 0 }
            };


            int startVertex = 0;

            var hamiltonianCycle = new HamiltonianCycle(graph, startVertex);
            
            Console.WriteLine("Graph without Hamiltonian Cycle:");
            MeasureSequencialTime(hamiltonianCycle, graph);
            Console.WriteLine();
            MeasureParallelTime(hamiltonianCycle, graph);

            Console.WriteLine("\nGraph with Hamiltonian Cycle:");
            MeasureSequencialTime(hamiltonianCycle, graphWithCycle);
            Console.WriteLine();
            MeasureParallelTime(hamiltonianCycle, graphWithCycle);
        }

        static void MeasureParallelTime(HamiltonianCycle hamiltonianCycle, int[,] graph)
        {
            var stopwatch = new Stopwatch();
            hamiltonianCycle.Graph = graph;

            stopwatch.Start();
            var parallelResult = hamiltonianCycle.FindHamiltonianCycleParallel();
            stopwatch.Stop();

            if (parallelResult != null)
            {
                Console.WriteLine("Hamiltonian Cycle Found (Parallel):");
                Console.WriteLine(string.Join(" -> ", parallelResult));
            }
            else
            {
                Console.WriteLine("No Hamiltonian Cycle Found (Parallel).");
            }

            Console.WriteLine("Parallel Search Time: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        static void MeasureSequencialTime(HamiltonianCycle hamiltonianCycle, int[,] graph)
        {
            var stopwatch = new Stopwatch();
            hamiltonianCycle.Graph = graph;

            stopwatch.Start();
            var nonParallelResult = hamiltonianCycle.FindHamiltonianCycleNonParallel();
            stopwatch.Stop();

            if (nonParallelResult != null)
            {
                Console.WriteLine("Hamiltonian Cycle Found (Non-Parallel):");
                Console.WriteLine(string.Join(" -> ", nonParallelResult));
            }
            else
            {
                Console.WriteLine("No Hamiltonian Cycle Found (Non-Parallel).");
            }

            Console.WriteLine("Non-Parallel Search Time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }
}
