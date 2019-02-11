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
using testRetailerClasses;

namespace PriceTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly PriceTrackerContext _context;

        public HomeController(PriceTrackerContext context)
        {
            _context = context;
            
//            using (_context)
//            {
//                var testConnection = _context.Database.EnsureCreated();
//                var test = _context.Database.ToString();
//                Console.WriteLine("");
//            }
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
            Hashtable amazonResults;
            ViewBag.ProductDescription = search;
            
            try
            {
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
                Date = DateTime.Today,
                AmazonPrice = Convert.ToDouble(amazonResults["Formatted Price"])
            };
            
            return View(result);
        }

        public IActionResult SavedSearch()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /*public IActionResult Search()
        {
            var instResult = new Result();

            try
            {
                _context.Result.Add(instResult);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            return View();
        }*/

        public string TestSelect()
        {
            var result = _context.Result.ToList();
            var stringBuilder = new StringBuilder();
            foreach (var r in result)
            {
                stringBuilder.AppendLine(r.Date.ToString() + r.AmazonPrice + r.JohnLewisPrice + r.ResultId);
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