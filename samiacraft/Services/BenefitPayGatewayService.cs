using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace samiacraft.Services
{
    /// <summary>
    /// Complete Benefit Pay Gateway service
    /// Implements payment initialization, processing, and response handling as per official documentation
    /// </summary>
    public class BenefitPayGatewayService
    {
        // Benefit Pay Production Endpoint
        private const string PRODUCTION_ENDPOINT = "https://live.benefit-gateway.bh/payment/API/hosted.htm";
        
        // Benefit Pay Test Endpoint
        private const string TEST_ENDPOINT = "https://test.benefit-gateway.bh/payment/API/hosted.htm";

        private readonly string _tranportalId;
        private readonly string _tranportalPassword;
        private readonly string _resourceKey;
        private readonly bool _isTestMode;
        private readonly string _responseUrl;
        private readonly string _errorUrl;

        public BenefitPayGatewayService(
            string tranportalId,
            string tranportalPassword,
            string resourceKey,
            string responseUrl,
            string errorUrl,
            bool isTestMode = true)
        {
            _tranportalId = tranportalId;
            _tranportalPassword = tranportalPassword;
            _resourceKey = resourceKey;
            _responseUrl = responseUrl;
            _errorUrl = errorUrl;
            _isTestMode = isTestMode;
        }

        /// <summary>
        /// Initiates a payment transaction with Benefit Pay Gateway
        /// </summary>
        public BenefitPayPaymentResult InitiatePayment(
            string trackId,
            decimal amount,
            string currency = "048",
            string cardType = "D",
            string udf1 = "",
            string udf2 = "",
            string udf3 = "",
            string udf4 = "",
            string udf5 = "")
        {
            try
            {
                // Validate that all required credentials are configured
                var missingFields = new List<string>();
                if (string.IsNullOrEmpty(_tranportalId))
                    missingFields.Add("TranportalId");
                if (string.IsNullOrEmpty(_tranportalPassword))
                    missingFields.Add("TranportalPassword");
                if (string.IsNullOrEmpty(_resourceKey))
                    missingFields.Add("ResourceKey");
                if (string.IsNullOrEmpty(_responseUrl))
                    missingFields.Add("ResponseUrl");
                if (string.IsNullOrEmpty(_errorUrl))
                    missingFields.Add("ErrorUrl");
                
                if (missingFields.Count > 0)
                {
                    return new BenefitPayPaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"BenefitPay configuration incomplete. Missing: {string.Join(", ", missingFields)}. " +
                                     "Configure these in appsettings.json under 'BenefitPay' section."
                    };
                }
                
                // Validate ResourceKey length (must be exactly 32 characters for AES-256)
                if (_resourceKey.Length != 32)
                {
                    return new BenefitPayPaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = $"ResourceKey must be exactly 32 characters (got {_resourceKey.Length}). " +
                                     "Check 'BenefitPay:ResourceKey' in appsettings.json and ensure it's the correct encryption key from Benefit Pay."
                    };
                }
                
                // Create request object
                var paymentRequest = new BenefitPayRequest
                {
                    id = _tranportalId,
                    amt = amount.ToString("F3"),
                    action = "1",
                    password = _tranportalPassword,
                    resourceKey = _resourceKey,
                    currencycode = currency,
                    trackId = trackId,
                    udf1 = udf1,
                    udf2 = udf2,
                    udf3 = udf3,
                    udf4 = udf4,
                    udf5 = udf5,
                    cardType = cardType,
                    responseURL = _responseUrl,
                    errorURL = _errorUrl
                };

                // Serialize to JSON
                var requestJson = JsonConvert.SerializeObject(new { id = _tranportalId });
                var trandata = JsonConvert.SerializeObject(new[] { paymentRequest }, 
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                // Encrypt the transaction data
                string encryptedTrandata = BenefitPayEncryptionService.Encrypt(trandata, _resourceKey);

                // Create final request
                var finalRequest = JsonConvert.SerializeObject(new[] {
                    new {
                        id = _tranportalId,
                        trandata = encryptedTrandata
                    }
                }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                // Send to Benefit Pay
                var response = SendRequest(finalRequest);

                if (response.status == "1")
                {
                    return new BenefitPayPaymentResult
                    {
                        IsSuccessful = true,
                        RedirectUrl = response.result,
                        PaymentResult = response
                    };
                }
                else
                {
                    return new BenefitPayPaymentResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = response.errorText ?? "Payment initialization failed",
                        PaymentResult = response
                    };
                }
            }
            catch (Exception ex)
            {
                return new BenefitPayPaymentResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Payment initialization error: {ex.Message}",
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Processes payment response from Benefit Pay
        /// </summary>
        public BenefitPayResponseResult ProcessPaymentResponse(string encryptedTrandata)
        {
            try
            {
                var responseData = BenefitPayEncryptionService.ParseResponse(encryptedTrandata, _resourceKey);

                if (!responseData.IsValid)
                {
                    return new BenefitPayResponseResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = responseData.ErrorText ?? "Invalid response received"
                    };
                }

                // Map response data
                var result = new BenefitPayResponseResult
                {
                    IsSuccessful = responseData.Result == "CAPTURED",
                    PaymentID = responseData.PaymentID,
                    Result = responseData.Result,
                    ResponseCode = responseData.ResponseCode,
                    TransactionID = responseData.TransactionID,
                    ReferenceID = responseData.ReferenceID,
                    TrackID = responseData.TrackID,
                    Amount = responseData.Amount,
                    AuthCode = responseData.AuthCode,
                    PostDate = responseData.Date,
                    ErrorCode = responseData.ErrorCode,
                    ErrorText = responseData.ErrorText,
                    UDF1 = responseData.UDF1,
                    UDF2 = responseData.UDF2,
                    UDF3 = responseData.UDF3,
                    UDF4 = responseData.UDF4,
                    UDF5 = responseData.UDF5
                };

                // Set friendly error message
                if (!result.IsSuccessful)
                {
                    result.ErrorMessage = GetFriendlyErrorMessage(responseData.ResponseCode, responseData.Result, responseData.ErrorText);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new BenefitPayResponseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Payment response processing error: {ex.Message}",
                    Exception = ex.ToString(),
                };
            }
        }

        /// <summary>
        /// Sends HTTP request to Benefit Pay Gateway
        /// </summary>
        private BenefitPayApiResponse SendRequest(string jsonPayload)
        {
            try
            {
                string endpoint = _isTestMode ? TEST_ENDPOINT : PRODUCTION_ENDPOINT;
                
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(endpoint);
                webRequest.ContentType = "application/json";
                webRequest.Accept = "application/json";
                webRequest.Method = "POST";
                webRequest.Timeout = 30000; // 30 seconds

                // Send request
                using (StreamWriter requestStream = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestStream.WriteLine(jsonPayload);
                }

                // Get response
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (StreamReader responseStream = new StreamReader(webResponse.GetResponseStream()))
                    {
                        string responseString = responseStream.ReadToEnd();
                        List<BenefitPayApiResponse> responses = JsonConvert.DeserializeObject<List<BenefitPayApiResponse>>(responseString);
                        
                        return responses != null && responses.Count > 0 ? responses[0] : new BenefitPayApiResponse
                        {
                            status = "0",
                            errorText = "Empty response from payment gateway"
                        };
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        return new BenefitPayApiResponse
                        {
                            status = "0",
                            errorText = $"HTTP {response.StatusCode}: {responseText}"
                        };
                    }
                }
                return new BenefitPayApiResponse
                {
                    status = "0",
                    errorText = $"Network error: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new BenefitPayApiResponse
                {
                    status = "0",
                    errorText = $"Request error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Maps Benefit Pay error codes to friendly messages
        /// </summary>
        private string GetFriendlyErrorMessage(string responseCode, string result, string errorText)
        {
            if (!string.IsNullOrEmpty(errorText))
                return errorText;

            // Map response codes to messages per Benefit Pay documentation
            var errorMessages = new Dictionary<string, string>
            {
                { "05", "Please contact your card issuer" },
                { "14", "Invalid card number" },
                { "33", "Expired card" },
                { "36", "Restricted card" },
                { "51", "Insufficient funds" },
                { "54", "Expired card" },
                { "55", "Incorrect PIN" },
                { "61", "Card limit exceeded" },
                { "62", "Restricted card" },
                { "63", "Security violation" },
                { "65", "Card limit exceeded" },
                { "75", "Too many PIN attempts" },
                { "76", "Invalid/nonexistent recipient account" },
                { "78", "Account number not on file" },
                { "91", "Card issuer unavailable" },
                { "92", "Invalid routing destination" },
                { "93", "Cannot complete - violation" },
                { "96", "System error" },
                { "98", "Exceeds issuer withdrawal limit" },
                { "99", "General error" }
            };

            if (!string.IsNullOrEmpty(responseCode) && errorMessages.TryGetValue(responseCode, out string message))
                return message;

            if (result == "NOT CAPTURED")
                return "Payment was not captured. Please try again or contact support.";
            if (result == "CANCELED")
                return "Payment was canceled by user.";
            if (result == "DENIED BY RISK")
                return "Payment was denied by risk management system.";
            if (result == "HOST TIMEOUT")
                return "Payment gateway timeout. Please try again.";

            return "Payment processing failed. Please try again or contact support.";
        }
    }

    /// <summary>
    /// Payment initialization request format for Benefit Pay
    /// </summary>
    public class BenefitPayRequest
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("amt")]
        public string amt { get; set; }

        [JsonProperty("action")]
        public string action { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("resourceKey")]
        public string resourceKey { get; set; }

        [JsonProperty("currencycode")]
        public string currencycode { get; set; }

        [JsonProperty("trackid")]
        public string trackId { get; set; }

        [JsonProperty("udf1")]
        public string udf1 { get; set; }

        [JsonProperty("udf2")]
        public string udf2 { get; set; }

        [JsonProperty("udf3")]
        public string udf3 { get; set; }

        [JsonProperty("udf4")]
        public string udf4 { get; set; }

        [JsonProperty("udf5")]
        public string udf5 { get; set; }

        [JsonProperty("cardType")]
        public string cardType { get; set; }

        [JsonProperty("responseURL")]
        public string responseURL { get; set; }

        [JsonProperty("errorURL")]
        public string errorURL { get; set; }
    }

    /// <summary>
    /// Response from Benefit Pay API
    /// </summary>
    public class BenefitPayApiResponse
    {
        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("result")]
        public string result { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("errorText")]
        public string errorText { get; set; }
    }

    /// <summary>
    /// Result of payment initialization
    /// </summary>
    public class BenefitPayPaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorMessage { get; set; }
        public BenefitPayApiResponse PaymentResult { get; set; }
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Result of payment response processing
    /// </summary>
}
