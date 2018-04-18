using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EventLibrary
{
    public class Transaction
    {
        //methods
        public static void Add(string info, string amount, int id)      //add data to TRANSACTION table
        {
            MySqlConnection connection = new MySqlConnection(Connection.CONNECTIONINFO);
            connection.Open();
            string date = DateTime.Now.ToShortDateString();
            string sql = "INSERT INTO TRANSACTION " +
                         "(Info, Amount, Date, Identity_Nr) " +
                         "VALUES (" + "'" + info + "', " + amount + ", " + "'" + date + "' " + ", " + id + ");";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}
