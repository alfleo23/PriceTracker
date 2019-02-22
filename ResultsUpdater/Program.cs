using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using ResultsUpdater.Models;

namespace ResultsUpdater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var updater = new Updater();
            
            while (true)
            {
                await updater.UpdateResults();
                
                //daily
                await Task.Delay(TimeSpan.FromHours(24));
                
                //every 10 minutes
                //await Task.Delay(TimeSpan.FromMinutes(10));
                
                // uncomment this to see how the background scraper is operating
                //await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }
        
        
     }
        
}