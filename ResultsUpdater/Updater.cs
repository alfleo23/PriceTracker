using System;
using System.Collections;
using System.Linq;
using System.Net.Mail;
using System.Text;
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

                if (savedSearches.Count != 0)
                {
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
                        
                        //get the latest result for this saved search
                        var currentSearchResult = search.Results.OrderByDescending(x => x.Date).FirstOrDefault();

                        
                        search.Results.Add(result);
                        await _context.SaveChangesAsync();

                        // compare the prices here
                        var emailBody = ComparePrices(result, currentSearchResult);
                        if (!string.IsNullOrEmpty(emailBody))
                        {
                            Console.WriteLine(SendNotificationEmail(emailBody));
                        }
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("completed ");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                else
                {
                    Console.WriteLine("No Saved Searches to update");
                }
            }
            
            Console.WriteLine($"Update Task completed successfully at {DateTime.Now}");
        }

        private string ComparePrices(Result newResult, Result currentResult)
        {
            var emailBody = new StringBuilder();
            bool anyChanges = false;
            
            if (currentResult != null)
            {
                if (newResult.AmazonPrice < currentResult.AmazonPrice)
                {
                    emailBody.AppendLine(
                        $"Amazon price dropped, was {currentResult.AmazonPrice}, now is {newResult.AmazonPrice}");
                    anyChanges = true;
                }

                if (newResult.EbayPrice < currentResult.EbayPrice)
                {
                    emailBody.AppendLine(
                        $"Ebay price dropped, was {currentResult.EbayPrice}, now is {newResult.EbayPrice}");
                    anyChanges = true;

                }

                if (newResult.JohnLewisPrice < currentResult.JohnLewisPrice)
                {
                    emailBody.AppendLine(
                        $"John Lewis price dropped, was {currentResult.JohnLewisPrice}, now is {newResult.JohnLewisPrice}");
                    anyChanges = true;
                }
            }

            return (anyChanges) ? emailBody.ToString() : "";
        }

        private string SendNotificationEmail(string emailBody)
        {
            var password = "email password";
            var sender = "email sender";
            var receiver = "email receiver";
                
            SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
            var mail = new MailMessage();
            mail.From = new MailAddress(sender);
            mail.To.Add(receiver);
            mail.Subject = "Test Mail - 1";
            mail.IsBodyHtml = true;
            
            string htmlBody;
            htmlBody = emailBody;
            mail.Body = htmlBody;
            
            SmtpServer.Port = 587;
            SmtpServer.UseDefaultCredentials = false;
            SmtpServer.Credentials = new System.Net.NetworkCredential(sender, password);
            SmtpServer.EnableSsl = true;
            SmtpServer.Send(mail);

            return "Mail sent successfully!";
        }

    }
    
}