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
    public class EbayScraper : Scraper
    {
        //keep the scrapers in different classes to have custom implementations int the future
        private IHtmlCollection<IElement> productHeadings;
        private IHtmlCollection<IElement> productPrices;
        private IHtmlCollection<IElement> productLinks;

        
        public EbayScraper()
        {
            SearchUrlSelector = "https://www.ebay.co.uk/sch/i.html?_from=R40&_trksid=p2380057.m570.l1313.TR12.TRC2.A0.H0.X{productPlaceHolder}.TRS0&_nkw={productPlaceHolder}&_sacat=0&LH_ItemCondition=1000";
            ProductHeadingSelector = ".vip";
            ProductPriceSelector = ".prc .bold";
            ProductLinkSelector = ".vip";
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

            
            Console.WriteLine("Ebay -------------");
            Console.WriteLine("Ebay best similarity " + bestSimilarityCoefficient);
            Console.WriteLine("Found product: " + productHeadings[headingIndex].Text() + "\n" + "price: " + productPrices[headingIndex].Text().Trim());
            Console.WriteLine("");

            // check if there is more than one price in the price element
            var pricesCount = productPrices[headingIndex].TextContent.Count(x => x == '£');
            if (pricesCount != 1)
            {
                // two prices
                var formattedPrices = productPrices[headingIndex].TextContent.Replace('£', ' ').Trim().Split(new string[] {"to"}, StringSplitOptions.None).ToList();
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
        }
        
    }
}