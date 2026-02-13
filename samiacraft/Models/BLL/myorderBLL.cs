using samiacraft.Helpers;
using samiacraft.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace samiacraft.Models.BLL
{
    public class myorderBLL
    {
        public string PaymentMethodTitle { get; set; }
        public int? PaymentMethodID { get; set; }
        public int? OrderID { get; set; }
        public string OrderNo { get; set; }
        public int? CustomerID { get; set; }
        public double? AmountTotal { get; set; }
        public double? GrandTotal { get; set; }
        public double? Tax { get; set; }
        public double? DeliveryAmount { get; set; }
        public double? DiscountAmount { get; set; }
        public int? TotalItems { get; set; }
        public int? StatusID { get; set; }
        public int? DeliveryStatus { get; set; }
        public int? OrderStatus { get; set; }
        [DisplayFormat(DataFormatString = "{0: dd, MMM, yyyy}")]
        public DateTime OrderDate { get; set; }
        [DisplayFormat(DataFormatString = "{0: dd, MMM, yyyy}")]
        public DateTime? CheckoutDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
        /*Cust Order Info*/
        public string Address { get; set; }
        public string NearestPlace { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ContactNo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string CustomerName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string PlaceType { get; set; }
        public string Email { get; set; }
        public string CardNotes { get; set; }
        public string SelectedTime { get; set; }
        public int? Row_Counter { get; set; }
        public List<OrderDetails> OrderDetail = new List<OrderDetails>();
        /*Order Details*/
        public class OrderDetails
        {
            public int OrderDetailID { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public int Quantity { get; set; }
            public int OrderNo { get; set; }
            public double Price { get; set; }
            public double? DiscountPrice { get; set; }
        }
        public class GiftDetails
        {
            public int OrderDetailID { get; set; }
            public string GiftTitle { get; set; }
            public string GiftImage { get; set; }
            public int Quantity { get; set; }
            public double Price { get; set; }
        }

        public static DataTable _dt;
        public static DataSet _ds;
        public List<myorderBLL> GetAll(int CustomerID)
        {
            try
            {
                var lst = new List<myorderBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@CustomerID", CustomerID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetMyOrders_Vitamito", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        //lst = _dt.DataTableToList<myorderBLL>();
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<myorderBLL>>().ToList();
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<myorderBLL> GetById(int OrderID)
        {
            try
            {
                var lst = new List<myorderBLL>();
                SqlParameter[] p = new SqlParameter[1];
                p[0] = new SqlParameter("@OrderID", OrderID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetMyOrdersById_Vitamito", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        //lst = _dt.DataTableToList<myorderBLL>();
                        lst = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<myorderBLL>>().ToList();
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public myorderBLL GetDetails(int? OrderID)
        {
            try
            {
                var obj = new myorderBLL();
                List<OrderDetails> lstIM = new List<OrderDetails>();
                List<GiftDetails> giftList = new List<GiftDetails>();
                SqlParameter[] p = new SqlParameter[1];

                p[0] = new SqlParameter("@OrderID", OrderID);
                _ds = (new DBHelper().GetDatasetFromSP)("sp_GetMyOrderDetails_Vitamito_V2", p);
                if (_ds != null)
                {
                    if (_ds.Tables.Count > 0)
                    {
                        if (_ds.Tables[0] != null)
                        {
                            obj = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[0])).ToObject<List<myorderBLL>>().FirstOrDefault();
                        }
                        if (_ds.Tables[1] != null)
                        {
                            lstIM = JArray.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(_ds.Tables[1])).ToObject<List<OrderDetails>>();
                        }
                        obj.OrderDetail = lstIM;
                    }
                }
                return obj;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}