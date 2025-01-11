using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class HamiltonianCycle
{
    private int[,] graph;
    private readonly int numVertices;
    private readonly int startVertex;
    private readonly object lockObj = new();
    private bool cycleFound = false;

    public int[,] Graph
    {
        set { graph = value; }
        get { return graph; }
    }

    public HamiltonianCycle(int[,] graph, int startVertex)
    {
        this.graph = graph;
        this.numVertices = graph.GetLength(0);
        this.startVertex = startVertex;
    }

    public List<int> FindHamiltonianCycleParallel()
    {
        cycleFound = false;
        var path = new List<int> { startVertex };

        var result = ParallelSearch(path);
        return result ?? null;
    }

    public List<int> FindHamiltonianCycleNonParallel()
    {
        cycleFound = false;
        var path = new List<int> { startVertex };

        var result = NonParallelSearch(path);
        return result ?? null;
    }


    private List<int> ParallelSearch(List<int> path)
    {
        if (cycleFound)
        {
            return null;
        }

        int currentVertex = path.Last();

        if (path.Count == numVertices && graph[currentVertex, startVertex] == 1)
        {
            lock (lockObj)
            {
                if (!cycleFound)
                {
                    path.Add(startVertex);
                    cycleFound = true;
                    return path;
                }
            }

            return null;
        }

        var tasks = new List<Task<List<int>>>();

        for (int nextVertex = 0; nextVertex < numVertices; nextVertex++)
        {
            if (cycleFound)
            {
                break;
            }

            if (graph[currentVertex, nextVertex] == 1 && !path.Contains(nextVertex))
            {
                var newPath = new List<int>(path) { nextVertex };

                tasks.Add(Task.Run(() => ParallelSearch(newPath)));
            }
        }

        foreach (var task in tasks)
        {
            var result = task.Result;
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }


    private List<int> NonParallelSearch(List<int> path)
    {
        if (cycleFound)
        {
            return null;
        }

        int currentVertex = path.Last();

        if (path.Count == numVertices && graph[currentVertex, startVertex] == 1)
        {
            path.Add(startVertex);
            cycleFound = true;
            return path;
        }

        for (int nextVertex = 0; nextVertex < numVertices; nextVertex++)
        {
            if (cycleFound)
            {
                break;
            }

            if (graph[currentVertex, nextVertex] == 1 && !path.Contains(nextVertex))
            {
                var newPath = new List<int>(path) { nextVertex };
                var result = NonParallelSearch(newPath);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return null;
    }
}