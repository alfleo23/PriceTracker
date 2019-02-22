using System;
using System.Collections;
using System.Threading.Tasks;
using PriceTracker.Models;
using PriceTracker.ScrapeEngine;

namespace PriceTracker.TaskScheduler
{
    public class UpdateTask
    {
        public UpdateTask()
        {

        }

        public async Task UpdateSearches()
        {
            Console.WriteLine("task started successfully");

            using (var _context = new PriceTrackerContext())
            {
                var amazon = new AmazonScraper();
                var ebay = new EbayScraper();
                var jLewis = new JohnLewisScraper();
                Hashtable amazonResults;
                Hashtable ebayResults;
                Hashtable jLewisResults;

                //get the saved searches
                //var savedSearches = _context.SavedSearch.OrderByDescending(x => x.CreatedDate).Include(x => x.Results).ToList();

                //testing with only one saved search to automatically update 
                var testSavedSearch = _context.SavedSearch.Find(8);

                try
                {
                    jLewisResults = await jLewis.ScrapePricesForProduct(testSavedSearch.Description);
                    ebayResults = await ebay.ScrapePricesForProduct(testSavedSearch.Description);
                    amazonResults = await amazon.ScrapePricesForProduct(testSavedSearch.Description);
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

                testSavedSearch.Results.Add(result);
                _context.SaveChanges();

                Console.WriteLine("db updated");

                /*foreach (var search in testSavedSearch)
                {
                    try
                    {
                        jLewisResults = await jLewis.ScrapePricesForProduct(search.Description);
                        ebayResults = await ebay.ScrapePricesForProduct(search.Description);
                        amazonResults = await amazon.ScrapePricesForProduct(search.);
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
                }*/

            }
        }
    }
}