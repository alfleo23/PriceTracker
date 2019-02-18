using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Text;
using PriceTracker.Models;

namespace PriceTracker.ScrapeEngine
{
    public class JohnLewisScraper : Retailer
    {
        //keep the scrapers in different classes to have custom implementations int the future
        private IHtmlCollection<IElement> productHeadings;
        private IHtmlCollection<IElement> productPrices;
        
        public JohnLewisScraper()
        {
            SearchUrlSelector = "https://www.johnlewis.com/search?search-term={productPlaceHolder}";
            ProductHeadingSelector = ".product-card__title-inner";
            ProductPriceSelector = ".product-card__price-span";
        }

        public async Task<Hashtable> ScrapePricesForProduct(string productName)
        {
            var similarity = 3000;
            var bestSimilarityCoefficient = 3000;
            var headingIndex = 0;
            
            var configuration = Configuration.Default.WithDefaultLoader().WithCookies().WithMetaRefresh();
            var context = BrowsingContext.New(configuration);
            var searchPath = SearchUrlSelector.Replace("{productPlaceHolder}", productName);
            await context.OpenAsync(searchPath);
            
            // get headings
            productHeadings = context.Active.QuerySelectorAll(ProductHeadingSelector);
            // get prices container
            productPrices = context.Active.QuerySelectorAll(ProductPriceSelector);

            // in case the url readdress to a single product page
            if (productHeadings.Length == 0 || productPrices.Length == 0)
            {
                var singleProductHeading = context.Active.QuerySelectorAll(".product-header__title");
                var singleProductPrice = context.Active.QuerySelectorAll(".price--large");
                
                return new Hashtable
                {
                    {"Price", singleProductPrice.FirstOrDefault().Text().Replace('£', ' ')},
                    {"Formatted Price", singleProductPrice.FirstOrDefault().Text().Replace('£', ' ')},
                };
            }

            
            for (int i = 0; i < productHeadings.Length; i++)
            {
                // debug
                // Console.WriteLine(productHeadings[i].Text().ToLower());
                    
                similarity = StringSimilarity.ComputeLevenshteinDistance(productHeadings[i].Text().ToLower(), productName);
                if (similarity < bestSimilarityCoefficient)
                {
                    bestSimilarityCoefficient = similarity;
                    headingIndex = i;
                }
            }

            
            Console.WriteLine("John Lewis -------------");
            Console.WriteLine("John Lewis best similarity " + bestSimilarityCoefficient);
            Console.WriteLine("Found product: " + productHeadings[headingIndex].Text() + "\n" + "price: " + productPrices[headingIndex].Text());
            Console.WriteLine("");

            // check if there is more than one price in the price element
            if (productPrices[headingIndex].TextContent.Contains('-'))
            {
                // two prices
                var formattedPrices = productPrices[headingIndex].TextContent.Replace('£', ' ').SplitWithTrimming('-').ToList();
                var formattedPricesDouble = formattedPrices.Select(x => double.Parse(x)).ToList();
                var pricesAverage = formattedPricesDouble.Average();
            
                return new Hashtable
                {
                    {"Price", pricesAverage},
                    {"Formatted Price", pricesAverage},
                    {"Similarity", bestSimilarityCoefficient}
                };
            }
            else
            {
                // only one price
                var formattedPrice = productPrices[headingIndex].TextContent.Replace('£', ' ');
                var formattedPriceDouble = Convert.ToDouble(formattedPrice); 
            
                return new Hashtable
                {
                    {"Price", productPrices[headingIndex].Text()},
                    {"Formatted Price", formattedPrice},
                    {"Similarity", bestSimilarityCoefficient}
                };
            }
            
        }
        
    }
}