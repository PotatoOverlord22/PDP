using System.Net;
using System.Net.Sockets;
using System.Text;

namespace lab4_2
{
    public static class Program
    {
        public static void Main()
        {
            var pages = new List<string> { "documente-de-infiintare/", "documente-utile/" };
            var tasks = new List<Task>();

            foreach (var page in pages)
            {
                tasks.Add(LoadWebsitePage(page));
            }

            Task.WhenAll(tasks).Wait();
            Console.WriteLine("All pages loaded!");
        }

        public static Task LoadWebsitePage(string path)
        {
            var tcs = new TaskCompletionSource();
            var entry = Dns.GetHostEntry(State.Host);
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(entry.AddressList[0], State.Port);
            var state = new State(socket);
            ConnectTask(state, endpoint).ContinueWith(connectTask =>
            {
                if (connectTask.IsFaulted)
                {
                    Console.WriteLine($"Connection failed: {connectTask.Exception?.Message}");
                    tcs.SetException(connectTask.Exception ?? new Exception("Unknown error"));
                    return;
                }

                Console.WriteLine("Connected successfully!");

                SendTask(state, $"GET /{path} HTTP/1.1\r\nHost: {State.Host}\r\n\r\n").ContinueWith(sendTask =>
                {
                    if (sendTask.IsFaulted)
                    {
                        Console.WriteLine($"Send failed: {sendTask.Exception?.Message}");
                        tcs.SetException(sendTask.Exception ?? new Exception("Unknown error"));
                        return;
                    }

                    var sizeOfDataSent = sendTask.Result;
                    Console.WriteLine($"Data sent: {sizeOfDataSent} bytes");

                    ReceiveTask(state).ContinueWith(receiveTask =>
                    {
                        if (receiveTask.IsFaulted)
                        {
                            Console.WriteLine($"Receive failed: {receiveTask.Exception?.Message}");
                            tcs.SetException(receiveTask.Exception ?? new Exception("Unknown error"));
                            return;
                        }

                        Console.WriteLine($"Page loaded successfully!");
                        tcs.SetResult();
                    });
                });
            });
            return tcs.Task;
        }

        private static Task<State> ConnectTask(State state, IPEndPoint endpoint)
        {
            var tcs = new TaskCompletionSource<State>();
            var socket = state.Socket;
            socket.BeginConnect(endpoint, ar =>
            {
                try
                {
                    socket.EndConnect(ar);
                    tcs.SetResult(state);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        private static Task<int> SendTask(State state, string requestText)
        {
            var tcs = new TaskCompletionSource<int>();
            var socket = state.Socket;
            var requestBytes = Encoding.UTF8.GetBytes(requestText);
            socket.BeginSend(requestBytes, 0, requestBytes.Length, SocketFlags.None, ar =>
            {
                try
                {
                    tcs.SetResult(socket.EndSend(ar));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        private static Task<int> ReceiveTask(State state)
        {
            var tcs = new TaskCompletionSource<int>();
            ReceiveNext(state, tcs);
            return tcs.Task;
        }
        private static void ReceiveNext(State state, TaskCompletionSource<int> tcs)
        {
            var socket = state.Socket;
            socket.BeginReceive(state.Buffer, 0, State.BufferLength, SocketFlags.None, ar =>
            {
                try
                {
                    var bytesReceived = socket.EndReceive(ar);
                    Console.WriteLine($"Received {bytesReceived} bytes");
                    if (bytesReceived == 0)
                    {
                        Console.WriteLine(state.Content.ToString());
                        tcs.SetResult(bytesReceived);
                    }
                    else
                    {
                        var responseText = Encoding.UTF8.GetString(state.Buffer, 0, bytesReceived);
                        state.Content.Append(responseText);
                        Task.Run(() => ReceiveNext(state, tcs));
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
        }
        public sealed class State
        {
            public const string Host = "www.cnatdcu.ro";
            public const int Port = 80;
            public const int BufferLength = 1024;
            public readonly byte[] Buffer = new byte[BufferLength];
            public readonly StringBuilder Content = new StringBuilder();
            public readonly Socket Socket;

            public State(Socket socket)
            {
                Socket = socket;
            }
        }
    }
}