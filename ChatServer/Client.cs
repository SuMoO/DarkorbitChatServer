using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Client
    {
        private TcpClient m_tcpClient;
        private bool m_running;
        public User User { get; private set; }

        public Client(TcpClient client)
        {
            m_running = true;
            m_tcpClient = client;
        }

        public void Disconnect()
        {
            m_running = false;
        }

        protected virtual void RaiseEvent<T>(EventHandler<T> handler, T args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
