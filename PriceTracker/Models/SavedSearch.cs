using System;
using System.Collections.Generic;

namespace PriceTracker.Models
{
    public partial class SavedSearch
    {
        public SavedSearch()
        {
            Results = new HashSet<Result>();
        }

        public int SavedSearchId { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }

        public ICollection<Result> Results { get; set; }
    }
}
