using MPI;
using PDP_lab_8.Messages;
using System.Collections.Concurrent;

namespace PDP_lab_8
{
    [Serializable]
    public class CloseMessage : Message { }

    public class DSM
    {
        private readonly object _lock = new object();
        public ConcurrentDictionary<string, HashSet<int>> Subscribers { get; private set; }
        public int Var1 { get; private set; } = 0;
        public int Var2 { get; private set; } = 1;
        public int Var3 { get; private set; } = 2;

        public DSM()
        {
            Subscribers = new ConcurrentDictionary<string, HashSet<int>>();
            Subscribers["var1"] = new HashSet<int>();
            Subscribers["var2"] = new HashSet<int>();
            Subscribers["var3"] = new HashSet<int>();
        }

        public void UpdateVariable(string var, int value)
        {
            lock (_lock)
            {
                SetVariable(var, value);
                var message = new UpdateMessage(var, value);
                SendToSubscribers(var, message);
            }
        }

        public void SetVariable(string var, int value)
        {
            switch (var)
            {
                case "var1":
                    Var1 = value;
                    break;
                case "var2":
                    Var2 = value;
                    break;
                case "var3":
                    Var3 = value;
                    break;
            }
        }

        public void CheckAndReplace(string var, int oldValue, int newValue)
        {
            lock (_lock)
            {
                if ((var == "var1" && Var1 == oldValue) ||
                    (var == "var2" && Var2 == oldValue) ||
                    (var == "var3" && Var3 == oldValue))
                {
                    UpdateVariable(var, newValue);
                }
            }
        }

        public void SubscribeTo(string var, Intracommunicator comm)
        {
            Subscribers[var].Add(comm.Rank);
            var message = new SubscribeMessage(var, comm.Rank);
            SendAll(message, comm);
        }

        public void SyncSubscription(string var, int rank)
        {
            Subscribers[var].Add(rank);
        }

        public void SendToSubscribers(string var, Message message)
        {
            foreach (var rank in Subscribers[var])
            {
                if (rank != Communicator.world.Rank)
                {
                    Communicator.world.Send(message, rank, 0);
                }
            }
        }

        public void SendAll(Message message, Intracommunicator comm)
        {
            for (int i = 0; i < comm.Size; i++)
            {
                if (i != comm.Rank || message is CloseMessage)
                {
                    comm.Send(message, i, 0);
                }
            }
        }

        public void Close(Intracommunicator comm)
        {
            SendAll(new CloseMessage(), comm);
        }

        public static void WriteAll(DSM dsm, Intracommunicator comm)
        {
            Console.WriteLine($"Write all\nRank {comm.Rank} -> var1 = {dsm.Var1}, var2 = {dsm.Var2}, var3 = {dsm.Var3}");
            Console.WriteLine("Subscribers:");
            foreach (var kvp in dsm.Subscribers)
            {
                Console.WriteLine($"{kvp.Key} -> [{string.Join(", ", kvp.Value)}]");
            }
        }
    }
}