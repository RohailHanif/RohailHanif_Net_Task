using CurrencyConverterAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly List<string> _excludedCurrencies = new List<string> { "TRY", "PLN", "THB", "MXN" };
        private readonly IHttpClientFactory _httpClientFactory;
        private const string BaseUrl = "https://api.frankfurter.app/";

        // Constructor to initialize the IHttpClientFactory
        public CurrencyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Retrieves the latest exchange rates for a specific base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency for which to get exchange rates.</param>
        /// <returns>A JSON object with the latest exchange rates.</returns>
        [HttpGet("latest/{baseCurrency}")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await GetWithRetryAsync(client, $"{BaseUrl}latest?base={baseCurrency}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(JsonSerializer.Deserialize<object>(content));
            }

            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="request">The conversion request containing amount, from currency, and to currency.</param>
        /// <returns>The converted amount or a bad request response if the currencies are not supported.</returns>
        [HttpGet("convert")]
        public async Task<IActionResult> ConvertCurrency([FromBody] ConversionRequest request)
        {
            if (_excludedCurrencies.Contains(request.FromCurrency) || _excludedCurrencies.Contains(request.ToCurrency))
            {
                return BadRequest("Currency conversion not supported for TRY, PLN, THB, and MXN.");
            }

            var client = _httpClientFactory.CreateClient();
            var response = await GetWithRetryAsync(client, $"{BaseUrl}latest?amount={request.Amount}&from={request.FromCurrency}&to={request.ToCurrency}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(JsonSerializer.Deserialize<object>(content));
            }
            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }

        /// <summary>
        /// Retrieves historical exchange rates for a given period using pagination.
        /// </summary>
        /// <param name="request">The historical rates request containing the base currency and date range.</param>
        /// <param name="page">The page number for pagination.</param>
        /// <param name="pageSize">The page size for pagination.</param>
        /// <returns>A paginated set of historical exchange rates.</returns>
        [HttpGet("historical")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery] HistoricalRatesRequest request, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await GetWithRetryAsync(client, $"{BaseUrl}{request.StartDate:yyyy-MM-dd}..{request.EndDate:yyyy-MM-dd}?to={request.Base}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var rates = JsonSerializer.Deserialize<Rates>(content);
                var paginatedRates = rates.RateData
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var paginatedResponse = new ExchangeRates
                {
                    Amount = rates.Amount,
                    Base = rates.Base,
                    StartDate = rates.StartDate,
                    EndDate = rates.EndDate,
                    Rates = paginatedRates
                };
                return Ok(paginatedResponse);
            }

            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }

        /// <summary>
        /// Executes an HTTP GET request with retry logic.
        /// </summary>
        /// <param name="client">The HttpClient to use for the request.</param>
        /// <param name="url">The URL for the request.</param>
        /// <param name="maxRetries">The maximum number of retries (default is 3).</param>
        /// <returns>The HTTP response message.</returns>
        private async Task<HttpResponseMessage> GetWithRetryAsync(HttpClient client, string url, int maxRetries = 3)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < maxRetries; i++)
            {
                response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
            }
            return response;
        }

        /// <summary>
        /// Paginates a dictionary of exchange rates.
        /// </summary>
        /// <param name="rates">The dictionary of exchange rates.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The page size for pagination.</param>
        /// <returns>A paginated dictionary of exchange rates.</returns>
        public static Dictionary<string, Dictionary<string, double>> GetPaginatedRates(Dictionary<string, Dictionary<string, double>> rates, int pageNumber, int pageSize)
        {
            return rates.Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
