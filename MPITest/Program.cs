using MPI;
using System.Diagnostics;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        /*while (!Debugger.IsAttached)
        {
            Console.WriteLine("Waiting for debugger to attach...");
            Thread.Sleep(2000);
        }*/

        using (new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;
            int rank = comm.Rank;

            if (rank == 0)
            {
                int[] serializedData = { 1, 2, 3, 4, 5 };
                comm.Send<int[]>(serializedData, 1, 0);
            }
            else
            {
                int[] deserializedData = new int[20];
                comm.Receive<int[]>(0, 0, out deserializedData);
                Console.WriteLine(string.Join(", ", deserializedData));
                Console.WriteLine($"length: {deserializedData.Length}");
            }
        }
    }
}
