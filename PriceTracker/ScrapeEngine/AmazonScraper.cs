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
    public class AmazonScraper : Scraper
    {
        //keep the scrapers in different classes to have custom implementations int the future
        private IHtmlCollection<IElement> productHeadings;
        private IHtmlCollection<IElement> productPrices;
        private IHtmlCollection<IElement> productLinks;
        
        public AmazonScraper()
        {
            SearchUrlSelector = "https://www.amazon.co.uk/s/ref=nb_sb_noss_1?url=search-alias%3Daps&field-keywords=";
            ProductHeadingSelector = ".s-access-title";
            ProductPriceSelector = ".s-price";
            ProductLinkSelector = ".a-link-normal.s-access-detail-page.s-color-twister-title-link.a-text-normal";
        }

        public async Task<Hashtable> ScrapePricesForProduct(string productName)
        {
            var similarity = 3000;
            var bestSimilarityCoefficient = 3000;
            var headingIndex = 0;
            
            var configuration = Configuration.Default.WithDefaultLoader().WithCookies().WithMetaRefresh();
            var context = BrowsingContext.New(configuration);
            await context.OpenAsync(SearchUrlSelector + productName);
            
            // get headings
            productHeadings = context.Active.QuerySelectorAll(ProductHeadingSelector);
            // get prices container
            productPrices = context.Active.QuerySelectorAll(ProductPriceSelector);
            // get product links
            productLinks = context.Active.QuerySelectorAll(ProductLinkSelector);

            
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

            
            Console.WriteLine("Amazon -------------");
            Console.WriteLine("Amazon best similarity " + bestSimilarityCoefficient);
            Console.WriteLine("Found product: " + productHeadings[headingIndex].Text() + "\n" + "price: " + productPrices[headingIndex].Text());
            Console.WriteLine("");
            
            
            // check if there is more than one price in the price element
            var pricesCount = productPrices[headingIndex].TextContent.Count(x => x == '£');
            if (pricesCount != 1)
            {
                // two prices
                var formattedPrices = productPrices[headingIndex].TextContent.Replace('£', ' ').SplitWithTrimming('-').ToList();
                var formattedPricesDouble = formattedPrices.Select(x => double.Parse(x)).ToList();
                var pricesAverage = formattedPricesDouble.Average();
            
                return new Hashtable
                {
                    {"Price", pricesAverage},
                    {"Formatted Price", pricesAverage},
                    {"Similarity", bestSimilarityCoefficient},
                    {"Product Heading", productHeadings[headingIndex].Text()},
                    {"Product Link", productLinks[headingIndex].GetAttribute("href")}
                };
            }

            // only one price
            var formattedPrice = productPrices[headingIndex].TextContent.Replace('£', ' ');
            var formattedPriceDouble = Convert.ToDouble(formattedPrice); 
            
            return new Hashtable
            {
                {"Price", productPrices[headingIndex].Text()},
                {"Formatted Price", formattedPrice},
                {"Similarity", bestSimilarityCoefficient},
                {"Product Heading", productHeadings[headingIndex].Text()},
                {"Product Link", productLinks[headingIndex].GetAttribute("href")}
            };
            
            
            

            /*var formattedPrice = productPrices[headingIndex].TextContent.Replace('£', ' ');
            var formattedPriceDouble = Convert.ToDouble(formattedPrice); 
            
            var productPricesFormatted = new List<double> {};
            foreach (var price in productPrices)
            {
                productPricesFormatted.Add(formattedPriceDouble);
            }
            
            // standard deviation
            Console.WriteLine("Standard deviation is: " + StringSimilarity.CalculateStandardDeviation(productPricesFormatted));
            Console.WriteLine("");

            return new Hashtable
            {
                {"Product Heading", productHeadings[headingIndex].Text()},
                {"Price", productPrices[headingIndex].Text()},
                {"Formatted Price", formattedPrice},
                {"Similarity", bestSimilarityCoefficient},
                {"Product Link", productLinks[headingIndex].GetAttribute("href")}
            };*/
        }
        
    }
}