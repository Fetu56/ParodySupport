using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server //меню сервера - запуск по или отключить и список юзера кому
{
    class Server
    {
        Socket socket;
        IPEndPoint iPEndPoint;
        List<Client> clients;
        //List<User> users;
        bool work = true;
        public Server()
        {
            iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<Client>();
        }
        public Server(string ip, int port)
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
            Console.WriteLine($"Start server [{iPEndPoint.Address}]...");
            try
            {
                socket.Bind(iPEndPoint);
                socket.Listen(10);
                Task senderMsg = new Task(msgSender);
                senderMsg.Start();
                while (work)
                {
                    Connected();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            DisconnectAll();

        }
        private void Connected()
        {
            Socket socketClient = socket.Accept();
            //socketClient.Send(Encoding.Unicode.GetBytes("Auth on server or login. Log \"[Login] [Pass]\" or \"Reg [Login] [Pass]\" like \"Reg new 123\":"));
            socketClient.Send(Encoding.Unicode.GetBytes("Connected"));

            clients.Add(new Client(socketClient));
            Console.WriteLine($"Registrated user join. Info: ip - {socketClient.RemoteEndPoint}, protocol {socketClient.ProtocolType}");

            //string get = GetString();
            //if (get.Split("").Length == 3)
            //{
            //    if (get.StartsWith("Reg"))
            //    {
            //        users.Add(User.Registration(get.Split("")[1], get.Split("")[2], socketClient.AddressFamily.ToString()));
            //        clients.Add(socketClient);
            //        Console.WriteLine("Registrated new user.");
            //    }
            //    else if (get.StartsWith("Log"))
            //    {
            //        if (User.HaveAUser(users, get.Split("")[1], get.Split("")[2], socketClient.AddressFamily.ToString()))
            //        {
            //            clients.Add(socketClient);
            //            Console.WriteLine("Registrated user join.");
            //        }
            //    }
            //}

        }
        public void DisconnectAll()
        {
            clients.ForEach(x => x.Disconnect());
            clients.Clear();
        }
        private void msgSender()
        {
            string msg;
            while (true)
            {
                try
                {
                    Console.WriteLine("Выберите действие [1] - запуск программы, [2] - отключение, [0] - выход:");
                    msg = Console.ReadLine();
                    Option option = Option.nullOp;
                    switch(msg)
                    {
                        case "1":
                            option = Option.startProg;
                            break;
                        case "2":
                            option = Option.disconnect;
                            break;
                        case "0":
                            option = Option.exit;
                            break;
                    }    
                    if (option == Option.exit)
                    {
                        work = false;
                        Environment.Exit(0);
                        break;
                    }
                    else
                    {
                        if(clients.Count > 0)
                        {
                            Console.WriteLine("Выберите клиента, к которому применить действие. Для выбора всех введите \"!\".");
                            for (int i = 0; i < clients.Count; i++)
                            {
                                Console.WriteLine(i + ". " + clients[i]);
                            }
                            string get = Console.ReadLine();
                            switch (option)
                            {
                                case Option.startProg:
                                    Console.WriteLine("Введите адресс приложения и аргументы к его запуску:");
                                    if (get.StartsWith("!"))
                                    {
                                        SendMsgToAll("--start "+Console.ReadLine());
                                    }
                                    else
                                    {
                                        int id = int.Parse(get);
                                        clients[id].Send("--start " + Console.ReadLine());
                                    }
                                    
                                    break;
                                case Option.disconnect:
                                    if (get.StartsWith("!"))
                                    {
                                        SendMsgToAll("→disconnect@");
                                        DisconnectAll();
                                    }
                                    else
                                    {
                                        int id = int.Parse(get);
                                        clients[id].Send("→disconnect@");
                                        clients[id].Disconnect();
                                        clients.RemoveAt(id);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Недостаточное количество пользователей.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void SendMsgToAll(string msg)
        {
            clients.ForEach(x => x.Send(msg));
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