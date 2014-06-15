using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using ChatServer.NetworkEventArgs;

namespace ChatServer
{
    public class Client
    {
        private TcpClient m_tcpClient;
        private bool m_running;
        private Thread m_recieveThread;
        private Thread m_handlePacketThread;
        private Queue<string> m_packetQueue;
        private Server m_server;
        public User User { get; private set; }

        #region Wrapper For User
        public string UserId {
            get { return User.UserId; }
            set { User.UserId = value; }
        }

        public string SessionId
        {
            get { return User.SessionId; }
            set { User.SessionId = value; }
        }

        public string Name
        {
            get { return User.Name; }
            set { User.Name = value; }
        }

        public string ClanTag
        {
            get { return User.ClanTag; }
            set { User.ClanTag = value; }
        }
        #endregion

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageRecievedEventArgs> OnMessage;
        public event EventHandler<UserJoinedEventArgs> OnUserJoin;

        public Client(TcpClient client, Server s)
        {
            m_tcpClient = client;
            m_server = s;
            m_packetQueue = new Queue<string>();
        }

        public void Start()
        {
            m_running = true;

            m_handlePacketThread = new Thread(HandlePackets) {IsBackground = true};
            m_handlePacketThread.Start();

            m_recieveThread = new Thread(Recieve) {IsBackground = true};
            m_recieveThread.Start();
        }

        public void Disconnect()
        {
            m_running = false;
            AbortThread(m_recieveThread);
            AbortThread(m_handlePacketThread);
            SafeShutdown();
            m_packetQueue.Clear();
            m_server = null;
            m_tcpClient = null;
            m_handlePacketThread = null;
            m_packetQueue = null;
            m_recieveThread = null;
        }

        public void RawSend(string data)
        {
            try
            {
                var buffer = Encoding.Default.GetBytes(data);
                m_tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                RaiseEvent(OnError, new ErrorEventArgs{Error = ex, Message = "Error while sending data.", User = this});
            }
        }

        protected virtual void RaiseEvent<T>(EventHandler<T> handler, T args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void AbortThread(Thread t)
        {
            try
            {
                t.Abort();
            }
            catch (Exception ex)
            {
                RaiseEvent(OnError, new ErrorEventArgs{Error = ex, Message = "Error while aborting thread", User = this});
            }
        }

        protected virtual void SafeShutdown()
        {
            try
            {
                if (m_tcpClient != null && m_tcpClient.Connected)
                {
                    m_tcpClient.Close();
                }
            }
            catch (Exception ex)
            {
                RaiseEvent(OnError, new ErrorEventArgs{Error = ex, Message = "Error while shutting down the connection.", User = this});
            }
        }

        private void HandlePackets()
        {
            while (m_running)
            {
                try
                {
                    if (m_packetQueue.Count == 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var packet = m_packetQueue.Dequeue();

                    if (packet.StartsWith("<policy-file-request/>"))
                    {
                        RawSend(
                            "<?xml version=\"1.0\"?><cross-domain-policy xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"http://www.adobe.com/xml/schemas/PolicyFileSocket.xsd\"><allow-access-from domain=\"*\" to-ports=\"*\" secure=\"false\" /><site-control permitted-cross-domain-policies=\"master-only\" /></cross-domain-policy>");
                    }
                    else if (packet.StartsWith("bu"))
                    {
                        // Login packet
                        User = new User();

                        var parameters = packet.Replace("@", "%").Split('%');

                        ClanTag = parameters[7];
                        Name = parameters[2];
                        SessionId = parameters[4];
                        UserId = parameters[3];

                        if (m_server.IsBanned(this))
                        {
                            Disconnect();
                            return;
                        }

                        // If there is already an client with the same user id, disconnect the other one.
                        Client value;
                        if (m_server.Users.TryGetValue(UserId, out value))
                        {
                            value.Disconnect();
                            m_server.Users.Remove(UserId);
                        }

                        m_server.Users.Add(UserId, this);
                        // Rest will come soon when i finished database...
                    }
                }
                catch (Exception ex)
                {
                    RaiseEvent(OnError, new ErrorEventArgs{Error = ex, Message = "Error while handling packet.", User = this});
                }
            }
        }

        private void Recieve()
        {
            while (m_running)
            {
                try
                {
                    var buffer = new byte[2048];

                    var bytesRead = m_tcpClient.GetStream().Read(buffer, 0, buffer.Length);

                    // If there was actual data being send
                    if (bytesRead > 0)
                    {
                        Array.Resize(ref buffer, bytesRead);

                        var packet = Encoding.Default.GetString(buffer);

                        // If the string is not null not empty and not whitespace
                        if (!string.IsNullOrEmpty(packet) && packet.Trim() != "")
                        {
                            m_packetQueue.Enqueue(packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseEvent(OnError, new ErrorEventArgs{Error = ex, Message = "Error while recieving data", User = this});
                }
            }
        }
    }
}
