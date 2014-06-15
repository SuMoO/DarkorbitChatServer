using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ChatServer.NetworkEventArgs;

namespace ChatServer
{
    public class Server
    {
        private readonly Dictionary<string, Client> m_clients;
        private bool m_running;
        private Thread m_acceptThread;
        private TcpListener m_listener;

        public event EventHandler<ErrorEventArgs> OnClientError;
        public event EventHandler<MessageRecievedEventArgs> OnMessage;
        public event EventHandler<UserJoinedEventArgs> OnUserJoin;
        public event EventHandler<ServerErrorEventArgs> OnServerError;

        public Dictionary<string, Client> Users { get { return m_clients; } } 

        public Server()
        {
            m_clients = new Dictionary<string, Client>();
        }

        public void Start()
        {
            m_listener = new TcpListener(IPAddress.Any, 9338);
            m_running = true;
            m_acceptThread = new Thread(Accept);
            m_acceptThread.Start();
        }

        public void Stop()
        {
            m_running = false;
            try
            {
                m_acceptThread.Abort();
            }
            catch (Exception ex)
            {
                RaiseEvent(OnServerError,
                    new ServerErrorEventArgs {Error = ex, Message = "Error while trying to stop the server."});
            }
        }

        public bool IsBanned(Client c)
        {
            //TODO IMPLEMENT DATABASE
            return false;
        }

        private void Accept()
        {
            while (m_running)
            {
                try
                {
                    var accpetedClient = m_listener.AcceptTcpClient();
                    var user = new Client(accpetedClient, this);
                    AddHandlers(user);
                    user.Start();
                }
                catch (Exception ex)
                {
                    RaiseEvent(OnServerError, new ServerErrorEventArgs{Error = ex, Message = "Error while acepting new client"});
                }
            }
        }

        private void AddHandlers(Client client)
        {
            client.OnError += client_OnError;
            client.OnMessage += client_OnMessage;
            client.OnUserJoin += client_OnUserJoin;
        }

        protected virtual void RaiseEvent<T>(EventHandler<T> handler, T args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        void client_OnUserJoin(object sender, UserJoinedEventArgs e)
        {
            RaiseEvent(OnUserJoin, e);
        }

        void client_OnMessage(object sender, MessageRecievedEventArgs e)
        {
            RaiseEvent(OnMessage, e);
        }

        void client_OnError(object sender, ErrorEventArgs e)
        {
            RaiseEvent(OnClientError, e);
        }
    }
}
