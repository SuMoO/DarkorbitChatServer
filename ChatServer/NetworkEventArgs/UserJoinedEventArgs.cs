using System;

namespace ChatServer.NetworkEventArgs
{
    public class UserJoinedEventArgs : EventArgs
    {
        public Client User { get; set; }
    
    }
}
