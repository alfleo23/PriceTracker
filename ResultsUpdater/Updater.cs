using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResultsUpdater.Models;
using PriceTracker.ScrapeEngine;

namespace ResultsUpdater
{
    public class Updater
    {
        
        public async Task UpdateResults()
        {
            Console.WriteLine($"Update Task started at {DateTime.Now}");

            using (var _context = new PriceTrackerContext())
            {
                var amazon = new AmazonScraper();
                var ebay = new EbayScraper();
                var jLewis = new JohnLewisScraper();

                //get the saved searches
                var savedSearches = _context.SavedSearch.OrderByDescending(x => x.CreatedDate).Include(x => x.Results).ToList();

                foreach (var search in savedSearches)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"scraping prices for search: {search.Description} , id {search.SavedSearchId}, created on: {search.CreatedDate}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    
                    Hashtable amazonResults;
                    Hashtable ebayResults;
                    Hashtable jLewisResults;

                    try
                    {
                        jLewisResults = await jLewis.ScrapePricesForProduct(search.Description);
                        ebayResults = await ebay.ScrapePricesForProduct(search.Description);
                        amazonResults = await amazon.ScrapePricesForProduct(search.Description);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                    var result = new Result()
                    {
                        Date = DateTime.Now,
                        AmazonPrice = Convert.ToDouble(amazonResults["Formatted Price"]),
                        EbayPrice = Convert.ToDouble(ebayResults["Formatted Price"]),
                        JohnLewisPrice = Convert.ToDouble(jLewisResults["Formatted Price"]),
                        AmazonLink = amazonResults["Product Link"].ToString(),
                        AmazonHeading = amazonResults["Product Heading"].ToString(),
                        EbayLink = ebayResults["Product Link"].ToString(),
                        EbayHeading = ebayResults["Product Heading"].ToString(),
                        JohnLewisHeading = jLewisResults["Product Heading"].ToString(),
                        JohnLewisLink = jLewisResults["Product Link"].ToString(),
                    };

                    search.Results.Add(result);
                    await _context.SaveChangesAsync();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("completed ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
            
            Console.WriteLine($"Update Task completed successfully at {DateTime.Now}");
        }
    }
    
}