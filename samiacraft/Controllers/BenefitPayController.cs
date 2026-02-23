using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using samiacraft.Services;
using samiacraft.Models.BLL;

namespace samiacraft.Controllers
{
    /// <summary>
    /// Controller for BenefitPay payment gateway integration
    /// Handles payment initialization and response processing
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BenefitPayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BenefitPayGatewayService _benefitPayService;

        public BenefitPayController(IConfiguration configuration)
        {
            _configuration = configuration;

            // Initialize BenefitPay service with credentials from configuration
            var tranportalId = _configuration["BenefitPay:TranportalId"];
            var tranportalPassword = _configuration["BenefitPay:TranportalPassword"];
            var resourceKey = _configuration["BenefitPay:ResourceKey"];
            var responseUrl = $"{_configuration["AppSettings:BaseUrl"]}/benefitpay/response";
            var errorUrl = $"{_configuration["AppSettings:BaseUrl"]}/benefitpay/error";
            var isTestMode = _configuration.GetValue<bool>("BenefitPay:IsTestMode", true);

            _benefitPayService = new BenefitPayGatewayService(
                tranportalId,
                tranportalPassword,
                resourceKey,
                responseUrl,
                errorUrl,
                isTestMode
            );
        }

        /// <summary>
        /// Initiates a payment transaction
        /// POST /api/benefitpay/initiate
        /// </summary>
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.TrackId) || request.Amount <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Invalid request parameters"
                    });
                }

                // Initiate payment
                var result = _benefitPayService.InitiatePayment(
                    trackId: request.TrackId,
                    amount: request.Amount,
                    currency: request.Currency ?? "048",
                    cardType: request.CardType ?? "D",
                    udf1: request.OrderID?.ToString() ?? "",
                    udf2: request.CustomerID?.ToString() ?? "",
                    udf3: request.UDF3 ?? "",
                    udf4: request.UDF4 ?? "",
                    udf5: request.UDF5 ?? ""
                );

                if (result.IsSuccessful)
                {
                    return Ok(new
                    {
                        success = true,
                        redirectUrl = result.RedirectUrl,
                        message = "Payment initiated successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Payment initiation failed: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Handles payment response from BenefitPay gateway
        /// POST /api/benefitpay/response
        /// </summary>
        [HttpPost("response")]
        public async Task<IActionResult> ProcessPaymentResponse([FromForm] string trandata)
        {
            try
            {
                if (string.IsNullOrEmpty(trandata))
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Missing payment response data"
                    });
                }

                // Process response
                var result = _benefitPayService.ProcessPaymentResponse(trandata);

                if (result.IsSuccessful)
                {
                    return Ok(new
                    {
                        success = true,
                        paymentId = result.PaymentID,
                        transactionId = result.TransactionID,
                        referenceId = result.ReferenceID,
                        authCode = result.AuthCode,
                        amount = result.Amount,
                        trackId = result.TrackID,
                        message = "Payment processed successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage,
                        result = result.Result,
                        responseCode = result.ResponseCode
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Payment response processing failed: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Handles payment error response from BenefitPay
        /// POST /api/benefitpay/error
        /// </summary>
        [HttpPost("error")]
        public async Task<IActionResult> HandlePaymentError([FromForm] string errorText, [FromForm] string trackid)
        {
            try
            {
                return Ok(new
                {
                    success = false,
                    error = errorText ?? "Payment processing error",
                    trackId = trackid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Error handling failed: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// Request model for payment initiation
    /// </summary>
    public class PaymentInitiationRequest
    {
        public string TrackId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "048"; // AED
        public string CardType { get; set; } = "D"; // Debit
        public int? OrderID { get; set; }
        public int? CustomerID { get; set; }
        public string UDF3 { get; set; }
        public string UDF4 { get; set; }
        public string UDF5 { get; set; }
    }
}
