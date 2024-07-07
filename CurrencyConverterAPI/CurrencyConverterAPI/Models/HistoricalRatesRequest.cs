using System;

namespace CurrencyConverterAPI.Models
{
    public class HistoricalRatesRequest
    {
        public string Base { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
