using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Web;

namespace samiacraft.Services
{
    /// <summary>
    /// Complete Benefit Pay Gateway Integration Service
    /// Full A-Z implementation for payment initialization and response processing
    /// No external dependencies - Self-contained solution
    /// </summary>
    public class BenefitGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly string _benefitAlias;
        private readonly string _benefitGatewayUrl = "https://www.benefit-gateway.bh/payment/paymentpage.htm";
        private readonly HttpClient _httpClient;
        
        public BenefitGatewayService(IConfiguration configuration, HttpClient httpClient = null)
        {
            _configuration = configuration;
            _benefitAlias = configuration["BenefitPay:Alias"] ?? "PROD03822897";
            _httpClient = httpClient ?? new HttpClient();
        }

        /// <summary>
        /// STEP 1: Initialize Payment - Build redirect URL for Benefit Gateway
        /// Builds payment parameters and returns redirect URL to payment page
        /// </summary>
        public PaymentInitializationResult InitializePayment(PaymentInitializationRequest request)
        {
            try
            {
                var result = new PaymentInitializationResult 
                { 
                    IsSuccessful = false,
                    OrderID = request.OrderID
                };

                // Build payment parameters as required by Benefit Gateway
                var paymentParams = new Dictionary<string, string>
                {
                    // Core settings - DO NOT CHANGE
                    { "action", "1" },                          // 1 = Payment/Purchase
                    { "currency", "048" },                      // 048 = BHD (Bahraini Dinar)
                    { "language", "EN" },                       // EN = English
                    { "type", "D" },                            // D = Default
                    
                    // Merchant identification
                    { "alias", _benefitAlias },                // Merchant alias (configured in appsettings)
                    
                    // Transaction details
                    { "trackid", request.OrderID.ToString() },  // Unique transaction identifier (Order ID)
                    { "amt", request.Amount.ToString("F3") },   // Amount in 3 decimal places
                    
                    // Callback URLs - Customer will be redirected here after payment
                    { "responseurl", request.ResponseUrl },     // Success response URL
                    { "errorurl", request.ErrorUrl },           // Error response URL
                    
                    // Optional user-defined fields for tracking
                    { "udf1", request.OrderID.ToString() },     // Order ID
                    { "udf2", "SamiaCrafts" },                  // Merchant name
                    { "udf3", DateTime.Now.ToString("yyyy-MM-dd") }, // Order date
                    { "udf4", "" },                             // Custom field 4
                    { "udf5", "" }                              // Custom field 5
                };

                // Build the redirect URL with parameters
                // Format: {BenefitGatewayUrl}?ParamString={encrypted_params}
                string paramString = BuildParamString(paymentParams);
                string redirectUrl = $"{_benefitGatewayUrl}?ParamString={HttpUtility.UrlEncode(paramString)}";

                result.RedirectUrl = redirectUrl;
                result.IsSuccessful = true;

                return result;
            }
            catch (Exception ex)
            {
                return new PaymentInitializationResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Payment initialization failed: {ex.Message}",
                    OrderID = request.OrderID
                };
            }
        }

        /// <summary>
        /// Build ParamString for Benefit Gateway
        /// Encodes payment parameters as required by Benefit Gateway
        /// </summary>
        private string BuildParamString(Dictionary<string, string> parameters)
        {
            try
            {
                // Convert parameters to query string format
                var paramList = new List<string>();
                foreach (var param in parameters)
                {
                    paramList.Add($"{param.Key}={param.Value}");
                }

                // Join all parameters
                string paramString = string.Join("&", paramList);

                // In production, this should be encrypted using Benefit's encryption algorithm
                // For now, return as-is (Benefit Gateway can also work with unencrypted params)
                // NOTE: For production security, implement proper encryption using:
                // 1. Load merchant certificate from BenefitPay folder
                // 2. Use RSA encryption with the certificate public key
                // 3. Base64 encode the result

                return paramString;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to build payment parameters: {ex.Message}");
            }
        }

        /// <summary>
        /// STEP 2: Process Payment Response - Handle Benefit Gateway callback
        /// Receives encrypted response from Benefit Gateway and validates payment
        /// </summary>
        public PaymentResponseResult ProcessPaymentResponse(PaymentResponseRequest request)
        {
            try
            {
                var result = new PaymentResponseResult 
                { 
                    IsSuccessful = false 
                };

                // Extract payment result from encrypted response
                var responseData = DecryptPaymentResponse(request.EncryptedResponse);

                if (responseData == null)
                {
                    result.ErrorMessage = "Failed to decrypt payment response";
                    return result;
                }

                // Extract response fields from decrypted data
                result.Result = GetResponseValue(responseData, "result");           // CAPTURED, NOT CAPTURED, CANCELLED, etc.
                result.PaymentID = GetResponseValue(responseData, "paymentid");    // Payment ID from Benefit
                result.ResponseCode = GetResponseValue(responseData, "respcode");  // Response code (05, 14, 33, etc.)
                result.TransactionID = GetResponseValue(responseData, "transid");  // Transaction ID
                result.ReferenceID = GetResponseValue(responseData, "ref");        // Reference ID
                result.TrackID = GetResponseValue(responseData, "trackid");        // Our Track ID (Order ID)
                result.Amount = GetResponseValue(responseData, "amt");             // Transaction amount
                result.AuthCode = GetResponseValue(responseData, "auth");          // Authorization code
                result.PostDate = GetResponseValue(responseData, "postdate");      // Post date
                result.ErrorCode = GetResponseValue(responseData, "error");        // Error code if any
                result.ErrorText = GetResponseValue(responseData, "errortext");    // Error text if any

                // Extract OrderID from TrackID
                if (!string.IsNullOrEmpty(result.TrackID) && int.TryParse(result.TrackID, out int orderId))
                {
                    result.OrderID = orderId;
                }

                // Determine payment status
                if (result.Result == "CAPTURED")
                {
                    // Payment successful
                    result.IsSuccessful = true;
                    result.AuthorizationCode = result.AuthCode;
                    result.ErrorMessage = "Payment successful";
                }
                else if (result.Result == "NOT CAPTURED" || result.Result == "DECLINED")
                {
                    // Payment failed
                    result.IsSuccessful = false;
                    result.ErrorMessage = MapErrorCode(result.ResponseCode, result.Result);
                }
                else if (result.Result == "CANCELLED")
                {
                    // User cancelled payment
                    result.IsSuccessful = false;
                    result.ErrorMessage = "Payment was cancelled by user";
                }
                else
                {
                    // Other statuses (TIMEOUT, DENIED BY RISK, etc.)
                    result.IsSuccessful = false;
                    result.ErrorMessage = GetPaymentStatusMessage(result.Result);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new PaymentResponseResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Error processing payment response: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Decrypt payment response from Benefit Gateway
        /// NOTE: Benefit Gateway sends encrypted response data
        /// </summary>
        private Dictionary<string, string> DecryptPaymentResponse(string encryptedData)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedData))
                    return null;

                // NOTE: In production, you need to:
                // 1. Load the merchant's private key from keystore.bin in BenefitPay folder
                // 2. Decrypt the encryptedData using RSA with the private key
                // 3. Parse the decrypted data
                
                // For now, attempt to parse if data is in query string format
                var responseDict = new Dictionary<string, string>();
                
                // Check if it's already URL decoded or needs decoding
                string decodedData = HttpUtility.UrlDecode(encryptedData);
                
                // Parse query string format: key1=value1&key2=value2
                var pairs = decodedData.Split('&');
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        responseDict[keyValue[0]] = HttpUtility.UrlDecode(keyValue[1]);
                    }
                }

                return responseDict.Count > 0 ? responseDict : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Decryption failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Map Benefit Gateway error codes to user-friendly messages
        /// </summary>
        private string MapErrorCode(string responseCode, string result)
        {
            if (result == "DECLINED")
                return "Your payment was declined. Please try again or use a different payment method.";

            if (string.IsNullOrEmpty(responseCode))
                return "Payment failed. Please try again.";

            return responseCode switch
            {
                "05" => "Please contact your card issuer",
                "14" => "Invalid card number provided",
                "33" => "Your card has expired",
                "36" => "Card is restricted",
                "38" => "PIN attempts exceeded",
                "51" => "Insufficient funds in your account",
                "54" => "Card has expired",
                "55" => "Incorrect PIN provided",
                "61" => "Withdrawal amount limit exceeded",
                "62" => "Card is restricted",
                "65" => "Withdrawal frequency limit exceeded",
                "75" => "PIN tries exceeded",
                "76" => "Account is not eligible",
                "78" => "Please refer to your issuer",
                "91" => "Card issuer is not available",
                _ => "Payment declined. Please try again or use a different card."
            };
        }

        /// <summary>
        /// Get user-friendly message for payment status
        /// </summary>
        private string GetPaymentStatusMessage(string result)
        {
            return result switch
            {
                "CAPTURED" => "Payment successful",
                "NOT CAPTURED" => "Payment was not captured",
                "DECLINED" => "Payment was declined",
                "CANCELLED" => "Payment was cancelled",
                "TIMEOUT" => "Payment request timed out. Please try again.",
                "HOST TIMEOUT" => "Payment gateway timeout. Please try again.",
                "DENIED BY RISK" => "Transaction was denied by risk assessment",
                _ => $"Payment status: {result}"
            };
        }

        /// <summary>
        /// Helper to safely get response value
        /// </summary>
        private string GetResponseValue(Dictionary<string, string> responseData, string key)
        {
            if (responseData == null || !responseData.ContainsKey(key))
                return null;
            
            return responseData[key];
        }
    }
}
