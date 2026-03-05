using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace samiacraft.Services
{
    public class BenefitPayGatewayService
    {
        private const string PRODUCTION_ENDPOINT = "https://www.benefit-gateway.bh/payment/API/hosted.htm";
        private const string TEST_ENDPOINT = "https://test.benefit-gateway.bh/payment/API/hosted.htm";
        private const string AES_IV = "PGKEYENCDECIVSPC";

        private readonly string _tranportalId;
        private readonly string _tranportalPassword;
        private readonly string _resourceKey;
        private readonly string _responseUrl;
        private readonly string _errorUrl;
        private readonly string _cancelUrl;
        private readonly bool _isTestMode;

        // Constructor 1: IConfiguration (DI / Program.cs AddScoped)
        public BenefitPayGatewayService(IConfiguration config)
        {
            var s = config.GetSection("BenefitPay");
            _tranportalId = s["TranportalId"] ?? "";
            _tranportalPassword = s["TranportalPassword"] ?? "";
            _resourceKey = s["ResourceKey"] ?? "";
            _responseUrl = s["ResponseUrl"] ?? "";
            _errorUrl = s["ErrorUrl"] ?? "";
            _isTestMode = s.GetValue<bool>("IsTestMode");
        }

        // Constructor 2: 6 arguments (OrderController manually new karta hai)
        public BenefitPayGatewayService(
            string tranportalId,
            string tranportalPassword,
            string resourceKey,
            string responseUrl,
            string errorUrl,
            bool isTestMode)
        {
            _tranportalId = tranportalId;
            _tranportalPassword = tranportalPassword;
            _resourceKey = resourceKey;
            _responseUrl = responseUrl;
            _errorUrl = errorUrl;
            _isTestMode = isTestMode;
        }

        // ════════════════════════════════════════════════════════════════
        // INITIATE PAYMENT
        // ✅ overrideResponseUrl / overrideErrorUrl - localhost ya production
        // ════════════════════════════════════════════════════════════════
        public BenefitPayPaymentResult InitiatePayment(
            string trackId,
            decimal amount,
            string currency = "048",
            string cardType = "D",
            string udf1 = "",
            string udf2 = "",
            string udf3 = "",
            string udf4 = "",
            string udf5 = "",
            string? overrideResponseUrl = null,   // ✅ Dynamic URL support
            string? overrideErrorUrl = null)      // ✅ Dynamic URL support
        {
            try
            {
                if (_resourceKey.Length != 32)
                    return Fail($"ResourceKey must be 32 chars. Got: {_resourceKey.Length}");

                // ✅ Override URLs use karo agar diye hain, warna default
                string responseUrl = overrideResponseUrl ?? _responseUrl;
                string errorUrl = overrideErrorUrl ?? _errorUrl;

                // ── Step 1: Inner payload ──
                var innerList = new[]
                {
                    new
                    {
                        amt          = amount.ToString("F3"),
                        action       = "1",
                        password     = _tranportalPassword,
                        id           = _tranportalId,
                        resourceKey  = _resourceKey,
                        currencycode = currency,
                        trackId      = trackId,
                        udf1         = "",
                        udf2         = udf2,
                        udf3         = udf3,
                        udf4         = udf4,
                        udf5         = udf5,
                        responseURL  = responseUrl,
                        errorURL     = errorUrl,
                        cancelURL    = _cancelUrl,
                    }
                };

                // ── Step 2: Serialize ──
                var innerJson = JsonConvert.SerializeObject(innerList, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                var encryptedHex = AesEncrypt(innerJson, _resourceKey);

                var outerList = new[]
                {
                    new { id = _tranportalId, trandata = encryptedHex }
                };

                var outerJson = JsonConvert.SerializeObject(outerList, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                });

                // ── Step 3: POST ──
                var endpoint = _isTestMode ? TEST_ENDPOINT : PRODUCTION_ENDPOINT;
                var apiResp = SendRequest(outerJson, endpoint);

                if (apiResp.status == "1" && !string.IsNullOrEmpty(apiResp.result))
                {
                    var firstColon = apiResp.result.IndexOf(':');
                    if (firstColon < 0)
                        return Fail($"Unexpected result format: {apiResp.result}");

                    var paymentId = apiResp.result[..firstColon];
                    var paymentUrl = apiResp.result[(firstColon + 1)..];

                    if (paymentUrl.StartsWith("//"))
                        paymentUrl = "https:" + paymentUrl;

                    return new BenefitPayPaymentResult
                    {
                        IsSuccessful = true,
                        RedirectUrl = paymentUrl,
                        PaymentId = paymentId,
                        PaymentResult = apiResp
                    };
                }

                return Fail(apiResp.errorText ?? apiResp.error ?? "Payment init failed");
            }
            catch (Exception ex)
            {
                return Fail($"Exception: {ex.Message}");
            }
        }

        // ════════════════════════════════════════════════════════════════
        // PARSE RESPONSE - trandata decrypt karo
        // ════════════════════════════════════════════════════════════════
        public BenefitPayDecryptedResponse ParseResponse(string trandata)
        {
            try
            {
                var decrypted = AesDecrypt(trandata, _resourceKey);
                var urlDecoded = Uri.UnescapeDataString(decrypted);

                BenefitPayDecryptedResponse? result = null;
                try
                {
                    var list = JsonConvert.DeserializeObject<List<BenefitPayDecryptedResponse>>(urlDecoded);
                    result = list?[0];
                }
                catch
                {
                    result = JsonConvert.DeserializeObject<BenefitPayDecryptedResponse>(urlDecoded);
                }

                return result ?? new BenefitPayDecryptedResponse { result = "ERROR", errorText = "Deserialize failed" };
            }
            catch (Exception ex)
            {
                return new BenefitPayDecryptedResponse { result = "ERROR", errorText = ex.Message };
            }
        }

        // ════════════════════════════════════════════════════════════════
        // AES ENCRYPT
        // ════════════════════════════════════════════════════════════════
        private static string AesEncrypt(string plainText, string resourceKey)
        {
            using var aes = new AesManaged();
            aes.Key = Encoding.UTF8.GetBytes(resourceKey);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);

            return string.Concat(Array.ConvertAll(ms.ToArray(), x => x.ToString("X2")));
        }

        // ════════════════════════════════════════════════════════════════
        // AES DECRYPT
        // ════════════════════════════════════════════════════════════════
        private static string AesDecrypt(string hexCipher, string resourceKey)
        {
            var cipherBytes = Enumerable.Range(0, hexCipher.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hexCipher.Substring(x, 2), 16))
                .ToArray();

            using var aes = new AesManaged();
            aes.Key = Encoding.UTF8.GetBytes(resourceKey);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        // ════════════════════════════════════════════════════════════════
        // HTTP POST
        // ════════════════════════════════════════════════════════════════
        private static BenefitPayApiResponse SendRequest(string json, string endpoint)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(endpoint);
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.Method = "POST";
                req.Timeout = 30000;

                using var sw = new StreamWriter(req.GetRequestStream());
                sw.WriteLine(json);
                sw.Flush();

                using var resp = (HttpWebResponse)req.GetResponse();
                using var sr = new StreamReader(resp.GetResponseStream());
                var body = sr.ReadToEnd();

                var list = JsonConvert.DeserializeObject<List<BenefitPayApiResponse>>(body);
                return list?[0] ?? new BenefitPayApiResponse { status = "0", errorText = "Empty response" };
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse err)
            {
                using var sr = new StreamReader(err.GetResponseStream());
                return new BenefitPayApiResponse { status = "0", errorText = sr.ReadToEnd() };
            }
            catch (Exception ex)
            {
                return new BenefitPayApiResponse { status = "0", errorText = ex.Message };
            }
        }

        public static string GetDeclineMessage(string code) => code switch
        {
            "05" => "Please contact issuer",
            "14" => "Invalid card number",
            "33" or "54" => "Expired card",
            "36" or "62" => "Restricted card",
            "38" or "75" => "Allowable PIN tries exceeded",
            "51" => "Insufficient funds",
            "55" => "Incorrect PIN",
            "61" => "Exceeds withdrawal amount limit",
            "65" => "Exceeds withdrawal frequency limit",
            "76" => "Ineligible account",
            "78" => "Refer to Issuer",
            "91" => "Issuer is inoperative",
            _ => "Unable to process transaction. Please try again or use another card."
        };

        private static BenefitPayPaymentResult Fail(string msg) =>
            new() { IsSuccessful = false, ErrorMessage = msg };
    }

    // ════════════════════════════════════════════════════════════════════
    // DTOs
    // ════════════════════════════════════════════════════════════════════
    public class BenefitPayPaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string? RedirectUrl { get; set; }
        public string? PaymentId { get; set; }
        public string? ErrorMessage { get; set; }
        public BenefitPayApiResponse? PaymentResult { get; set; }
    }

    public class BenefitPayApiResponse
    {
        [JsonProperty("status")] public string? status { get; set; }
        [JsonProperty("result")] public string? result { get; set; }
        [JsonProperty("error")] public string? error { get; set; }
        [JsonProperty("errorText")] public string? errorText { get; set; }
    }

    public class BenefitPayDecryptedResponse
    {
        [JsonProperty("paymentId")] public string? paymentId { get; set; }
        [JsonProperty("result")] public string? result { get; set; }
        [JsonProperty("authRespCode")] public string? authRespCode { get; set; }
        [JsonProperty("authCode")] public string? authCode { get; set; }
        [JsonProperty("transId")] public string? transId { get; set; }
        [JsonProperty("ref")] public string? @ref { get; set; }
        [JsonProperty("date")] public string? date { get; set; }
        [JsonProperty("trackId")] public string? trackId { get; set; }
        [JsonProperty("amt")] public string? amt { get; set; }
        [JsonProperty("udf1")] public string? udf1 { get; set; }
        [JsonProperty("udf2")] public string? udf2 { get; set; }
        [JsonProperty("udf3")] public string? udf3 { get; set; }
        [JsonProperty("udf4")] public string? udf4 { get; set; }
        [JsonProperty("udf5")] public string? udf5 { get; set; }
        [JsonProperty("errorCode")] public string? errorCode { get; set; }
        [JsonProperty("errorText")] public string? errorText { get; set; }
    }
}