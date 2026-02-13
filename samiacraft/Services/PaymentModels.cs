namespace samiacraft.Services
{
    /// <summary>
    /// Request model for Benefit payment initialization
    /// </summary>
    public class PaymentInitializationRequest
    {
        public int OrderID { get; set; }
        public double Amount { get; set; }
        public string ResponseUrl { get; set; }
        public string ErrorUrl { get; set; }
    }

    /// <summary>
    /// Response model for Benefit payment initialization
    /// </summary>
    public class PaymentInitializationResult
    {
        public bool IsSuccessful { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorMessage { get; set; }
        public int OrderID { get; set; }
    }

    /// <summary>
    /// Request model for Benefit payment response processing
    /// </summary>
    public class PaymentResponseRequest
    {
        public int OrderID { get; set; }
        public string TransactionData { get; set; }
        public string PaymentID { get; set; }
        public string TrackID { get; set; }
        public string ErrorText { get; set; }
        public string EncryptedResponse { get; set; }
    }

    /// <summary>
    /// Response model for Benefit payment response processing
    /// Contains all fields returned by Benefit Gateway
    /// </summary>
    public class PaymentResponseResult
    {
        public bool IsSuccessful { get; set; }
        public int OrderID { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionID { get; set; }
        public string AuthorizationCode { get; set; }
        
        // Benefit Gateway specific fields
        public string PaymentID { get; set; }
        public string Result { get; set; }
        public string ResponseCode { get; set; }
        public string ReferenceID { get; set; }
        public string TrackID { get; set; }
        public string Amount { get; set; }
        public string AuthCode { get; set; }
        public string PostDate { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }
}

