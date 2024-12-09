using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace StockApi.Controllers
{
    [ApiController]
    [Route("api/stock")]
    public class StockController : ControllerBase
    {
        private readonly ApiSettings _apiSettings;  // 用來儲存 ApiSettings 配置
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<StockController> _logger;

        // 注入 IOptions<ApiSettings> 來獲取 appsettings 中的 ApiKey 和 IHttpClientFactory 來創建 HttpClient 實例
        public StockController(IOptions<ApiSettings> apiSettings, IHttpClientFactory httpClientFactory, ILogger<StockController> logger)
        {
            _apiSettings = apiSettings.Value;  // 訪問設定中的 ApiKey
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            // Log the loaded ApiKey
            _logger.LogInformation("Loaded ApiKey: {ApiKey}", _apiSettings.ApiKey);

        }

        private string ApiUrl => $"https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=IBM&interval=5min&apikey={_apiSettings.ApiKey}";

        // GET: api/stock
        [HttpGet]
        public async Task<IActionResult> GetStockData()
        {
            _logger.LogInformation("Starting API request to: {ApiUrl}", ApiUrl);

            // 使用 IHttpClientFactory 創建 HttpClient 實例
            var client = _httpClientFactory.CreateClient();

            try
            {
                // 發送 API 請求並接收響應
                string response = await client.GetStringAsync(ApiUrl);

                // 記錄 API 響應內容
                _logger.LogInformation("Received response from API.");

                // 解析 JSON
                var jsonData = JObject.Parse(response);

                // 提取 "Time Series (5min)" 部分的數據
                var timeSeries = jsonData["Time Series (5min)"] as JObject;

                if (timeSeries == null)
                {
                    _logger.LogWarning("No time series data found in the API response.");
                    return BadRequest("No stock data available.");
                }

                // 將 timeSeries 轉換為字典
                var stockData = timeSeries.ToObject<Dictionary<string, JObject>>();

                // 記錄提取的數據數量
                _logger.LogInformation("Extracted {StockDataCount} data points from the API response.", stockData.Count);

                // 返回數據為 JSON 格式
                return Ok(stockData);
            }
            catch (HttpRequestException ex)
            {
                // 記錄 HTTP 請求錯誤
                _logger.LogError(ex, "API request failed.");
                return StatusCode(500, $"API request failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // 記錄一般錯誤
                _logger.LogError(ex, "An error occurred while processing the request.");
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
