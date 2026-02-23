using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BenefitPG.Services
{
    /// <summary>
    /// BENEFIT Payment Gateway - Core Service (.NET 8)
    /// Integration Guide v1.4 | AES/CBC/PKCS7 | IV = PGKEYENCDECIVSPC
    /// UAT:  https://test.benefit-gateway.bh/payment/API/hosted.htm
    /// PROD: https://www.benefit-gateway.bh/payment/API/hosted.htm
    /// </summary>
    public class iPayBenefitPipe
    {
        // ── Constants (DO NOT CHANGE) ─────────────────────────────────────
        public const string AES_IV = "PGKEYENCDECIVSPC";
        public const string UAT_ENDPOINT = "https://test.benefit-gateway.bh/payment/API/hosted.htm";
        public const string PROD_ENDPOINT = "https://www.benefit-gateway.bh/payment/API/hosted.htm";

        // ── Request Fields ────────────────────────────────────────────────
        public string Amt { get; set; } = "";
        public string Action { get; set; } = "1";    // 1=Purchase, 4=Auth
        public string Password { get; set; } = "";
        public string Id { get; set; } = "";     // TranportalID (from bank)
        public string ResourceKey { get; set; } = "";     // Resource Key (from bank)
        public string CurrencyCode { get; set; } = "048";  // 048 = BHD (Bahraini Dinar)
        public string TrackId { get; set; } = "";
        public string Udf1 { get; set; } = "";     // Keep EMPTY per BENEFIT docs
        public string Udf2 { get; set; } = "";
        public string Udf3 { get; set; } = "";
        public string Udf4 { get; set; } = "";
        public string Udf5 { get; set; } = "";
        public string ResponseURL { get; set; } = "";
        public string ErrorURL { get; set; } = "";
        public string Language { get; set; } = "EN";   // EN or AR

        // ── Response Fields (auto-filled after ParseResponse) ─────────────
        public string PaymentId { get; private set; } = "";
        public string Result { get; private set; } = "";
        public string AuthRespCode { get; private set; } = "";
        public string AuthCode { get; private set; } = "";
        public string TransId { get; private set; } = "";
        public string Ref { get; private set; } = "";
        public string Date { get; private set; } = "";
        public string ErrorCode { get; private set; } = "";
        public string ErrorText { get; private set; } = "";

        // ── AES Encryption ────────────────────────────────────────────────
        public static string Encrypt(string plainText, string resourceKey)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(resourceKey);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var enc = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = enc.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToHexString(encrypted); // Uppercase hex - required by BENEFIT
        }

        // ── AES Decryption ────────────────────────────────────────────────
        public static string Decrypt(string hexCipher, string resourceKey)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(resourceKey);
            aes.IV = Encoding.UTF8.GetBytes(AES_IV);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var dec = aes.CreateDecryptor();
            var cipherBytes = Convert.FromHexString(hexCipher);
            var decrypted = dec.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }

        // ── STEP 1: Send Payment Init Request → Get Redirect URL ──────────
        public async Task<PaymentInitResponse> PerformTransactionAsync(bool useProduction = false)
        {
            // Build inner payload (will be encrypted)
            var innerPayload = new List<Dictionary<string, string>>
            {
                new()
                {
                    ["amt"]          = Amt,
                    ["action"]       = Action,
                    ["password"]     = Password,
                    ["id"]           = Id,
                    ["currencycode"] = CurrencyCode,
                    ["trackId"]      = TrackId,
                    ["udf1"]         = Udf1,
                    ["udf2"]         = Udf2,
                    ["udf3"]         = Udf3,
                    ["udf4"]         = Udf4,
                    ["udf5"]         = Udf5,
                    ["responseURL"]  = ResponseURL,
                    ["errorURL"]     = ErrorURL,
                    ["langid"]       = Language,
                }
            };

            // Serialize → URL-encode → AES Encrypt
            var plainJson = JsonSerializer.Serialize(innerPayload);
            var urlEncoded = Uri.EscapeDataString(plainJson);
            var encryptedHex = Encrypt(urlEncoded, ResourceKey);

            // Build outer request wrapper
            var outerRequest = new List<Dictionary<string, string>>
            {
                new() { ["id"] = Id, ["trandata"] = encryptedHex }
            };

            var requestBody = JsonSerializer.Serialize(outerRequest);
            var endpoint = useProduction ? PROD_ENDPOINT : UAT_ENDPOINT;

            using var http = new HttpClient();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var httpResp = await http.PostAsync(endpoint, content);
            var rawJson = await httpResp.Content.ReadAsStringAsync();

            try
            {
                var list = JsonSerializer.Deserialize<List<PaymentInitResponse>>(rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return list?[0] ?? new PaymentInitResponse { RawErrorText = "Empty response from gateway." };
            }
            catch (Exception ex)
            {
                return new PaymentInitResponse { RawErrorText = $"Parse error: {ex.Message}. Raw: {rawJson}" };
            }
        }

        // ── STEP 2: Decrypt trandata Received from BENEFIT PG ────────────
        /// <summary>
        /// Call this in your Response/Notification page when PG POSTs trandata.
        /// Returns true on success. On failure, check ErrorText property.
        /// </summary>
        public bool ParseResponse(string trandata)
        {
            try
            {
                var decrypted = Decrypt(trandata, ResourceKey);
                var urlDecoded = Uri.UnescapeDataString(decrypted);

                var list = JsonSerializer.Deserialize<List<BenefitResponseData>>(urlDecoded,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var d = list?[0];
                if (d is null) return false;

                PaymentId = d.PaymentId ?? "";
                Result = d.Result ?? "";
                AuthRespCode = d.AuthRespCode ?? "";
                AuthCode = d.AuthCode ?? "";
                TransId = d.TransId ?? "";
                Ref = d.Ref ?? "";
                Date = d.Date ?? "";
                ErrorCode = d.ErrorCode ?? "";
                ErrorText = d.ErrorText ?? "";
                Udf1 = d.Udf1 ?? "";
                Udf2 = d.Udf2 ?? "";
                Udf3 = d.Udf3 ?? "";
                Udf4 = d.Udf4 ?? "";
                Udf5 = d.Udf5 ?? "";
                TrackId = d.TrackId ?? "";
                Amt = d.Amt ?? "";
                return true;
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                return false;
            }
        }

        // ── Response Code → Human readable ───────────────────────────────
        public static string GetResponseDescription(string code) => code switch
        {
            "00" => "Approved",
            "05" => "Please contact your card issuer",
            "14" => "Invalid card number",
            "33" => "Expired card",
            "36" => "Restricted card",
            "38" => "Allowable PIN tries exceeded",
            "51" => "Insufficient funds",
            "54" => "Expired card",
            "55" => "Incorrect PIN",
            "61" => "Exceeds withdrawal amount limit",
            "62" => "Restricted card",
            "65" => "Exceeds withdrawal frequency limit",
            "75" => "Allowable number of PIN tries exceeded",
            "76" => "Ineligible account",
            "78" => "Please refer to your card issuer",
            "91" => "Card issuer is currently unavailable",
            _ => "Transaction could not be processed. Please try again or use another card."
        };
    }

    // ── DTOs ──────────────────────────────────────────────────────────────

    public class PaymentInitResponse
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "";
        [JsonPropertyName("result")] public string? Result { get; set; }
        [JsonPropertyName("error")] public string? Error { get; set; }
        [JsonPropertyName("errorText")] public string? RawErrorText { get; set; }

        public bool IsSuccess => Status == "1";
        public string PaymentId => IsSuccess && (Result?.Contains(':') ?? false) ? Result.Split(':')[0] : "";
        public string PaymentPageUrl => IsSuccess && (Result?.Contains(':') ?? false)
            ? string.Join(':', Result.Split(':').Skip(1)) : "";
    }

    public class BenefitResponseData
    {
        [JsonPropertyName("paymentId")] public string? PaymentId { get; set; }
        [JsonPropertyName("result")] public string? Result { get; set; }
        [JsonPropertyName("authRespCode")] public string? AuthRespCode { get; set; }
        [JsonPropertyName("authCode")] public string? AuthCode { get; set; }
        [JsonPropertyName("transId")] public string? TransId { get; set; }
        [JsonPropertyName("ref")] public string? Ref { get; set; }
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
        [JsonPropertyName("errorText")] public string? ErrorText { get; set; }
        [JsonPropertyName("udf1")] public string? Udf1 { get; set; }
        [JsonPropertyName("udf2")] public string? Udf2 { get; set; }
        [JsonPropertyName("udf3")] public string? Udf3 { get; set; }
        [JsonPropertyName("udf4")] public string? Udf4 { get; set; }
        [JsonPropertyName("udf5")] public string? Udf5 { get; set; }
        [JsonPropertyName("trackId")] public string? TrackId { get; set; }
        [JsonPropertyName("amt")] public string? Amt { get; set; }
    }
}