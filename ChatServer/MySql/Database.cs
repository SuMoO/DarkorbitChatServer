﻿using MySql.Data.MySqlClient;

namespace ChatServer.MySql
{
    public class Database
    {
        protected string ConnectionString;

        public void TryConnect(string server, string user, string pw, string database)
        {
            var tmp = string.Format("Server={0};Uid={1};Pwd={2};Database={3};", server, user, pw, database);
            using (var con = new MySqlConnection(tmp))
                con.Open();
            ConnectionString = tmp;
        }

        public MySqlConnection GetConnection()
        {
            var con = new MySqlConnection(ConnectionString);
            con.Open();
            return con;
        }

        public MySqlCommand BuildQuery(MySqlConnection con, string query, params object[] values)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = query;
            for (var i = 0; i < values.Length; i += 2)
                cmd.Parameters.AddWithValue(values[i].ToString(), values[(i + 1)]);
            return cmd;
        }
    }
}
