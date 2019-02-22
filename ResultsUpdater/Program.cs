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
                await Task.Delay(TimeSpan.FromSeconds(40));
            }
        }
        
        
     }
        
}