using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace CurrencyConverterAPI.Models
{
    public class Rates
    {
        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("base")]
        public string Base { get; set; }

        [JsonPropertyName("start_date")]
        public string StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string EndDate { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, double>> RateData { get; set; }
    }
    public class ExchangeRates
    {
        public double Amount { get; set; }
        public string Base { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Dictionary<string, Dictionary<string, double>> Rates { get; set; }
    }
}
