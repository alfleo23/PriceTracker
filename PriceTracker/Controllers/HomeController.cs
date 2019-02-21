using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceTracker.Models;
using PriceTracker.ScrapeEngine;

namespace PriceTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly PriceTrackerContext _context;

        public HomeController(PriceTrackerContext context)
        {
            _context = context;
            
            // TODO need to move this stuff away from here !!
            /*// logic to execute a callback asynchronously..will be useful for saved searches automatic updates...maybe has to be moved when the entity framework gets initialised
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(3);

            var timer = new System.Threading.Timer(e =>
            {
                Console.WriteLine("hello this is executed at:" + DateTime.Now);   
            }, null, startTimeSpan, periodTimeSpan);*/
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Result()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Result(string search)
        {
            var amazon = new AmazonScraper();
            var ebay = new EbayScraper();
            var jLewis = new JohnLewisScraper();
            Hashtable amazonResults;
            Hashtable ebayResults;
            Hashtable jLewisResults;
            ViewBag.ProductDescription = search;
            
            // scrape retailers
            try
            {
                jLewisResults = await jLewis.ScrapePricesForProduct(search);
                ebayResults = await ebay.ScrapePricesForProduct(search);
                amazonResults = await amazon.ScrapePricesForProduct(search);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // create new result
            //TODO add prices into the result object from other retailers once implemented
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
            
            //create Saved Search
            var savedSearch = new SavedSearch()
            {
                CreatedDate = DateTime.Now,
                Description = search,
            };
            
            // create save search view model
            var viewModel = new SaveSearchViewModel()
            {
                Result = result,
                SavedSearch = savedSearch
            };
            
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Save(SaveSearchViewModel s)
        {
            // save search into db
            try
            {
                _context.Database.ExecuteSqlCommand("SET foreign_key_checks = 0;");

                _context.SavedSearch.Add(s.SavedSearch);
                _context.SaveChanges();
                
                // dangerous approach..leads to incongruity if there are more db accesses from different users
                //todo consider locking the table while doing the magic
                var lastInserted = _context.SavedSearch.Last();
                var id = lastInserted.SavedSearchId;
                s.Result.SavedSearchId = id;
                
                _context.Result.Add(s.Result);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return RedirectToAction("SavedSearch", "Home");
        }

        public IActionResult SavedSearch()
        {
            //retrieve all saved searches from db and pass it to the view as a model
            var savedSearches = _context.SavedSearch.OrderByDescending(x => x.CreatedDate).Include(x => x.Results).ToList();

            // debug
            /*foreach (var savedSearch in savedSearches)
            {
                Console.WriteLine(savedSearch.Results.FirstOrDefault().AmazonPrice);
            }*/
            
            return View(savedSearches);
        }

        [HttpPost]
        public IActionResult DeleteSearch(int savedSearchID)
        {
            if (savedSearchID == 0 ) return RedirectToAction("Error");
            
            _context.Database.ExecuteSqlCommand("SET foreign_key_checks = 0;");
            
            //get a unique saved search
            var savedSearch = _context.SavedSearch.Where(search => search.SavedSearchId == savedSearchID).Include(search => search.Results).FirstOrDefault();
            
            if (savedSearch != null)
            {
                // remove all the results associated with this search id
                _context.Result.RemoveRange(savedSearch.Results);
                // remove saved search
                _context.SavedSearch.Remove(savedSearch);
                _context.SaveChanges();
            }
            
            return RedirectToAction("SavedSearch", "Home");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public string TestSelect()
        {
            var result = _context.Result.ToList();
            var stringBuilder = new StringBuilder();
            foreach (var r in result)
            {
                stringBuilder.AppendLine($"{r.Date.ToString()}  {r.AmazonPrice} {r.EbayPrice} {r.JohnLewisPrice}  {r.ResultId}");
            }

            return stringBuilder.ToString();
        }

        public async Task<string> Update()
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
            var testSavedSearch = _context.SavedSearch.Find(1);

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

            return "successful";



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
            }

            return "successful";*/
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}