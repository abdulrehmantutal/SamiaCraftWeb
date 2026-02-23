using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace samiacraft.Services
{
    /// <summary>
    /// BenefitPay Integration Service - Alternative approach without IKVM compatibility issues
    /// Uses direct HTTP integration with BenefitPay gateway
    /// </summary>
    public class BenefitPayDirectService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public BenefitPayDirectService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Initialize BenefitPay payment using direct HTTP integration
        /// Alternative to iPayBenefitPipe.dll which has .NET 8 compatibility issues
        /// </summary>
        public BenefitPayInitResult InitializePaymentDirect(int orderID, double grandTotal, string orderNo)
        {
            var result = new BenefitPayInitResult();

            try
            {
                // Get BenefitPay configuration
                string alias = _configuration["BenefitPay:Alias"] ?? "PROD03822897";
                string baseUrl = _configuration["AppSettings:WebsiteURL"] ?? "https://www.samiacrafts.com";

                // Build BenefitPay redirect URL with parameters
                string responseUrl = $"{baseUrl}/Order/BenefitPayResponse?OrderID={orderID}";
                string errorUrl = $"{baseUrl}/Order/BenefitPayResponse?OrderID=0";

                // Build payment initialization URL
                // Note: This is a simplified approach - you may need actual BenefitPay gateway URL
                string benefitPayGatewayUrl = BuildBenefitPayUrl(
                    alias,
                    orderID.ToString(),
                    grandTotal,
                    responseUrl,
                    errorUrl
                );

                result.Success = true;
                result.PaymentURL = benefitPayGatewayUrl;
                result.Message = "Payment session created successfully";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Message = "Failed to initialize payment";
            }

            return result;
        }

        /// <summary>
        /// Build BenefitPay gateway URL with parameters
        /// This is a placeholder - actual implementation depends on BenefitPay documentation
        /// </summary>
        private string BuildBenefitPayUrl(string alias, string trackId, double amount, string responseUrl, string errorUrl)
        {
            // This is a simplified example
            // You'll need to replace this with actual BenefitPay gateway URL format

            string gatewayUrl = "https://www.benefit-gateway.bh/payment"; // Placeholder

            var queryParams = new StringBuilder();
            queryParams.Append($"?alias={Uri.EscapeDataString(alias)}");
            queryParams.Append($"&trackid={Uri.EscapeDataString(trackId)}");
            queryParams.Append($"&amount={amount:F3}");
            queryParams.Append($"&currency=048");
            queryParams.Append($"&language=EN");
            queryParams.Append($"&responseURL={Uri.EscapeDataString(responseUrl)}");
            queryParams.Append($"&errorURL={Uri.EscapeDataString(errorUrl)}");

            return gatewayUrl + queryParams.ToString();
        }

        /// <summary>
        /// Parse BenefitPay response (manual parsing without iPayBenefitPipe)
        /// </summary>
        public BenefitPayResponseResult ParseResponse(IFormCollection formData)
        {
            var result = new BenefitPayResponseResult();

            try
            {
                // Manual parsing of response fields
                if (formData.ContainsKey("result"))
                {
                    result.Result = formData["result"].ToString();
                    result.ResponseCode = formData["auth_code"].ToString();
                    result.TrackID = formData["trackid"].ToString();
                    result.TransactionID = formData["tranid"].ToString();
                    result.Amount = formData["amt"].ToString();
                    result.ErrorText = formData["error_text"].ToString();
                    result.IsSuccessful = result.Result == "CAPTURED";
                }
                else if (formData.ContainsKey("ErrorText"))
                {
                    result.ErrorText = formData["ErrorText"].ToString();
                    result.TrackID = formData["Trackid"].ToString();
                    result.IsSuccessful = false;
                }
                else
                {
                    result.IsSuccessful = false;
                    result.ErrorText = "Unknown response format";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorText = ex.Message;
            }

            return result;
        }
    }

    #region Result Models

    public class BenefitPayInitResult
    {
        public bool Success { get; set; }
        public string PaymentURL { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BenefitPayResponseResult
    {
        public bool IsSuccessful { get; set; }
        //public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Exception { get; set; }
        public string PaymentID { get; set; }
        public string Result { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionID { get; set; }
        public string ReferenceID { get; set; }
        public string TrackID { get; set; }
        public string Amount { get; set; }
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        public string UDF4 { get; set; }
        public string UDF5 { get; set; }
        public string AuthCode { get; set; }
        public string PostDate { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }

    #endregion
}