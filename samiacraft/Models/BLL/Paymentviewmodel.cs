namespace samiacraft.Models.BLL
{
    public class Paymentviewmodel
    {
    }
    public class PaymentRequestModel
    {
        public string Amount { get; set; } = "";
        public string TrackId { get; set; } = "";
        public string Udf2 { get; set; } = "";  // e.g. Order ID
        public string Udf3 { get; set; } = "";  // e.g. Customer Name
        public string Udf4 { get; set; } = "";
        public string Udf5 { get; set; } = "";
    }

    // ── Used by Approved / Declined / Error pages ─────────────────────────────
    public class PaymentResultModel
    {
        public string Status { get; set; } = "";  // "approved" | "declined" | "error"
        public string PaymentId { get; set; } = "";
        public string Result { get; set; } = "";
        public string AuthRespCode { get; set; } = "";
        public string AuthCode { get; set; } = "";
        public string TransId { get; set; } = "";
        public string Ref { get; set; } = "";
        public string Date { get; set; } = "";
        public string TrackId { get; set; } = "";
        public string Amount { get; set; } = "";
        public string Udf1 { get; set; } = "";
        public string Udf2 { get; set; } = "";
        public string Udf3 { get; set; } = "";
        public string Udf4 { get; set; } = "";
        public string Udf5 { get; set; } = "";
        public string ErrorCode { get; set; } = "";
        public string ErrorText { get; set; } = "";
        public string ResponseMsg { get; set; } = "";  // Human-readable decline reason
    }
}
