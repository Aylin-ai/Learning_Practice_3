using MySqlConnector;

namespace DemoApplication.Infrastructure.DB;

public class DBUtils
{
    public static MySqlConnection GetDBConnection()
    {
        string host = "localhost";
        int port = 3306;
        string database = "learning_practice_3";
        string username = "root";
        string password = "546870";

        return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
    }
}