using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        public User User { get; private set; }

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<MessageRecievedEventArgs> OnMessage;
        public event EventHandler<UserJoinedEventArgs> OnUserJoin;

        public Client(TcpClient client)
        {
            m_running = true;
            m_tcpClient = client;
        }

        public void Start()
        {
            
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

        private void Send(string data)
        {
            try
            {
                var buffer = Encoding.Default.GetBytes(data);
                m_tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                
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

                    if (bytesRead > 0)
                    {
                        Array.Resize(ref buffer, bytesRead);

                        var packet = Encoding.Default.GetString(buffer);

                        m_packetQueue.Enqueue(packet);
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
