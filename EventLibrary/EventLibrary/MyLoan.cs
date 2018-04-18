using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EventLibrary
{
    public class MyLoan
    {
        //fields
        static MySqlConnection connection = new MySqlConnection(Connection.CONNECTIONINFO);
        public static List<int> GetLoans(int id)
        {
            connection.Open();
            List<int> loans = new List<int>();
            string sql = "SELECT Loan_Nr " +
                         "FROM LOAN " +
                         "WHERE Identity_Nr = '" + id + "';";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int loanNr = Convert.ToInt32(reader["Loan_Nr"]);
                loans.Add(loanNr);
            }
            reader.Close();
            connection.Close();
            return loans;
        }
        public static void RemoveLoan(int loanNr)
        {
            connection.Open();
            string sql = "DELETE FROM LOAN " +
                         "WHERE Loan_Nr = " + loanNr + ";";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
        public static void UpdateMaterialItems(string materialCode, int inStockNow)
        {
            connection.Open();
            string sql = "UPDATE MATERIAL " +
                         "SET Items = " + inStockNow +
                         " WHERE Material_Code = '" + materialCode + "';";
            MySqlCommand command = new MySqlCommand(sql, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
