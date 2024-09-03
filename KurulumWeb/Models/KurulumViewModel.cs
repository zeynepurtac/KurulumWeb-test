using SNMPDB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KurulumWeb.Models
{
    public class UserLoginModel
    {
        public int User_ID { get; set; } // Kullanıcı kimliği (gerektiğinde kullanılabilir)
        public string Username { get; set; } // Kullanıcı adı
        public string Password { get; set; } // Şifre (güvenlik nedenleriyle şifreler genellikle şifrelenmiş olarak saklanır)
        public bool IsAdmin { get; set; } // Kullanıcı tipi (örneğin, "User" veya "Admin")
        public string Creator { get; set; } // Oluşturan kullanıcı adı
        public string CreateDate { get; set; } // Kayıt oluşturulma tarihi ve saati
        public int Company_ID { get; set; }
        public int Status { get; set; }

        // İhtiyaç durumuna göre başka özellikler de eklenebilir
        public SNMPDB.UserLogin ToUserLogin()
        {
            return new SNMPDB.UserLogin
            {
                Username = this.Username,
                Password = this.Password,
                Creator = this.Creator,
                IsAdmin = this.IsAdmin,
                Create_DateTime = DateTime.Now,
                Company_ID = this.Company_ID,
                Status = this.Status,
                // Diğer alanları da atama yap
            };
        }
    }

    [Serializable]
    public class UploadResult
    {
        public bool Success { get; set; }
        public List<int> ErrorRows { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DropdownData
    {
        public SelectList SeriNoList { get; set; }
        public SelectList LocationList { get; set; }
        public KurulumViewModel ViewModel { get; set; }
    }

    public class KurulumViewModel
    {
        public SelectList GsmNo1 { get; set; }
        public SelectList GsmNo2 { get; set; }
        public string SelectedGsmID1 { get; set; }
        public bool chechedGms1 { get; set; }
        public bool chechedGms2 { get; set; }
        public string SelectedGsmID2 { get; set; }

        public SelectList GsmNo { get; set; }
        public string SelectedGsmNoID { get; set; }

        public SelectList SeriNo { get; set; }
        public string SelectedSeriNoID { get; set; }

        public SelectList Location { get; set; }
        public string SelectedLocationID { get; set; }
        public SelectList Terminal { get; set; }
        public string SelectedTerminalID { get; set; }
        public bool isVisibleOpenButton { get; set; }
        public string Groupname { get; set; }

        public bool chechedOrange { get; set; }
        public bool chechedInwi { get; set; }
        public bool chechSingle { get; set; }
        public bool chechMulti { get; set; }
        public bool chechMulti2 { get; set; }
        public bool chechMulti3 { get; set; }

        public string Site_Name { get; set; }
        public string Ricon_SN { get; set; }
        public string Gsm_Number { get; set; }

        public SelectList SeriNoList { get; set; }
        public SelectList LocationList { get; set; }
    }

    public class AllModel
    {
        public int ID { get; set; }
        public int SId { get; set; }
        public string SiteName { get; set; }
        public string SelectedSiteName { get; set; }
        public SelectList site { get; set; }
        public int Company_ID { get; set; }
        public string Site_Name { get; set; }
        public int Status { get; set; }
        public string Ricon_SN { get; set; }
        public string RiconSeri { get; set; }
    }

    public class DeviceDetails
    {
        public string Operator { get; set; }
        public string GSMNumber { get; set; }
        public string Details { get; set; }
    }

    public class InstallRatioViewModel
    {
        public int InstallCount { get; set; }
        public int SimCardCount { get; set; }
        public double Ratio { get; set; }
    }

    public class Model
    {
        public string Gsmno { get; set; }
        public int Id { get; set; }
        public string GSM_No1 { get; set; }
        public string APN1_Name { get; set; }

        //public string Apn1Name { get; set; }
        public string APN1_Username { get; set; }

        //public string Apn1Username { get; set; }
        public string APN1_Password { get; set; }

        //public string Apn1Password { get; set; }
        public string GSM_No2 { get; set; }

        //public string Gsm_No2 { get; set; }
        public string APN2_Name { get; set; }

        //public string Apn2Name { get; set; }
        public string APN2_Username { get; set; }

        //public string Apn2Username { get; set; }
        public string APN2_Password { get; set; }

        //public string Apn2Password { get; set; }
        public string Lan1_IP { get; set; }

        //public string Lan1ip { get; set; }
        public string Lan1_SubnetMask { get; set; }

        //public string Lan1SubnetMask { get; set; }
        public string Lan_Subnet { get; set; }

        //public string LanSubnet { get; set; }
        //public int Company_ID { get; set; }
        //public int Companyid { get; set; }
        public string Ricon_SN { get; set; }

        //public string Ricon_Sn { get; set; }
        public bool chechedop1 { get; set; }

        public bool chechedop2 { get; set; }
    }

    public class UserViewModel
    {
        public List<UserLogin> Users { get; set; }

        public int User_ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public string Creator { get; set; }
        public DateTime Create_DateTime { get; set; }
    }

    public class GroupsViewModel
    {
        public List<Groups> AzureGroup { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int Status { get; set; }
    }

    public class OrangeDataModel
    {
        public List<Tbl_Orange> Oranges { get; set; }
        public string Gsmno { get; set; }
        public int Orange_ID { get; set; }
        public string Ricon_SN { get; set; }
        public string GSM_No1 { get; set; }
        public string APN1_Name { get; set; }
        public string APN1_Username { get; set; }
        public string APN1_Password { get; set; }
        public string WAN_ip { get; set; }
        public string vlanid_TG { get; set; }
        public string lan_ip_TG { get; set; }
        public string lan_subnet_TG { get; set; }
        public string lan_subnetmask_TG { get; set; }
        public string vlanid_Servizi { get; set; }
        public string lan_ip_Servizi { get; set; }
        public string lan_subnet_Servizi { get; set; }
        public string lan_subnetmask_Servizi { get; set; }
        public string Tunnel_dc1_r1 { get; set; }
        public string Tunnel_dc2_r1 { get; set; }
        public string Tunnel_ig_r1 { get; set; }
        public string Tg_dhcp_start { get; set; }
        public string Tg_dhcp_end { get; set; }
        public string Ser_dhcp_start { get; set; }
        public string Ser_dhcp_end { get; set; }

        public int Status { get; set; }
    }

    public class OrangeModel
    {
        public string Ricon_SN { get; set; }
        public string GSM_No1 { get; set; }
        public string APN1_Name { get; set; }
        public string APN1_Username { get; set; }
        public string APN1_Password { get; set; }
        public string WAN_ip { get; set; }
        public string vlanid_TG { get; set; }
        public string lan_ip_TG { get; set; }
        public string lan_subnet_TG { get; set; }
        public string lan_subnetmask_TG { get; set; }
        public string vlanid_Servizi { get; set; }
        public string lan_ip_Servizi { get; set; }
        public string lan_subnet_Servizi { get; set; }
        public string lan_subnetmask_Servizi { get; set; }
        public string Tunnel_dc1_r1 { get; set; }
        public string Tunnel_dc2_r1 { get; set; }
        public string Tunnel_ig_r1 { get; set; }
        public string Tg_dhcp_start { get; set; }
        public string Tg_dhcp_end { get; set; }
        public string Ser_dhcp_start { get; set; }
        public string Ser_dhcp_end { get; set; }
    }

    public class InwiDataModel
    {
        public List<Tbl_Inwi> Inwies { get; set; }
        public string Gsmno { get; set; }
        public int Inwi_ID { get; set; }
        public string i_Ricon_SN { get; set; }
        public string GSM_No2 { get; set; }
        public string APN2_Name { get; set; }
        public string APN2_Username { get; set; }
        public string APN2_Password { get; set; }
        public string i_WAN_ip { get; set; }
        public string i_vlanid_TG { get; set; }
        public string i_lan_ip_TG { get; set; }
        public string i_lan_subnet_TG { get; set; }
        public string i_lan_subnetmask_TG { get; set; }
        public string i_vlanid_Servizi { get; set; }
        public string i_lan_ip_Servizi { get; set; }
        public string i_lan_subnet_Servizi { get; set; }
        public string i_lan_subnetmask_Servizi { get; set; }
        public string i_Tg_dhcp_start { get; set; }
        public string i_Tg_dhcp_end { get; set; }
        public string i_Ser_dhcp_start { get; set; }
        public string i_Ser_dhcp_end { get; set; }

        public int Status { get; set; }
    }
    public class InwiModel
    {
     
        public string i_Ricon_SN { get; set; }
        public string GSM_No2 { get; set; }
        public string APN2_Name { get; set; }
        public string APN2_Username { get; set; }
        public string APN2_Password { get; set; }
        public string i_WAN_ip { get; set; }
        public string i_vlanid_TG { get; set; }
        public string i_lan_ip_TG { get; set; }
        public string i_lan_subnet_TG { get; set; }
        public string i_lan_subnetmask_TG { get; set; }
        public string i_vlanid_Servizi { get; set; }
        public string i_lan_ip_Servizi { get; set; }
        public string i_lan_subnet_Servizi { get; set; }
        public string i_lan_subnetmask_Servizi { get; set; }
        public string i_Tg_dhcp_start { get; set; }
        public string i_Tg_dhcp_end { get; set; }
        public string i_Ser_dhcp_start { get; set; }
        public string i_Ser_dhcp_end { get; set; }

    
    }

    // Excel veri modeli
    public class ExcelDataModel
    {
        public List<string> Headers { get; set; }
        public List<List<string>> Rows { get; set; }
    }
}