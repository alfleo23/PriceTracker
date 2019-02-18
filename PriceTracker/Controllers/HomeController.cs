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
            
            /*using (_context)
            {
                var testConnection = _context.Database.EnsureCreated();
                var test = _context.Database.ToString();
                Console.WriteLine("");
            }*/
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
                JohnLewisPrice = Convert.ToDouble(jLewisResults["Formatted Price"])
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
            // need to retrieve all saved searches from db and pass it to the view as a model

            var savedSearches = _context.SavedSearch.OrderByDescending(x => x.CreatedDate).Include(x => x.Results).ToList();

            // debug
//            foreach (var savedSearch in savedSearches)
//            {
//                Console.WriteLine(savedSearch.Results.FirstOrDefault().AmazonPrice);
//            }
            
            return View(savedSearches);
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
                stringBuilder.AppendLine($"{r.Date.ToString()}  {r.AmazonPrice}  {r.JohnLewisPrice}  {r.ResultId}");
            }

            return stringBuilder.ToString();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}