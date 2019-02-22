using System;
using System.Threading;
using System.Threading.Tasks;
using PriceTracker.Models;
using PriceTracker.ScrapeEngine;

namespace PriceTracker.TaskScheduler
{
    public class UpdateSearchesService : HostedService
    {
        private readonly UpdateTask _updateTask;

        public UpdateSearchesService(UpdateTask updateTask)
        {
            _updateTask = updateTask;
        }
        
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // this task was supposed to run on the background but the db operations are disabled
                await UpdateTask.UpdateSearches();
                await Task.Delay(TimeSpan.FromHours(2), cancellationToken);
            }
        }
    }
}