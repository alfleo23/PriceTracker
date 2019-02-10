using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceTracker.Models;

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

        public IActionResult Search()
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
        }

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