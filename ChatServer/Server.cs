using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    public class Server
    {
        private List<Client> m_clients;
        private bool m_running;
        private Thread m_acceptThread;
        private TcpListener m_listener;

        public Server()
        {
            m_clients = new List<Client>();
        }

        public void Start()
        {
            m_listener = new TcpListener(IPAddress.Any, 9338);
            m_running = true;
            m_acceptThread = new Thread(Accept);
        }

        private void Accept()
        {
            while (m_running)
            {
                var accpetedClient = m_listener.AcceptTcpClient();
                var user = new Client(accpetedClient);
                m_clients.Add(user);
            }
        }
    }
}
