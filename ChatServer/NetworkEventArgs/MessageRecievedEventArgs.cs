using System;

namespace ChatServer.NetworkEventArgs
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public Client Sender { get; set; }

        public string Message { get; set; }
    }
}
