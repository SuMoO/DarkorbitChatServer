using System;

namespace ChatServer.NetworkEventArgs
{
    public class PacketEventArgs : EventArgs
    {
        public Client User { get; set; }

        public string Packet { get; set; }
    }
}
