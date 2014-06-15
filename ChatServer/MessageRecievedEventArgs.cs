using System;

namespace ChatServer
{
    public class MessageRecievedEventArgs : EventArgs
    {
        public User Sender { get; set; }

        public string Message { get; set; }
    }
}
