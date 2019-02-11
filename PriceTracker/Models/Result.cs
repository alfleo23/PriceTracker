using System;
using System.Collections.Generic;

namespace PriceTracker.Models
{
    public class Result
    {
        public int ResultId { get; set; }
        public DateTime? Date { get; set; }
        public double? AmazonPrice { get; set; }
        public double? EbayPrice { get; set; }
        public double? JohnLewisPrice { get; set; }
        public int? SavedSearchId { get; set; }

        public SavedSearch SavedSearch { get; set; }
    }
}
