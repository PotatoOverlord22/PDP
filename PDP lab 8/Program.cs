using MPI;

namespace PDP_lab_8
{
    class Program
    {
        // should start with 3 processes
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                var comm = Communicator.world;
                var dsm = new DSM();

                if (comm.Size != 3)
                {
                    throw new Exception("This program should be run with 3 processes");
                }

                if (comm.Rank == 0)
                {
                    var listener = new Listener(dsm, comm);
                    var listenerThread = new Thread(listener.Run);
                    listenerThread.Start();

                    dsm.SubscribeTo("var1", comm);
                    dsm.SubscribeTo("var2", comm);
                    dsm.SubscribeTo("var3", comm);
                    dsm.CheckAndReplace("var1", 0, 123);
                    dsm.CheckAndReplace("var2", 2, 321);
                    dsm.CheckAndReplace("var3", 100, 9);
                    dsm.Close(comm);

                    listenerThread.Join();
                }
                else if (comm.Rank == 1)
                {
                    var listener = new Listener(dsm, comm);
                    var listenerThread = new Thread(listener.Run);
                    listenerThread.Start();

                    dsm.SubscribeTo("var", comm);
                    dsm.SubscribeTo("var3", comm);

                    listenerThread.Join();
                }
                else if (comm.Rank == 2)
                {
                    var listener = new Listener(dsm, comm);
                    var listenerThread = new Thread(listener.Run);
                    listenerThread.Start();

                    dsm.SubscribeTo("var2", comm);
                    dsm.CheckAndReplace("var2", 1, 100);

                    listenerThread.Join();
                }
            }
        }
    }
}