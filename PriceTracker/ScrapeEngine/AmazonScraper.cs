using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace testRetailerClasses
{
    //TODO need to modify this class to work with entity framework
    public class AmazonScraper : Retailer
    {
        private IHtmlCollection<IElement> productHeadings;
        private IHtmlCollection<IElement> productPrices;
        
        public AmazonScraper()
        {
            SearchUrlSelector = "https://www.amazon.co.uk/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords=";
            ProductHeadingSelector = ".s-access-title";
            ProductPriceSelector = ".s-price";
        }

        public async Task ScrapePricesForProduct(string productName)
        {
            var similarity = 3000;
            var bestSimilarityCoefficient = 3000;
            var headingIndex = 0;
            
            var configuration = AngleSharp.Configuration.Default.WithDefaultLoader().WithCookies().WithMetaRefresh();
            var context = BrowsingContext.New(configuration);
            await context.OpenAsync(SearchUrlSelector + productName);
            
            // get headings
            productHeadings = context.Active.QuerySelectorAll(".s-access-title");
            // get prices container
            productPrices = context.Active.QuerySelectorAll(".s-price");

            
            for (int i = 0; i < productHeadings.Length; i++)
            {
                // just for testing
                // Console.WriteLine(productHeadings[i].Text().ToLower());
                    
                similarity = StringSimilarity.ComputeLevenshteinDistance(productHeadings[i].Text().ToLower(), productName);
                if (similarity < bestSimilarityCoefficient)
                {
                    bestSimilarityCoefficient = similarity;
                    headingIndex = i;
                }
            }

            
            Console.WriteLine("Amazon best similarity " + bestSimilarityCoefficient);
            Console.WriteLine("Amazon -------------");
            Console.WriteLine("Found product: " + productHeadings[headingIndex].Text() + "\n" + "price: " + productPrices[headingIndex].Text());
            Console.WriteLine("");

            //var dao = new DAO();
            var formattedPrice = productPrices[headingIndex].TextContent.Replace('£', ' ');
            var formattedPriceDouble = Convert.ToDouble(formattedPrice); 
            //dao.InsertPrice(formattedPriceDouble, "Amazon");

            
            // stupid logic for standard deviation
            List<double> productPricesFormatted = new List<double> {};
            foreach (var price in productPrices)
            {
                productPricesFormatted.Add(Convert.ToDouble(price.TextContent.Replace('£', ' ')));
            }
            
            Console.WriteLine("Standard deviation is: " + StringSimilarity.CalculateStandardDeviation(productPricesFormatted));
            Console.WriteLine("");
        }
        
    }
}