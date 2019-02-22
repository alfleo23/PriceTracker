using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PriceTracker.Models;
using PriceTracker.ScrapeEngine;

namespace PriceTracker.TaskScheduler
{
    public class UpdateTask
    {
        public static async Task UpdateSearches()
        {
            Console.WriteLine("task started successfully");

            using (var _context = new PriceTrackerContext())
            {
                var amazon = new AmazonScraper();
                var ebay = new EbayScraper();
                var jLewis = new JohnLewisScraper();

                //get the saved searches
                var savedSearches = _context.SavedSearch.OrderByDescending(x => x.CreatedDate).Include(x => x.Results).ToList();

                foreach (var search in savedSearches)
                {
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
                }

            }
        }
    }
}