using MPI;
using System;

class Program
{
    const int SYNC_TAG = 100; // Example tag for synchronization

    static void Main(string[] args)
    {
        using (new MPI.Environment(ref args))
        {
            Intracommunicator communicator = Communicator.world;

            if (communicator.Rank == 0)
            {

                ReceiveRequest status = communicator.ImmediateReceive<string>(Communicator.anySource, SYNC_TAG);

                Console.WriteLine($"Message received from Rank {status.GetValue()}");
            }
            else
            {
                string message = $"Hello from Ra2222nk {communicator.Rank}";
                communicator.Send(message, 0, SYNC_TAG);
            }
        }
    }
}
