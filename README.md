# CurrencyConverterAPI

This project is a simple currency converter API built with ASP.NET Core. It retrieves the latest exchange rates, converts amounts between different currencies, and provides historical exchange rates with pagination.

## How to Run the Application

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or any other preferred IDE

### Steps to Run
1. Clone the repository:
    ```bash
    git clone https://github.com/RohailHanif/RohailHanif_Net_Task
    cd CurrencyConverterAPI
    ```

2. Restore the dependencies and build the project:
    ```bash
    dotnet restore
    dotnet build
    ```

3. Run the application:
    ```bash
    dotnet run
    ```

4. The API will be available at `https://localhost:5001` (or the port specified in your launch settings).

### Endpoints

#### Retrieve Latest Exchange Rates
GET /api/currency/latest/{baseCurrency}

### Convert Currency
Body:
```json
GET /api/currency/convert
```
Body:
```json
{
  "amount": 100,
  "fromCurrency": "EUR",
  "toCurrency": "USD"
}
```

### Retrieve Historical Exchange Rates

GET /api/currency/convert
Body:
```json
/api/Currency/historical?Base=USD&StartDate=2020-01-01&EndDate=2020-01-31&page=1&pageSize=10 
```

### Assumptions
-	The base URL for the Frankfurter API is https://api.frankfurter.app/.
-	The application retries failed requests up to 3 times.
### Enhancements and Changes
Given more time, the following enhancements and changes could be made:
-	Caching: Implement caching to reduce the number of calls to the Frankfurter API.
-	Rate Limiting: Add rate limiting to prevent overloading the Frankfurter API.
-	Detailed Error Handling: Improve error handling with more detailed error messages.
-	Authentication: Add authentication and authorization for accessing the API.
-	Deployment: Set up CI/CD pipelines for automated testing and deployment.
