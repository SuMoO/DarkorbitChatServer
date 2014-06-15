using System;

namespace ChatServer
{
    public class UserJoinedEventArgs : EventArgs
    {
        public User User { get; set; }
    
    }
}
