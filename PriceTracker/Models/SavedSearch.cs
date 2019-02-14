using System;
using System.Collections.Generic;

namespace PriceTracker.Models
{
    public class SavedSearch
    {
        public SavedSearch()
        {
            Results = new HashSet<Result>();
        }

        public int SavedSearchId { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Result> Results { get; set; }
    }
}
