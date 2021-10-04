using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Client
    {
        public Socket socket { get; private set; }
        public Client(Socket socketg)
        {
            socket = socketg;
        }
        public void Disconnect()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
        public void Send(string msg)
        {
            socket.Send(Encoding.Unicode.GetBytes(msg));
        }
        public override string ToString()
        {
            return $"Client with ip {socket.RemoteEndPoint} is " + (socket.Connected ? "connected" : "not connected");
        }
    }
}
