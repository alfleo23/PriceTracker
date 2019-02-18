using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
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

            var formattedPrice = productPrices[headingIndex].TextContent.Replace('£', ' ').Trim();
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
                {"Price", productPrices[headingIndex].Text().Trim()},
                {"Formatted Price", formattedPrice},
                {"Similarity", bestSimilarityCoefficient},
                {"Product Link", productLinks[headingIndex].GetAttribute("href")}
            };
        }
        
    }
}