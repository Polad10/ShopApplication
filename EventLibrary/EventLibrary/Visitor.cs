using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EventLibrary
{
    public class Visitor
    {
        //fields
        static MySqlConnection connection = new MySqlConnection(Connection.CONNECTIONINFO);

        //methods
        public static int GetId(string tagId)
        {
            connection.Open();
            int id = 0;
            string sql = "SELECT Identity_Nr " +
                         "FROM PARTICIPANT " +
                         "WHERE Tag_Id = '" + tagId + "';";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = Convert.ToInt32(reader["Identity_Nr"]);
            }
            reader.Close();
            connection.Close();
            return id;
        }
        public static double GetBalance(int id) 
        {
            connection.Open();
            string sql = "SELECT Balance " +
                         "FROM Participant " +
                         "WHERE Identity_Nr = " + id + ";";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();
            double balance = 0;
            while (reader.Read())
            {
                balance = Convert.ToDouble(reader["Balance"]);
            }
            reader.Close();
            connection.Close();
            return balance;
        }
        public static void UpdateBalance(string amount, int id)   
        {
            connection.Open();
            string sql = "UPDATE PARTICIPANT " +
                         "SET Balance = " + amount +
                         " WHERE Identity_Nr = " + id + ";";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
        public static void UpdateStatus(string status, int id)      
        {
            connection.Open();
            string sql = "UPDATE PARTICIPANT " +
                         "SET Status = 'participate' " +
                         "WHERE Identity_Nr = " + id + ";";
            if (status == "left")
            {
                sql = "Update PARTICIPANT " +
                      "SET Status = 'left', Balance = 0 " +
                      "WHERE Identity_Nr = " + id + ";";
            }
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
