using System;
using System.Collections.Generic;

namespace PriceTracker.Models
{
    public partial class Result
    {
        public int ResultId { get; set; }
        public DateTime? Date { get; set; }
        public string AmazonHeading { get; set; }
        public double? AmazonPrice { get; set; }
        public string AmazonLink { get; set; }
        public string EbayHeading { get; set; }
        public double? EbayPrice { get; set; }
        public string EbayLink { get; set; }
        public double? JohnLewisPrice { get; set; }
        public string JohnLewisHeading { get; set; }
        public string JohnLewisLink { get; set; }
        public int? SavedSearchId { get; set; }

        public SavedSearch SavedSearch { get; set; }
    }
}
