using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KurulumWeb.Models
{
    public class InstallDataInfomation
    {
        public int? Install_ID { get; set; }

        public string Ricon_SN { get; set; }       
        
        public string GSM_No { get; set; }   
        
        public string Site_Name { get; set; }
        
        public string WAN_ip { get; set; }
        
        public string Operator { get; set; }
        
        public string Username { get; set; }

        public int Company_ID { get; set; }
  
        public DateTime Date_Time { get; set; }
       
    }
}