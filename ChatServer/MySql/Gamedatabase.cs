namespace ChatServer.MySql
{
    class Gamedatabase : Database
    {
        public User GetUserData(User user)
        {
            using (var con = GetConnection())
            {
                using (var cmd = BuildQuery(con,
                    "SELECT * from accounts WHERE accounts.id=@USERID AND accounts.sessionid=@SID"
                    , "@USERID", user.UserId, "@SID", user.SessionId))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return null;

                        var retVal = new User
                        {
                            UserId = r.GetString("id"),
                            SessionId = r.GetString("sessionid"),
                            Name = r.GetString("username"),
                            ClanTag = r.GetString("clanTag")
                        };

                        return retVal;
                    }
                }
            }
        }

        public bool Banned(User user)
        {
            using (var con = GetConnection())
            {
                using (var cmd = BuildQuery(con,
                    "SELECT * from accounts WHERE accounts.id=@USERID AND accounts.sessionid=@SID"
                    , "@USERID", user.UserId, "@SID", user.SessionId))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return false;

                        return r.GetBoolean("banned");
                    }
                }
            }
        }

        public bool Admin(User user)
        {
            using (var con = GetConnection())
            {
                using (var cmd = BuildQuery(con,
                    "SELECT * from accounts WHERE accounts.id=@USERID AND accounts.sessionid=@SID"
                    , "@USERID", user.UserId, "@SID", user.SessionId))
                {
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return false;

                        return r.GetInt32("rank") == 21;
                    }
                }
            }
        }
    }
}
