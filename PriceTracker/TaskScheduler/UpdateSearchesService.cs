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
                await _updateTask.UpdateSearches();
                await Task.Delay(TimeSpan.FromSeconds(40), cancellationToken);
            }
        }
    }
}