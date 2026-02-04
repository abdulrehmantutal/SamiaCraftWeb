using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AlHilal.Models
{
    public class HblPayModel
    {
    }
    public class CheckoutModelEnc
    {
        public string USER_ID { get; set; }
        public string PASSWORD { get; set; }
        public string RETURN_URL { get; set; }
        public string CANCEL_URL { get; set; }
        public string CHANNEL { get; set; }
        public string TYPE_ID { get; set; }

        public OrderSummaryEnc ORDER { get; set; }
        public ShippingDetailEnc SHIPPING_DETAIL { get; set; }
        public CyberSourceDataEnc ADDITIONAL_DATA { get; set; }
        public ExternalDetailEnc EXTERNAL_DETAIL { get; set; }
    }

    public class OrderSummaryEnc
    {
        public string DISCOUNT_ON_TOTAL { get; set; }
        public string SUBTOTAL { get; set; }
        public List<OrderSummaryDescriptionEnc> OrderSummaryDescription { get; set; }
    }

    public class OrderSummaryDescriptionEnc
    {
        public string ITEM_NAME { get; set; }
        public string QUANTITY { get; set; }
        public string UNIT_PRICE { get; set; }
        public string OLD_PRICE { get; set; }
        public string CATEGORY { get; set; }
        public string SUB_CATEGORY { get; set; }
    }

    public class ShippingDetailEnc
    {
        public string NAME = "DHL SERVICE";
        public string ICON_PATH { get; set; }
        public string DELIEVERY_DAYS { get; set; }
        public string SHIPPING_COST { get; set; }
    }

    public class CyberSourceDataEnc
    {
        public string PAYMENT_TOKEN { get; set; }
        public string REFERENCE_NUMBER { get; set; }
        public string CUSTOMER_ID { get; set; }
        public string CURRENCY { get; set; }
        public string BILL_TO_FORENAME { get; set; }
        public string BILL_TO_SURNAME { get; set; }
        public string BILL_TO_EMAIL { get; set; }
        public string BILL_TO_PHONE { get; set; }
        public string BILL_TO_ADDRESS_LINE { get; set; }
        public string BILL_TO_ADDRESS_CITY { get; set; }
        public string BILL_TO_ADDRESS_STATE { get; set; }
        public string BILL_TO_ADDRESS_COUNTRY { get; set; }
        public string BILL_TO_ADDRESS_POSTAL_CODE { get; set; }

        public string SHIP_TO_FORENAME { get; set; }
        public string SHIP_TO_SURNAME { get; set; }
        public string SHIP_TO_EMAIL { get; set; }
        public string SHIP_TO_PHONE { get; set; }
        public string SHIP_TO_ADDRESS_LINE { get; set; }
        public string SHIP_TO_ADDRESS_CITY { get; set; }
        public string SHIP_TO_ADDRESS_STATE { get; set; }
        public string SHIP_TO_ADDRESS_COUNTRY { get; set; }
        public string SHIP_TO_ADDRESS_POSTAL_CODE { get; set; }
        public MerchantDefinedFieldEnc MerchantFields { get; set; }


        [JsonIgnore] 
        public int ORDER_ID_TRACKING { get; set; }

        [JsonIgnore]
        public int CUSTOMER_ID_TRACKING { get; set; }

        [JsonIgnore]
        public string PAYMENT_METHOD_TRACKING { get; set; }

    }

    public class MerchantDefinedFieldEnc
    {
        public string MDD1 { get; set; }//Channel of Operation -- 2digit identifier
        public string MDD2 { get; set; }//3D Secure Registration -- YES/NO
        public string MDD3 { get; set; }//Product Category
        public string MDD4 { get; set; }//Product Name
        public string MDD5 { get; set; }// Previous Customer (If yes then Customer ID to be passed)
        public string MDD6 { get; set; }//Shipping Method
        public string MDD7 { get; set; }//Number Of Items Sold
        public string MDD8 { get; set; }//Product Shipping Country Name
        public string MDD9 { get; set; }//Hours Till Departure
        public string MDD10 { get; set; }//Flight Type
        public string MDD11 { get; set; }//Full Journey/ Itinerary
        public string MDD12 { get; set; }//3rd Party Booking -- YES | NO
        public string MDD13 { get; set; }//Hotel Name
        public string MDD14 { get; set; }//Date of Booking -- DD-MM-YY hh:mm
        public string MDD15 { get; set; }//Check In Date -- DD-MM-YY hh:mm
        public string MDD16 { get; set; }//Check Out Date -- DD-MM-YY hh:mm
        public string MDD17 { get; set; }//Product Type
        public string MDD18 { get; set; }//Customer ID/ Phone Number 
        public string MDD19 { get; set; }//Country Of Top - up -- YES | NO
        public string MDD20 { get; set; }//VIP Customer -- DD-MM-YY hh:mm
    }

    public class ExternalDetailEnc
    {
        public string MERCHANT_PLATFORM { get; set; }
        public string PLUGIN_VERSION { get; set; }
        public string BUILD_VERSION { get; set; }
        public string RELEASE_DATE { get; set; }
    }
}