using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ChatServer.MySql;
using ChatServer.NetworkEventArgs;

namespace ChatServer
{
    public class Server
    {
        private readonly Dictionary<string, Client> m_clients;
        
        private bool m_running;
        private Thread m_acceptThread;
        private TcpListener m_listener;
        private readonly Gamedatabase m_gamedatabase;

        public event EventHandler<ErrorEventArgs> OnClientError;
        public event EventHandler<MessageRecievedEventArgs> OnMessage;
        public event EventHandler<UserJoinedEventArgs> OnUserJoin;
        public event EventHandler<ServerErrorEventArgs> OnServerError;

        public Dictionary<string, Client> Users { get { return m_clients; } } 

        public Server()
        {
            m_clients = new Dictionary<string, Client>();
            m_gamedatabase = new Gamedatabase();
        }

        public void Start()
        {
            m_gamedatabase.TryConnect("localhost", "root", "", "skyuniverse");
            m_listener = new TcpListener(IPAddress.Any, 9338);
            m_listener.Start();
            m_running = true;
            m_acceptThread = new Thread(Accept);
            m_acceptThread.Start();
        }

        public void Stop()
        {
            m_running = false;
            foreach (var client in m_clients)
            {
                client.Value.Disconnect();
            }
            try
            {
                m_acceptThread.Abort();
            }
            catch (Exception ex)
            {
                RaiseEvent(OnServerError,
                    new ServerErrorEventArgs {Error = ex, Message = "Error while trying to stop the server."});
            }
            m_listener.Stop();
        }

        public bool IsBanned(Client c)
        {
            return m_gamedatabase.Banned(c.User);
        }

        public bool Valid(User user)
        {
            var realData = m_gamedatabase.GetUserData(user);

            if (realData == null)
                return false;

            return user.UserId == realData.UserId &&
                   user.SessionId == realData.SessionId &&
                   user.ClanTag == realData.ClanTag &&
                   user.Name == realData.Name;
        }

        public bool IsAdmin(Client c)
        {
            return m_gamedatabase.Admin(c.User);
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
