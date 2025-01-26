using MPI;
using PDP_lab_8.Messages;

namespace PDP_lab_8
{
    public class Listener
    {
        private readonly DSM _dsm;
        private readonly Intracommunicator _comm;

        public Listener(DSM dsm, Intracommunicator comm)
        {
            _dsm = dsm;
            _comm = comm;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine($"Rank {_comm.Rank} waiting...");
                var message = _comm.Receive<Message>(Communicator.anySource, 0);

                switch (message)
                {
                    case CloseMessage:
                        Console.WriteLine($"Rank {_comm.Rank} stopped listening...");
                        return;

                    case SubscribeMessage subscribeMessage:
                        Console.WriteLine($"Subscribe message received. Rank {_comm.Rank} received: rank {subscribeMessage.Rank} subscribes to {subscribeMessage.Var}");
                        _dsm.SyncSubscription(subscribeMessage.Var, subscribeMessage.Rank);
                        break;

                    case UpdateMessage updateMessage:
                        Console.WriteLine($"Update message received. Rank {_comm.Rank} received: {updateMessage.Var} -> {updateMessage.Value}");
                        _dsm.SetVariable(updateMessage.Var, updateMessage.Value);
                        break;
                }

                DSM.WriteAll(_dsm, _comm);
            }
        }
    }
}