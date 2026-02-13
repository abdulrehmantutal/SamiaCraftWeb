using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AlHilal.Models.BLL
{
    public class PaymentDetails
    {
        public string sessionID { get; set; }
        public int GrandTotal { get; set; }
        public string Description { get; set; }
        public string OrderNo { get; set; }
    }
}