using System;

namespace ChatServer.NetworkEventArgs
{
    public class ErrorEventArgs : EventArgs
    {
        public Client User { get; set; }

        public string Message { get; set; }

        public Exception Error { get; set; }
    }
}
