using Newtonsoft.Json.Linq;
using samiacraft.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

    namespace samiacraft.Models.BLL
    {
        public class checkoutBLL
        {
            public int? PaymentMethodID { get; set; }
            public int? OrderID { get; set; }
            public int OrderNo { get; set; }
            public int? CustomerID { get; set; }
            public double? AmountTotal { get; set; }
            public double? GrandTotal { get; set; }
            public double? Tax { get; set; }
            public double? DeliveryAmount { get; set; }
            public double? DiscountAmount { get; set; }
            public int? TotalItems { get; set; }
            public int? StatusID { get; set; }
            public Nullable<System.DateTime> OrderDate { get; set; }
            public Nullable<System.DateTime> LastUpdatedDate { get; set; }
            public int? LastUpdatedBy { get; set; }
            public int? CustOrderInfoID { get; set; }
            public string Address { get; set; }
            public string NearestPlace { get; set; }
            public string Country { get; set; }
            public string ContactNo { get; set; }
            public string DeliveryTime { get; set; }
            public string CustomerName { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
            public string PlaceType { get; set; }
            public string Email { get; set; }
            public string CardNotes { get; set; }
            public string SelectedTime { get; set; }
            public string City { get; set; } // Added City property
            public Nullable<int> DeliveryStatus { get; set; }

        public List<OrderDetails> OrderDetail { get; set; } = new List<OrderDetails>();
            public string OrderDetailString { get; set; }

            /*Order Details*/
            public class OrderDetails
            {
                public int OrderDetailID { get; set; }
                public int? OrderID { get; set; }
                public int ID { get; set; }
                public string Name { get; set; }
                public string ProNote { get; set; }
                public string Image { get; set; }
                public int GiftID { get; set; }
                public int Qty { get; set; }
                public double? Price { get; set; }
                public double? NewPrice { get; set; }
                public double Cost { get; set; }
                public Nullable<System.DateTime> LastUpdatedDate { get; set; }
                public int LastUpdatedBy { get; set; }
                public int DealID { get; set; }
                public int Key { get; set; }
            }

            /*Order Gifts Details*/
            public class OrderGiftDetails
            {
                public int OrderDetailID { get; set; }
                public int ItemID { get; set; }
                public string Title { get; set; }
                public string Image { get; set; }
                public int GiftID { get; set; }
                public int Quantity { get; set; }
                public double DisplayPrice { get; set; }
                public double Cost { get; set; }
                public double DiscountAmount { get; set; }
                public Nullable<System.DateTime> LastUpdatedDate { get; set; }
                public int LastUpdatedBy { get; set; }
                public int ItemKey { get; set; }
            }

            public static DataSet _ds;

            public int InsertOrder(checkoutBLL data)
            {
                try
                {
                    int rtn = 0;
                    var OrderDate = DateTime.UtcNow.AddMinutes(180); // Bahrain time

                    // Null checks
                    if (data == null)
                    {
                        LogError("Data object is null");
                        return 0;
                    }

                    // Parse OrderDetail if string is provided
                    if (!string.IsNullOrEmpty(data.OrderDetailString) && (data.OrderDetail == null || data.OrderDetail.Count == 0))
                    {
                        try
                        {
                            data.OrderDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OrderDetails>>(data.OrderDetailString);
                        }
                        catch (Exception ex)
                        {
                            LogError($"Failed to parse OrderDetailString: {ex.Message}");
                        }
                    }

                    // Validate required fields
                    if (string.IsNullOrEmpty(data.CustomerName) || string.IsNullOrEmpty(data.ContactNo))
                    {
                        LogError("CustomerName or ContactNo is missing");
                        return 0;
                    }

                    SqlParameter[] p = new SqlParameter[23];

                    // ORDER MASTER
                    p[0] = new SqlParameter("@CustomerID", data.CustomerID ?? (object)DBNull.Value);
                    p[1] = new SqlParameter("@AmountTotal", data.AmountTotal ?? 0);
                    p[2] = new SqlParameter("@GrandTotal", data.GrandTotal ?? 0);
                    p[3] = new SqlParameter("@Tax", data.Tax ?? 0);
                    p[4] = new SqlParameter("@AmountDiscount", data.DiscountAmount ?? 0);
                    p[5] = new SqlParameter("@StatusID", 2);
                    p[6] = new SqlParameter("@OrderDate", OrderDate);
                    p[7] = new SqlParameter("@LastUpdatedDate", data.LastUpdatedDate ?? OrderDate);
                    p[8] = new SqlParameter("@LastUpdateBy", data.LastUpdatedBy ?? 0);


                    // CUSTOMER ORDER INFO
                    p[9] = new SqlParameter("@Address", data.Address ?? "");
                    p[10] = new SqlParameter("@NearestPlace", data.NearestPlace ?? "");
                    p[11] = new SqlParameter("@Country", data.Country ?? "Bahrain");
                    p[12] = new SqlParameter("@ContactNo", data.ContactNo ?? "");
                    p[13] = new SqlParameter("@DeliveryTime", data.DeliveryTime ?? "");
                    p[14] = new SqlParameter("@CustomerName", data.CustomerName ?? "");
                    p[15] = new SqlParameter("@Latitude", data.Latitude ?? "0");
                    p[16] = new SqlParameter("@Longitude", data.Longitude ?? "0");
                    p[17] = new SqlParameter("@PlaceType", data.PlaceType ?? "House");
                    p[18] = new SqlParameter("@Email", data.Email ?? "");
                    p[19] = new SqlParameter("@CardNotes", data.CardNotes ?? "");
                    p[20] = new SqlParameter("@ServiceCharges", data.DeliveryAmount ?? 0);
                    p[21] = new SqlParameter("@LocationID", 2148);
                    p[22] = new SqlParameter("@DeliveryStatus", data.DeliveryStatus);
                    // Insert Order
                    var result = new DBHelper().GetTableFromSP("sp_InsertOrder_Website", p);

                    if (result == null || result.Rows.Count == 0)
                    {
                        LogError("sp_InsertOrder_Website returned no results");
                        return 0;
                    }

                    int OrderID = int.Parse(result.Rows[0]["ID"].ToString());
                    rtn = OrderID;

                    // Insert Payment
                    try
                    {
                        SqlParameter[] pay = new SqlParameter[6];
                        pay[0] = new SqlParameter("@OrderID", OrderID);
                        pay[1] = new SqlParameter("@CashPayment", data.GrandTotal ?? 0);
                        pay[2] = new SqlParameter("@CardPayment", 0);
                        pay[3] = new SqlParameter("@CreditPayment", 0);
                        pay[4] = new SqlParameter("@PaymentType", GetPaymentTypeName(data.PaymentMethodID));
                        pay[5] = new SqlParameter("@Total", data.GrandTotal ?? 0);

                        new DBHelper().ExecuteNonQueryReturn("sp_InsertPayment_Vitamito", pay);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Payment insertion failed: {ex.Message}");
                    }

                    // Deduct Stock
                    try
                    {
                        if (data.OrderDetail != null && data.OrderDetail.Count > 0)
                        {
                            foreach (var item in data.OrderDetail)
                            {
                                SqlParameter[] dst = new SqlParameter[4];
                                dst[0] = new SqlParameter("@ItemID", item.ID);
                                dst[1] = new SqlParameter("@LocationID", 2195); // Your location ID
                                dst[2] = new SqlParameter("@Quantity", item.Qty);
                                dst[3] = new SqlParameter("@LastUpdatedDate", OrderDate);

                                new DBHelper().GetTableFromSP("sp_DeductStockAdmin", dst);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Stock deduction failed: {ex.Message}");
                    }

                    // Insert Order Details
                    try
                    {
                        if (data.OrderDetail != null && data.OrderDetail.Count > 0)
                        {
                            int OrderDetailID = 0;
                            foreach (var item in data.OrderDetail)
                            {
                                double? discounted = 0;
                                if (item.NewPrice != null && item.NewPrice > 0)
                                {
                                    discounted = item.Price - item.NewPrice;
                                }

                                SqlParameter[] para = new SqlParameter[7];
                                para[0] = new SqlParameter("@OrderID", OrderID);
                                para[1] = new SqlParameter("@ItemID", item.ID);
                                para[2] = new SqlParameter("@Quantity", item.Qty);
                                para[3] = new SqlParameter("@Price", item.Price ?? 0);
                                para[4] = new SqlParameter("@DiscountPrice", discounted ?? 0);
                                para[5] = new SqlParameter("@LastUpdateDT", OrderDate);
                                para[6] = new SqlParameter("@LastUpdateBy", data.LastUpdatedBy ?? 0);

                                var detailResult = new DBHelper().GetTableFromSP("sp_OrderDetails_Vitamito_V2", para);
                                if (detailResult != null && detailResult.Rows.Count > 0)
                                {
                                    OrderDetailID = int.Parse(detailResult.Rows[0]["ID"].ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Order details insertion failed: {ex.Message}");
                    }

                    return rtn;
                }
                catch (Exception ex)
                {
                    LogError($"InsertOrder failed: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    return 0;
                }
            }

            public int OrderUpdate(int OrderID, int StatusID)
            {
                try
                {
                    int rtn = 0;
                    SqlParameter[] p = new SqlParameter[2];
                    p[0] = new SqlParameter("@OrderID", OrderID);
                    p[1] = new SqlParameter("@StatusID", StatusID);
                    rtn = new DBHelper().ExecuteNonQueryReturn("sp_OrderReject", p);
                    return rtn;
                }
                catch (Exception ex)
                {
                    LogError($"OrderUpdate failed: {ex.Message}");
                    return 0;
                }
            }

            private string GetPaymentTypeName(int? paymentMethodID)
            {
                switch (paymentMethodID)
                {
                    case 1: return "Cash";
                    case 2: return "Credimax";
                    case 3: return "BenefitPay";
                    case 4: return "Mastercard";
                    case 5: return "Bank Transfer";
                    default: return "Cash";
                }
            }

            private void LogError(string message)
            {
                try
                {
                    // ASP.NET Core compatible error logging
                    string errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

                    if (!Directory.Exists(errorPath))
                    {
                        Directory.CreateDirectory(errorPath);
                    }

                    string errorFile = Path.Combine(errorPath, $"error_{DateTime.Now:yyyyMMdd}.txt");

                    using (StreamWriter writer = new StreamWriter(errorFile, true))
                    {
                        writer.WriteLine("-----------------------------------------------------------------------------");
                        writer.WriteLine($"Date: {DateTime.Now}");
                        writer.WriteLine($"Message: {message}");
                        writer.WriteLine("-----------------------------------------------------------------------------");
                    }
                }
                catch
                {
                    // Silent fail - don't let logging crash the application
                }
            }
        }
    }