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
                    Console.WriteLine("Выберите действие:\n[1] - запуск программы\n[2] - отключение\n[3] - смена данных в реестре\n[0] - выход:");
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
                        case "3":
                            option = Option.changeReg;
                            break;
                        case "0":
                            option = Option.exit;
                            break;
                        default:
                            option = Option.nullOp;
                            break;
                    }    
                    if (option == Option.exit)
                    {
                        work = false;
                        Environment.Exit(0);
                        break;
                    }
                    else if(option != Option.nullOp)
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
                                    if (get.StartsWith("!"))
                                    {
                                        Console.WriteLine("Введите адресс приложения и аргументы к его запуску:");
                                        SendMsgToAll("--start "+Console.ReadLine());
                                    }
                                    else
                                    {
                                        int id = int.Parse(get);
                                        clients[id].Send("-getapps");
                                        {
                                            Console.WriteLine("Программы на компьютере пользователя:"+GetString(clients[id].socket));
                                            string getRes = Console.ReadLine();
                                            int tmp;
                                            while (!int.TryParse(getRes, out tmp))
                                            {
                                                getRes = Console.ReadLine();
                                            }
                                            clients[id].Send(getRes);
                                            string getR = GetString(clients[id].socket);
                                            if (getR != "Error")
                                            {
                                                Console.WriteLine("Список запускаемых файлов:"+ getR);
                                                do
                                                {
                                                    getRes = Console.ReadLine();
                                                } while (!int.TryParse(getRes, out tmp));
                                                clients[id].Send(getRes);
                                                Console.WriteLine("Программа запущена.");
                                            }
                                            else { Console.WriteLine("Error"); }
                                        }
                                        
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
                                case Option.changeReg:
                                    Console.WriteLine("Введите параметр, который хотите сменить и его значение\n[1] - ip\n[2] - порт\n");
                                    string req = "";
                                    switch(Console.ReadLine())
                                    {
                                        case "1":
                                            req = "ip";
                                            break;
                                        case "2":
                                            req = "port";
                                            break;
                                    }
                                    string data = Console.ReadLine();
                                    if(data.Length == 0 || (data.Length > 15 && req.StartsWith("ip")) || (data.Length > 4 && req.StartsWith("port")) || req == "")
                                    {
                                        Console.WriteLine("Ошибка с вводимыми данными.");
                                    }
                                    else
                                    {
                                        if (get.StartsWith("!"))
                                        {
                                            SendMsgToAll($"-reg {req} {data}");
                                        }
                                        else
                                        {
                                            int id = int.Parse(get);
                                            clients[id].Send($"-reg {req} {data}");
                                        }
                                        Console.WriteLine("Запрос отправлен.");
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
        private string GetString(Socket sc)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = sc.Receive(data);
                stringBuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (sc.Available > 0);
            return stringBuilder.ToString();
        }
        private string GetCords()
        {
            Console.WriteLine("Введите координаты в формате \"x y\":");
            int cordX = 0;
            int cordY = 0;
            string get = Console.ReadLine();
            while(!int.TryParse(get.Split(' ')[0], out cordX) || !int.TryParse(get.Split(' ')[1], out cordY))
            {
                get = Console.ReadLine();
            }
            return cordX + " " + cordY;
        }
    }
}