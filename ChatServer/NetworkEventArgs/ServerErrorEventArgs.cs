using System;

namespace ChatServer.NetworkEventArgs
{
    public class ServerErrorEventArgs : EventArgs
    {
        public Exception Error { get; set; }

        public string Message { get; set; }
    }
}
