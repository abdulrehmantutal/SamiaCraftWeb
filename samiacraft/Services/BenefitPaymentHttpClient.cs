using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace samiacraft.Services
{
    /// <summary>
    /// HTTP client for calling Benefit Gateway payment service from old .NET Framework application
    /// Provides compatibility layer between .NET 8.0 and legacy ASP.NET Framework
    /// </summary>
    public class BenefitPaymentHttpClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public BenefitPaymentHttpClient(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _baseUrl = configuration["BenefitPay:ServiceUrl"] ?? "https://localhost:7050";
        }

        /// <summary>
        /// Initializes payment by calling the old PremiumPOS Web API
        /// </summary>
        public async Task<PaymentInitializationResult> InitializePaymentAsync(PaymentInitializationRequest request)
        {
            try
            {
                var payload = new
                {
                    OrderID = request.OrderID,
                    Amount = request.Amount,
                    ResponseUrl = request.ResponseUrl,
                    ErrorUrl = request.ErrorUrl
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/api/BenefitPayment/Initialize",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<PaymentInitializationResult>(responseContent);
                    return result ?? new PaymentInitializationResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Invalid response from payment service"
                    };
                }
                else
                {
                    return new PaymentInitializationResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"Payment service error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaymentInitializationResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Payment initialization failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Processes payment response by calling the old PremiumPOS Web API
        /// </summary>
        public async Task<PaymentResponseResult> ProcessPaymentResponseAsync(PaymentResponseRequest request)
        {
            try
            {
                var payload = new
                {
                    TransactionData = request.TransactionData,
                    PaymentID = request.PaymentID,
                    TrackID = request.TrackID,
                    ErrorText = request.ErrorText
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/api/BenefitPayment/ProcessResponse",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<PaymentResponseResult>(responseContent);
                    return result ?? new PaymentResponseResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Invalid response from payment service"
                    };
                }
                else
                {
                    return new PaymentResponseResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"Payment service error: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PaymentResponseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Payment response processing failed: {ex.Message}"
                };
            }
        }
    }
}
