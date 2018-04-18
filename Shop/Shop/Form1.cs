using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Phidgets.Events;
using Phidgets;
using EventLibrary;             //custom library

namespace Shop
{
    public partial class Form1 : Form
    {
        //fields
        MySqlConnection connection;
        public const int SHOPNR = 1;        //stores shop's number
        Cart cart;
        List<NumericUpDown> nuds;           //used by errorTimer
        public Form1()
        {
            InitializeComponent();
            this.Text = "Shop " + SHOPNR;
            //connection = new MySqlConnection(Connection.CONNECTIONINFO);

            this.BackColor = Color.FromArgb(81, 80, 82);
            orderGroupBox.BackColor = Color.FromArgb(51, 49, 56);
            orderListBox.BackColor = Color.FromArgb(51, 49, 56);
            pricesListBox.BackColor = Color.FromArgb(51, 49, 56);
            orderListBox.ForeColor = Color.FromArgb(255, 255, 250);
            pricesListBox.ForeColor = Color.FromArgb(255, 255, 250);
            totalLabel.ForeColor = Color.FromArgb(255, 255, 250);
            totalPriceLabel.ForeColor = Color.FromArgb(255, 255, 250);
            nuds = new List<NumericUpDown>();
            Initialize();

            //sausageMcMuffinPriceLabel.Text = GetPrice(sausageMcMuffinLabel.Text);
            //McDoublePriceLabel.Text = GetPrice(McDoubleLabel.Text);
            //mightyAngusPriceLabel.Text = GetPrice(mightyAngusLabel.Text);
            //juniorChickenPriceLabel.Text = GetPrice(juniorChickenLabel.Text);
            //baconRanchChickenPriceLabel.Text = GetPrice(baconRanchChickenLabel.Text);
            //baconMcDoublePriceLabel.Text = GetPrice(baconMcDoubleLabel.Text);
            //bigMacPriceLabel.Text = GetPrice(bigMacLabel.Text);
            //angusClassicPriceLabel.Text = GetPrice(angusClassicLabel.Text);
            //teaPriceLabel.Text = GetPrice(teaLabel.Text);
            //shamrockShakePriceLabel.Text = GetPrice(shamrockShakeLabel.Text);
            //strawberryBananaPriceLabel.Text = GetPrice(strawberryBananaLabel.Text);
            //sundaesPriceLabel.Text = GetPrice(sundaesLabel.Text);
            //roastBrewedIcedCoffeePriceLabel.Text = GetPrice(roastBrewedIcedCoffeeLabel.Text);
            //roastBrewedCoffeePriceLabel.Text = GetPrice(roastBrewedCoffeeLabel.Text);
            //spearmintGumPriceLabel.Text = GetPrice(spearmintGumLabel.Text);
            //mustardSaucePriceLabel.Text = GetPrice(mustardSauceLabel.Text);
            //ketchupSaucePriceLabel.Text = GetPrice(ketchupSauceLabel.Text);
            //barbequeSaucePriceLabel.Text = GetPrice(barbequeSauceLabel.Text);
            //bigmacSaucePriceLabel.Text = GetPrice(bigmacSauceLabel.Text);

            cart = new Cart();

            try
            {
                MyPhidget.Start();
                MyPhidget.AddToTagEvent(Pay);
            }
            catch (PhidgetException)
            {
                MessageBox.Show("Failed to establish connection with Phidget");
            }
        }
        private void Initialize()
        {
            foodsPanel.Visible = false;
            drinksPanel.Visible = false;
            dessertsPanel.Visible = false;
            othersPanel.Visible = false;
            foodsButton.BackColor = Color.FromArgb(51, 49, 56);
            drinksButton.BackColor = Color.FromArgb(51, 49, 56);
            dessertsButton.BackColor = Color.FromArgb(51, 49, 56);
            othersButton.BackColor = Color.FromArgb(51, 49, 56);
        }
        private void foodsButton_Click(object sender, EventArgs e)
        {
            Initialize();
            foodsPanel.Visible = true;
            foodsButton.BackColor = Color.FromArgb(0, 64, 0);
        }

        private void drinksButton_Click(object sender, EventArgs e)
        {
            Initialize();
            drinksPanel.Visible = true;
            drinksButton.BackColor = Color.FromArgb(0, 64, 0);
        }

        private void dessertsButton_Click(object sender, EventArgs e)
        {
            Initialize();
            dessertsPanel.Visible = true;
            dessertsButton.BackColor = Color.FromArgb(0, 64, 0);
        }

