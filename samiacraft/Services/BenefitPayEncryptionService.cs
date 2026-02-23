using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace samiacraft.Services
{
    /// <summary>
    /// Encryption service for Benefit Pay gateway
    /// Handles AES-256 encryption/decryption as per Benefit Payment Gateway documentation
    /// </summary>
    public class BenefitPayEncryptionService
    {
        // Fixed initialization vector as per Benefit Pay documentation
        private const string FIXED_IV = "PGKEYENCDECIVSPC";

        /// <summary>
        /// Creates AES managed instance with provided resource key and IV
        /// </summary>
        private static AesManaged CreateAes(string resourceKey, string initVector)
        {
            // Validate resource key
            if (string.IsNullOrEmpty(resourceKey))
            {
                throw new ArgumentException(
                    "ResourceKey is empty or not configured. " +
                    "Please add 'BenefitPay:ResourceKey' to appsettings.json with a 32-character encryption key from Benefit Pay portal.");
            }
            
            if (resourceKey.Length != 32)
            {
                throw new ArgumentException(
                    $"ResourceKey must be exactly 32 characters long (got {resourceKey.Length}). " +
                    "Check your 'BenefitPay:ResourceKey' in appsettings.json.");
            }
            
            // Validate IV
            if (initVector.Length != 16)
            {
                throw new ArgumentException(
                    $"Initialization Vector must be exactly 16 characters (got {initVector.Length}).");
            }
            
            var aes = new AesManaged();
            aes.Key = Encoding.UTF8.GetBytes(resourceKey);
            aes.IV = Encoding.UTF8.GetBytes(initVector);
            return aes;
        }

        /// <summary>
        /// Encrypts plain text using AES-256-CBC
        /// </summary>
        public static string Encrypt(string plainText, string resourceKey)
        {
            try
            {
                using (AesManaged aes = CreateAes(resourceKey, FIXED_IV))
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                                sw.Write(plainText);
                            
                            return String.Concat(Array.ConvertAll(ms.ToArray(), x => x.ToString("X2")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts hex-encoded encrypted text using AES-256-CBC
        /// </summary>
        public static string Decrypt(string hexEncryptedText, string resourceKey)
        {
            try
            {
                byte[] encryptedBytes = StringToByteArray(hexEncryptedText);
                
                using (var aes = CreateAes(resourceKey, FIXED_IV))
                {
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor();
                    using (MemoryStream ms = new MemoryStream(encryptedBytes))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts hex string to byte array
        /// </summary>
        private static byte[] StringToByteArray(string hexString)
        {
            var result = new System.Collections.Generic.List<byte>();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                result.Add(Convert.ToByte(hexString.Substring(i, 2), 16));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Parses and decrypts payment response from Benefit Pay
        /// </summary>
        public static BenefitPayResponseData ParseResponse(string encryptedTrandata, string resourceKey)
        {
            try
            {
                var responseData = new BenefitPayResponseData();
                
                if (string.IsNullOrEmpty(encryptedTrandata))
                    return responseData;

                // Decrypt the response
                string decryptedJson = Decrypt(encryptedTrandata, resourceKey);
                
                // Parse JSON array
                JArray jsonArray = JArray.Parse(System.Net.WebUtility.UrlDecode(decryptedJson));
                
                if (jsonArray.Count > 0)
                {
                    var jsonObject = jsonArray[0] as JObject;
                    if (jsonObject != null)
                    {
                        responseData.PaymentID = jsonObject["paymentid"]?.ToString();
                        responseData.Result = jsonObject["result"]?.ToString();
                        responseData.ResponseCode = jsonObject["authRespCode"]?.ToString() ?? jsonObject["authrespcode"]?.ToString();
                        responseData.TransactionID = jsonObject["transid"]?.ToString();
                        responseData.ReferenceID = jsonObject["ref"]?.ToString();
                        responseData.TrackID = jsonObject["trackid"]?.ToString();
                        responseData.Amount = jsonObject["amt"]?.ToString();
                        responseData.AuthCode = jsonObject["authCode"]?.ToString() ?? jsonObject["authcode"]?.ToString();
                        responseData.Date = jsonObject["date"]?.ToString();
                        responseData.ErrorCode = jsonObject["errorCode"]?.ToString() ?? jsonObject["errorcode"]?.ToString();
                        responseData.ErrorText = jsonObject["errorText"]?.ToString() ?? jsonObject["errortext"]?.ToString();
                        
                        // UDF Fields
                        responseData.UDF1 = jsonObject["udf1"]?.ToString();
                        responseData.UDF2 = jsonObject["udf2"]?.ToString();
                        responseData.UDF3 = jsonObject["udf3"]?.ToString();
                        responseData.UDF4 = jsonObject["udf4"]?.ToString();
                        responseData.UDF5 = jsonObject["udf5"]?.ToString();
                        
                        responseData.IsValid = true;
                    }
                }
                
                return responseData;
            }
            catch (Exception ex)
            {
                return new BenefitPayResponseData
                {
                    IsValid = false,
                    ErrorText = $"Failed to parse response: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// Represents decrypted BenefitPay response data
    /// </summary>
    public class BenefitPayResponseData
    {
        public string PaymentID { get; set; }
        public string Result { get; set; }
        public string ResponseCode { get; set; }
        public string TransactionID { get; set; }
        public string ReferenceID { get; set; }
        public string TrackID { get; set; }
        public string Amount { get; set; }
        public string AuthCode { get; set; }
        public string Date { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
        public string UDF1 { get; set; }
        public string UDF2 { get; set; }
        public string UDF3 { get; set; }
        public string UDF4 { get; set; }
        public string UDF5 { get; set; }
        public bool IsValid { get; set; } = false;
    }
}
