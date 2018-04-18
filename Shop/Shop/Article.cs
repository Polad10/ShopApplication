using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using EventLibrary;

namespace Shop
{
    class Article
    {
        //fields
        int items;
        MySqlConnection connection;

        //properties
        public string Name { get; private set; }
        public int ItemsAvailable { get; private set; }     //number of items available in stock
        public int ItemsSold { get; private set; }          //total number of items sold
        public string ArticleCode { get; private set; }
        public double Price { get; private set; }
        public int Items                                    //number of items visitor wants to buy
        {
            get { return items; }
            private set
            {
                if(value <= ItemsAvailable)
                {
                    items = value;
                }
                else
                {
                    throw new InsufficientItemsException("Insufficient number of items in stock!");
                }
            }
        }

        //constructor
        public Article(string name, int items)
        {
            connection = new MySqlConnection(Connection.CONNECTIONINFO);
            this.Name = name;
            SetArticleCodeAndPrice();
            SetNrOfAvailableItems();
            this.Items = items;
        }
        
        //methods
        public void AddItem(int itemNr)
        {
            Items += itemNr;
        }
        public void RemoveItem()
        {
            Items--;
        }
        private void SetArticleCodeAndPrice()
        {
            connection.Open();
            string sql = "SELECT Article_Code, Price " +
                         "FROM ARTICLE " +
                         "WHERE Article_Name = '" + Name + "';";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ArticleCode = reader["Article_Code"].ToString();
                Price = Convert.ToDouble(reader["Price"]);
            }
            reader.Close();
            connection.Close();
        }
        private void SetNrOfAvailableItems()
        {
            connection.Open();
            int shopNr = Form1.SHOPNR;
            string sql = "SELECT InStock, Sold " +
                         "FROM SHOP_ARTICLE " +
                         "WHERE Shop_Nr = " + shopNr +
                         " AND Article_Code = '" + ArticleCode + "';";
            MySqlCommand command = new MySqlCommand(sql, connection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ItemsAvailable = Convert.ToInt32(reader["InStock"]);
                ItemsSold = Convert.ToInt32(reader["Sold"]);
            }
            reader.Close();
            connection.Close();
        }
        public override string ToString()
        {
            return Name + "  " + Items + "x";
        }
    }
}