        private void othersButton_Click(object sender, EventArgs e)
        {
            Initialize();
            othersPanel.Visible = true;
            othersButton.BackColor = Color.FromArgb(0, 64, 0);
        }
        private void Pay(object sender, TagEventArgs e)
        {
            totalPriceLabel.ForeColor = Color.FromArgb(255, 255, 250);

            if (cart.GetTotalPrice() != 0)          
            {
                try
                {
                    string tagId = e.Tag;
                    int id = Visitor.GetId(tagId);
                    double balance = Visitor.GetBalance(id);
                    double total = cart.GetTotalPrice();

                    if(total <= balance)        //sufficient balance
                    {
                        connection.Open();
                        double remainder = balance - total;
                        string remainderBalance = remainder.ToString().Replace(',', '.');
                        Visitor.UpdateBalance(remainderBalance, id);

                        string totalPrice = total.ToString().Replace(',', '.');
                        string sql = "INSERT INTO SALES_ORDER " +
                                     "(Date, Total, Identity_Nr, Shop_Nr) " +
                                     "VALUES ('" + DateTime.Now.ToShortDateString() + "', " + totalPrice + ", " + id + ", " + SHOPNR + ");";
                        MySqlCommand command = new MySqlCommand(sql, connection);
                        command.ExecuteNonQuery();

                        sql = "SELECT MAX(Order_Nr) " +
                              "FROM Sales_Order " +
                              "WHERE Identity_Nr = " + id + ";";
                        command = new MySqlCommand(sql, connection);
                        int orderNr = (int)command.ExecuteScalar();

                        List<Article> articles = cart.articles;
                        foreach (Article article in articles)
                        {
                            int inStock = article.ItemsAvailable - article.Items;
                            int totalSold = article.ItemsSold + article.Items;
                            sql = "UPDATE SHOP_ARTICLE " +
                                  "SET InStock = " + inStock + ", Sold = " + totalSold +
                                  " WHERE Shop_Nr = " + SHOPNR +
                                  " AND Article_Code = '" + article.ArticleCode + "';";
                            command = new MySqlCommand(sql, connection);
                            command.ExecuteNonQuery();

                            string extendedPrice = (article.Price * article.Items).ToString().Replace(',', '.');
                            sql = "INSERT INTO ORDER_LINE_ARTICLE " +
                                  "(Order_Nr, Quantity, Extended_Price, Article_Code) " +
                                  "VALUES (" + orderNr + ", " + article.Items + ", " + extendedPrice + ", '" + article.ArticleCode + "');";
                            command = new MySqlCommand(sql, connection);
                            command.ExecuteNonQuery();

                            string amount = article.Price.ToString().Replace(',', '.');
                            string info = "Payment in shop";
                            for (int i = 0; i < article.Items; i++)
                            {
                                Transaction.Add(info, amount, id);
                            }
                        }
                        cart.articles.Clear();
                        UpdateInfo();
                    }
                    else
                    {
                        totalPriceLabel.ForeColor = Color.FromArgb(192, 0, 0);
                    }
                }
                catch (MySqlException)
                {
                    MessageBox.Show("Something went wrong with database operation");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void Reset()
        {
            ketchupSauceNUD.Value = 0;
            mustardSauceNUD.Value = 0;
            barbequeSauceNUD.Value = 0;
            bigmacSauceNUD.Value = 0;
            spearmintGumNUD.Value = 0;
            bakedMuffinsNUD.Value = 0;
            cinnamonMeltsNUD.Value = 0;
            bakedPieNUD.Value = 0;
            appleSlicesNUD.Value = 0;
            sundaesNUD.Value = 0;
            shamrockShakeNUD.Value = 0;
            iceCreamNUD.Value = 0;
            cookieNUD.Value = 0;
            blueberryPomegranateNUD.Value = 0;
            mangoPineappleNUD.Value = 0;
            strawberryBananaNUD.Value = 0;
            roastBrewedIcedCoffeeNUD.Value = 0;
            teaNUD.Value = 0;
            roastBrewedCoffeeNUD.Value = 0;
            appleJuiceNUD.Value = 0;
            orangeJuiceNUD.Value = 0;
            sausageMcMuffinNUD.Value = 0;
            McDoubleNUD.Value = 0;
            baconMcDoubleNUD.Value = 0;
            juniorChickenNUD.Value = 0;
            baconRanchChickenNUD.Value = 0;
            mightyAngusNUD.Value = 0;
            angusClassicNUD.Value = 0;
            bigMacNUD.Value = 0;
        }
        private void AddToCart(string name, int items, NumericUpDown nud)
        {
            if(items != 0)
            {
                try
                {
                    cart.AddArticle(name, items);
                    Reset();
                    UpdateInfo();
                }
                catch (MySqlException)
                {
                    MessageBox.Show("Something went wrong with database operation");
                }
                catch (InsufficientItemsException)
                {
                    nud.ForeColor = Color.Black;
                    nud.BackColor = Color.Red;
                    nuds.Add(nud);
                    errorTimer.Start();
                }
            }
        }
        private void UpdateInfo()
        {
            List<Article> articles = cart.articles;
            orderListBox.Items.Clear();
            pricesListBox.Items.Clear();

            foreach (Article article in articles)
            {
                orderListBox.Items.Add(article);
                pricesListBox.Items.Add(article.Price * article.Items + " €");
            }

            totalPriceLabel.Text = cart.GetTotalPrice() + " €";
        }
        private void clearAllButton_Click(object sender, EventArgs e)
        {
            totalPriceLabel.ForeColor = Color.FromArgb(255, 255, 250);
            cart.articles.Clear();
            UpdateInfo();
            Reset();
        }

        private void removeItemButton_Click(object sender, EventArgs e)
        {
            totalPriceLabel.ForeColor = Color.FromArgb(255, 255, 250);
            Article article = (Article)orderListBox.SelectedItem;

            if (article != null)
            {
                cart.RemoveArticle(article);
                UpdateInfo();
            }
        }
        private void ketchupSauceAddButton_Click(object sender, EventArgs e)
        {
            int items = (int)ketchupSauceNUD.Value;
            string name = ketchupSauceLabel.Text;
            AddToCart(name, items, ketchupSauceNUD);
        }

        private void mustardSauceAddButton_Click(object sender, EventArgs e)
        {
            int items = (int)mustardSauceNUD.Value;
            string name = mustardSauceLabel.Text;
            AddToCart(name, items, mustardSauceNUD);
        }

        private void barbequeSauceAddButton_Click(object sender, EventArgs e)
        {
            int items = (int)barbequeSauceNUD.Value;
            string name = barbequeSauceLabel.Text;
            AddToCart(name, items, barbequeSauceNUD);
        }

        private void bigmacSauceAddButton_Click(object sender, EventArgs e)
        {
            int items = (int)bigmacSauceNUD.Value;
            string name = bigmacSauceLabel.Text;
            AddToCart(name, items, bigmacSauceNUD);
        }

        private void spearmintGumAddButton_Click(object sender, EventArgs e)
        {
            int items = (int)spearmintGumNUD.Value;
            string name = spearmintGumLabel.Text;
            AddToCart(name, items, spearmintGumNUD);
        }
        private void bakedMuffinsButton_Click(object sender, EventArgs e)
        {
            int items = (int)bakedMuffinsNUD.Value;
            string name = bakedMuffinsLabel.Text;
            AddToCart(name, items, bakedMuffinsNUD);
        }

        private void cinnamonMeltsButton_Click(object sender, EventArgs e)
        {
            int items = (int)cinnamonMeltsNUD.Value;
            string name = cinnamonMeltsLabel.Text;
            AddToCart(name, items, cinnamonMeltsNUD);
        }

        private void bakedPieButton_Click(object sender, EventArgs e)
        {
            int items = (int)bakedPieNUD.Value;
            string name = bakedPieLabel.Text;
            AddToCart(name, items, bakedPieNUD);
        }

        private void appleSlicesButton_Click(object sender, EventArgs e)
        {
            int items = (int)appleSlicesNUD.Value;
            string name = appleSlicesLabel.Text;
            AddToCart(name, items, appleSlicesNUD);
        }

        private void sundaesButton_Click(object sender, EventArgs e)
        {
            int items = (int)sundaesNUD.Value;
            string name = sundaesLabel.Text;
            AddToCart(name, items, sundaesNUD);
        }

        private void shamrockShakeButton_Click(object sender, EventArgs e)
        {
            int items = (int)shamrockShakeNUD.Value;
            string name = shamrockShakeLabel.Text;
            AddToCart(name, items, shamrockShakeNUD);
        }

        private void iceCreamButton_Click(object sender, EventArgs e)
        {
            int items = (int)iceCreamNUD.Value;
            string name = iceCreamLabel.Text;
            AddToCart(name, items, iceCreamNUD);
        }

        private void cookieButton_Click(object sender, EventArgs e)
        {
            int items = (int)cookieNUD.Value;
            string name = cookieLabel.Text;
            AddToCart(name, items, cookieNUD);
        }

        private void blueberryPomegranateButton_Click(object sender, EventArgs e)
        {
            int items = (int)blueberryPomegranateNUD.Value;
            string name = blueberryPomegranateLabel.Text;
            AddToCart(name, items, blueberryPomegranateNUD);
        }

        private void mangoPineappleButton_Click(object sender, EventArgs e)
        {
            int items = (int)mangoPineappleNUD.Value;
            string name = mangoPineappleLabel.Text;
            AddToCart(name, items, mangoPineappleNUD);
        }

        private void strawberryBananaButton_Click(object sender, EventArgs e)
        {
            int items = (int)strawberryBananaNUD.Value;
            string name = strawberryBananaLabel.Text;
            AddToCart(name, items, strawberryBananaNUD);
        }

        private void roastBrewedIcedCoffeeButton_Click(object sender, EventArgs e)
        {
            int items = (int)roastBrewedIcedCoffeeNUD.Value;
            string name = roastBrewedIcedCoffeeLabel.Text;
            AddToCart(name, items, roastBrewedIcedCoffeeNUD);
        }

        private void teaButton_Click(object sender, EventArgs e)
        {
            int items = (int)teaNUD.Value;
            string name = teaLabel.Text;
            AddToCart(name, items, teaNUD);
        }

        private void roastBrewedCoffeeButton_Click(object sender, EventArgs e)
        {
            int items = (int)roastBrewedCoffeeNUD.Value;
            string name = roastBrewedCoffeeLabel.Text;
            AddToCart(name, items, roastBrewedCoffeeNUD);
        }

        private void appleJuiceButton_Click(object sender, EventArgs e)
        {
            int items = (int)appleJuiceNUD.Value;
            string name = appleJuiceLabel.Text;
            AddToCart(name, items, appleJuiceNUD);
        }

        private void orangeJuiceButton_Click(object sender, EventArgs e)
        {
            int items = (int)orangeJuiceNUD.Value;
            string name = orangeJuiceLabel.Text;
            AddToCart(name, items, orangeJuiceNUD);
        }

        private void sausageMcMuffinButton_Click(object sender, EventArgs e)
        {
            int items = (int)sausageMcMuffinNUD.Value;
            string name = sausageMcMuffinLabel.Text;
            AddToCart(name, items, sausageMcMuffinNUD);
        }

        private void McDoubleButton_Click(object sender, EventArgs e)
        {
            int items = (int)McDoubleNUD.Value;
            string name = McDoubleLabel.Text;
            AddToCart(name, items, McDoubleNUD);
        }

        private void baconMcDoubleButton_Click(object sender, EventArgs e)
        {
            int items = (int)baconMcDoubleNUD.Value;
            string name = baconMcDoubleLabel.Text;
            AddToCart(name, items, baconMcDoubleNUD);
        }

        private void juniorChickenButton_Click(object sender, EventArgs e)
        {
            int items = (int)juniorChickenNUD.Value;
            string name = juniorChickenLabel.Text;
            AddToCart(name, items, juniorChickenNUD);
        }

        private void baconRanchChickenButton_Click(object sender, EventArgs e)
        {
            int items = (int)baconRanchChickenNUD.Value;
            string name = baconRanchChickenLabel.Text;
            AddToCart(name, items, baconRanchChickenNUD);
        }

        private void mightyAngusButton_Click(object sender, EventArgs e)
        {
            int items = (int)mightyAngusNUD.Value;
            string name = mightyAngusLabel.Text;
            AddToCart(name, items, mightyAngusNUD);
        }

        private void angusClassicButton_Click(object sender, EventArgs e)
        {
            int items = (int)angusClassicNUD.Value;
            string name = angusClassicLabel.Text;
            AddToCart(name, items, angusClassicNUD);
        }

        private void bigMacButton_Click(object sender, EventArgs e)
        {
            int items = (int)bigMacNUD.Value;
            string name = bigMacLabel.Text;
            AddToCart(name, items, bigMacNUD);
        }

        private void errorTimer_Tick(object sender, EventArgs e)        //go back to default state
        {
            errorTimer.Stop();

            foreach(NumericUpDown nud in nuds)
            {
                nud.BackColor = Color.Black;
                nud.ForeColor = Color.FromArgb(255, 224, 192);
            }

            nuds.Clear();
        }
        private string GetPrice(string articleName)
        {
            string price = null;

            try
            {
                connection.Open();
                string sql = "SELECT Price " +
                             "FROM ARTICLE " +
                             "WHERE Article_Name = '" + articleName + "';";
                MySqlCommand command = new MySqlCommand(sql, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    price = reader["Price"].ToString() + " €";
                }
                reader.Close();
            }
            catch (MySqlException)
            {
                MessageBox.Show("Something went wrong with database operation");
            }
            finally
            {
                connection.Close();
            }
            return price;
        }
    }
}
