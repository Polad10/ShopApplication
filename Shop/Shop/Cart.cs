using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shop
{
    class Cart
    {
        //properties
        public List<Article> articles { get; private set; }
        
        //constructor
        public Cart()
        {
            articles = new List<Article>();
        }

        //methods
        public void AddArticle(string name, int items)
        {
            Article article = GetArticle(name);
            if (article == null)        //this article is not added yet
            {
                article = new Article(name, items);
                articles.Add(article);
            }
            else                        //this article already added
            {   
                article.AddItem(items);
            }
        }
        public void RemoveArticle(Article article)
        {
            if(article.Items > 1)
            {
                article.RemoveItem();           //decrement number of items
            }
            else
            {
                articles.Remove(article);       //remove the article itself
            }
        }
        private Article GetArticle(string name)
        {
            foreach(Article article in articles)
            {
                if(article.Name == name)
                {
                    return article;
                }
            }
            return null;
        }
        public double GetTotalPrice()
        {
            double totalPrice = 0;
            foreach(Article article in articles)
            {
                totalPrice += article.Price * article.Items;
            }
            return totalPrice;
        }
    }
}
