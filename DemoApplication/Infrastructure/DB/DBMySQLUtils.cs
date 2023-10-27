using System;
using MySqlConnector;

namespace DemoApplication.Infrastructure.DB;

public class DBMySQLUtils
{
    public static MySqlConnection
        GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            string connString = "Server=" + host + ";Database=" + database
                                + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }
}