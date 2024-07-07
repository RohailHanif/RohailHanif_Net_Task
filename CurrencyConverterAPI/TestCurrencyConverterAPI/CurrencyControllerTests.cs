using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using Moq.Protected;
using System.Threading;
using System.Net;
using CurrencyConverterAPI.Controllers;
using CurrencyConverterAPI.Models;
using System.Text.Json;
using System.Collections.Generic;
using System;

public class CurrencyControllerTests
{
    private readonly CurrencyController _controller;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

    public CurrencyControllerTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _controller = new CurrencyController(_mockHttpClientFactory.Object);
    }
    #region GetLatestRates Test case
    [Fact]
    public async Task GetLatestRates_ReturnsOkResult_WithRates()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"rates\":{\"USD\":1.1}}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var result = await _controller.GetLatestRates("EUR");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var rates = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, double>>>(okResult.Value.ToString());
        Assert.NotNull(rates);
        Assert.Equal(1.1, rates["rates"]["USD"]);
    }
    #endregion
    #region ConvertCurrency Test Case
    [Fact]
    public async Task ConvertCurrency_ReturnsOkResult_WithConvertedAmount()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"rates\":{\"USD\":1.1},\"amount\":110}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var request = new ConversionRequest { FromCurrency = "EUR", ToCurrency = "USD", Amount = 100 };

        var result = await _controller.ConvertCurrency(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var jsonElement = (JsonElement)okResult.Value;
        var convertedAmount = jsonElement.GetProperty("amount").GetDouble();
        Assert.Equal(110, convertedAmount);
    }

    [Fact]
    public async Task ConvertCurrency_ReturnsBadRequest_ForExcludedCurrencies()
    {
        var request = new ConversionRequest { FromCurrency = "TRY", ToCurrency = "USD", Amount = 100 };

        var result = await _controller.ConvertCurrency(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal("Currency conversion not supported for TRY, PLN, THB, and MXN.", badRequestResult.Value);
    }
    #endregion
    #region GetHistoricalRates Test Case
    public async Task GetHistoricalRates_ReturnsOkResult_WithPaginatedRates()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"amount\": 1, \"base\": \"EUR\", \"start_date\": \"2020-01-01\", \"end_date\": \"2020-01-31\", \"rates\": { \"2020-01-01\": { \"USD\": 1.1 }, \"2020-01-02\": { \"USD\": 1.2 } } }")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var historicalRequest = new HistoricalRatesRequest
        {
            Base = "USD",
            StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2020, 1, 31)
        };

        var result = await _controller.GetHistoricalRates(historicalRequest, page: 1, pageSize: 1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var paginatedResponse = okResult.Value as ExchangeRates;
        Assert.NotNull(paginatedResponse);
        Assert.Equal(1, paginatedResponse.Rates.Count);
    }
    [Fact]
    public async Task GetHistoricalRates_ReturnsBadRequest_WhenResponseIsNull()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Bad Request"
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var historicalRequest = new HistoricalRatesRequest
        {
            Base = "USD",
            StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2020, 1, 31)
        };

        var result = await _controller.GetHistoricalRates(historicalRequest, page: 1, pageSize: 10);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        Assert.Equal("Bad Request", objectResult.Value);
    }
    #endregion
}
