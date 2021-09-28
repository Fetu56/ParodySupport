using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        Socket socket;
        IPEndPoint iPEndPoint;
        public Task process { get; private set; }
        public Client()
        {
            iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public Client(string ip, int port)
        {
            try
            {
                iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception)
            {
                iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Start()
        {
            process = new Task(ClientStartThread);
            process.Start();
        }

        private void ClientStartThread()
        {
            try
            {
                socket.Connect(iPEndPoint);
                Console.WriteLine($"Welcome to server[{iPEndPoint.Address}]!");
                while (socket.Connected)
                {
                    string get = GetString();
                    if (get.StartsWith("-"))
                    {
                        try
                        {
                            switch (get.Split(' ')[0])
                            {
                                case "--start":
                                    string args = "";
                                    get.Split(' ').ToList().Skip(3).ToList().ForEach(x => args += " " + x);
                                    if (args != "")
                                    {
                                        Console.WriteLine(get.Replace(get.Split(' ')[0], ""));
                                        Process.Start(get.Replace(get.Split(' ')[0], "").Replace(args, ""), args);
                                    }
                                    else
                                    {
                                        Process.Start(get.Replace(get.Split(' ')[0], ""));
                                    }
                                    break;
                                case "-mouse":
                                    Cursor.SetCursorPos(int.Parse(get.Split(' ')[1]), int.Parse(get.Split(' ')[2]));
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                    else if(get.StartsWith("→disconnect@"))
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine(get);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }
        private string GetString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = socket.Receive(data);
                stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (socket.Available > 0);
            return stringBuilder.ToString();
        }

    }
}