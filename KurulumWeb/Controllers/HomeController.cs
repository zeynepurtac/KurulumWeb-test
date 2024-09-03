using KurulumWeb.Helper;
using KurulumWeb.Models;
using SNMPDB;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using System;
using SMSApi.Api.Response;
using System.Diagnostics;
using System.Web.UI.WebControls;
using NPOI.SS.Formula.Functions;

namespace KurulumWeb.Controllers
{
    public class HomeController : BaseController
    {
        //private RiconAppsDBEntities2 ctx = new RiconAppsDBEntities2();

        private RiconApps_FASEntities1 ctx = new RiconApps_FASEntities1();
        private string secilen_operator;

        //private Methods m = new Methods();

        private MethodAzure a = new MethodAzure();

        #region Excel Format Orange

        [HttpPost]
        public ActionResult UploadExcelOrange(HttpPostedFileBase file)
        {
            try
            {
                if (file != null)
                {
                    string filename = Path.GetFileName(file.FileName);
                    string folderPath = Server.MapPath("~/excelfolder/");

                    // Create the folder if it doesn't exist
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filepath = Path.Combine(folderPath, filename);
                    file.SaveAs(filepath);

                    UploadResult uploadResult1 = UploadExcelOrange1(filepath, filename);

                    if (uploadResult1.Success && uploadResult1.ErrorRows.Count == 0)
                    {
                        TempData["sonuc"] = "Upload is Successfully";
                        return Json(new { success = true });
                    }
                    else if (uploadResult1.Success && uploadResult1.ErrorRows.Count > 0)
                    {
                        TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
                        return Json(new { success = true, errorRows = uploadResult1.ErrorRows });
                    }
                    else
                    {
                        TempData["sonuc"] = "Upload is not Successfully";
                        return Json(new { success = false, errorRows = uploadResult1.ErrorRows, uploadResult1.ErrorMessage });
                    }
                }
                else
                {
                    TempData["sonuc"] = "File Not Selected.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                TempData["sonuc"] = "There was an error loading the file: " + e.Message;
                return Json(new { success = false, message = e.Message });
            }
        }
        private UploadResult UploadExcelOrange1(string filepath, string filename)
        {
            UploadResult result = new UploadResult();
            try
            {
                Thread.Sleep(10000);
                FileInfo excelFile = new FileInfo(filepath);

                using (ExcelPackage package = new ExcelPackage(excelFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    List<int> errorRows = new List<int>();
                    int currentRow = 2;

                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
                    {
                        con.Open();
                        using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
                        {
                            objbulk.DestinationTableName = "Tbl_Orange";
                            objbulk.ColumnMappings.Add("Ricon_SN", "Ricon_SN");
                            objbulk.ColumnMappings.Add("GSM_No1", "GSM_No1");
                            objbulk.ColumnMappings.Add("APN1_Name", "APN1_Name");
                            objbulk.ColumnMappings.Add("APN1_Username", "APN1_Username");
                            objbulk.ColumnMappings.Add("WAN_ip", "WAN_ip");
                            objbulk.ColumnMappings.Add("vlanid_TG", "vlanid_TG");
                            objbulk.ColumnMappings.Add("lan_ip_TG", "lan_ip_TG");
                            objbulk.ColumnMappings.Add("lan_subnet_TG", "lan_subnet_TG");
                            objbulk.ColumnMappings.Add("lan_subnetmask_TG", "lan_subnetmask_TG");
                            objbulk.ColumnMappings.Add("vlanid_Servizi", "vlanid_Servizi");
                            objbulk.ColumnMappings.Add("lan_ip_Servizi", "lan_ip_Servizi");
                            objbulk.ColumnMappings.Add("lan_subnet_Servizi", "lan_subnet_Servizi");
                            objbulk.ColumnMappings.Add("lan_subnetmask_Servizi", "lan_subnetmask_Servizi");
                            objbulk.ColumnMappings.Add("Tunnel_dc1_r1", "Tunnel_dc1_r1");
                            objbulk.ColumnMappings.Add("Tunnel_dc2_r1", "Tunnel_dc2_r1");
                            objbulk.ColumnMappings.Add("Tunnel_ig_r1", "Tunnel_ig_r1");
                            //
                            objbulk.ColumnMappings.Add("Tg_dhcp_start", "Tg_dhcp_start");
                            objbulk.ColumnMappings.Add("Tg_dhcp_end", "Tg_dhcp_end");
                            objbulk.ColumnMappings.Add("Ser_dhcp_start", "Ser_dhcp_start");
                            objbulk.ColumnMappings.Add("Ser_dhcp_end", "Ser_dhcp_end");

                            DataTable filteredTable = new DataTable(); // Yeni bir tablo oluştur

                            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                            {
                                filteredTable.Columns.Add(worksheet.Cells[1, col].Text);
                            }

                            for (int row = 2; row <= rowCount; row++)
                            {
                                DataRow dataRow = filteredTable.Rows.Add();
                                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                                {
                                    dataRow[col - 1] = worksheet.Cells[row, col].Text;
                                }

                                string gsmNo1 = dataRow["GSM_No1"].ToString();

                                // Kontrol SQL sorgusu
                                string checkIfExistsQuery = "SELECT COUNT(*) FROM Tbl_Orange WHERE GSM_No1 = @GSM_No1";
                                using (SqlCommand checkCmd = new SqlCommand(checkIfExistsQuery, con))
                                {
                                    checkCmd.Parameters.AddWithValue("@GSM_No1", gsmNo1);
                                    int existingRowCount = (int)checkCmd.ExecuteScalar();

                                    if (existingRowCount > 0)
                                    {
                                        // Tbl_Orange tablosunda gsm numarası varsa, hatalı satıra ekle ve işlemi devam ettirme
                                        errorRows.Add(currentRow);
                                        currentRow++;
                                        continue;
                                    }
                                }

                                string apnName1 = dataRow["APN1_Name"].ToString();
                                string vlanIdTg = dataRow["vlanid_TG"].ToString();
                                string lanIpTg = dataRow["lan_ip_TG"].ToString();
                                string labSubnetTg = dataRow["lan_subnet_TG"].ToString();
                                string labSubnetMaskTg = dataRow["lan_subnetmask_TG"].ToString();
                                string vlanIdServ = dataRow["vlanid_Servizi"].ToString();
                                string lanIpServ = dataRow["lan_ip_Servizi"].ToString();
                                string labSubnetServ = dataRow["lan_subnet_Servizi"].ToString();
                                string labSubnetMaskServ = dataRow["lan_subnetmask_Servizi"].ToString();
                                string tunneldc1r1 = dataRow["Tunnel_dc1_r1"].ToString();
                                string tunneldc2r1 = dataRow["Tunnel_dc2_r1"].ToString();
                                string tunneligr1 = dataRow["Tunnel_ig_r1"].ToString();
                                //
                                string tgdhcpStart = dataRow["Tg_dhcp_start"].ToString();
                                string tgdhcpEnd = dataRow["Tg_dhcp_end"].ToString();
                                string serdhcpStart = dataRow["Ser_dhcp_start"].ToString();
                                string serdhcpEnd = dataRow["Ser_dhcp_end"].ToString();

                                // Koşulları kontrol et
                                if (!string.IsNullOrEmpty(gsmNo1) && !string.IsNullOrEmpty(apnName1) &&
                                    !string.IsNullOrEmpty(vlanIdTg) && !string.IsNullOrEmpty(lanIpTg) &&
                                    !string.IsNullOrEmpty(labSubnetTg) && !string.IsNullOrEmpty(labSubnetMaskTg) &&
                                    !string.IsNullOrEmpty(vlanIdServ) && !string.IsNullOrEmpty(lanIpServ) &&
                                    !string.IsNullOrEmpty(labSubnetServ) && !string.IsNullOrEmpty(tunneldc1r1) &&
                                    !string.IsNullOrEmpty(tunneldc2r1) && !string.IsNullOrEmpty(tunneligr1) &&
                                    !string.IsNullOrEmpty(tgdhcpStart) && !string.IsNullOrEmpty(tgdhcpEnd) &&
                                    !string.IsNullOrEmpty(serdhcpStart) && !string.IsNullOrEmpty(serdhcpEnd))
                                {
                                    // Eğer buraya girdiyse, bu GSM_No1 daha önce eklenmemiş demektir, ekle
                                    objbulk.WriteToServer(filteredTable);
                                }
                                else
                                {
                                    // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
                                    errorRows.Add(currentRow);
                                }
                                currentRow++;
                            }

                            // Diğer veritabanı işlemleri
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = con;
                                cmd.CommandText = "UPDATE Tbl_Orange SET Status = 1 WHERE Status IS NULL";
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = con;
                                cmd.CommandText = "INSERT INTO GsmNumber (GSM_No, Company_ID, Status) SELECT GSM_No1, 2, 1 FROM Tbl_Orange WHERE GSM_No1 NOT IN (SELECT GSM_No FROM GsmNumber)";
                                cmd.ExecuteNonQuery();
                            }

                            con.Close();
                        }

                        if (errorRows.Count > 0)
                        {
                            result.Success = true; // Hata olmasına rağmen işlem başarılıdır
                            result.ErrorRows = errorRows;
                        }
                        else
                        {
                            result.Success = true;
                            result.ErrorRows = new List<int>();  // Boş bir hata listesi// Hata olmadığında işlem başarılıdır
                        }

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false; // Hata olduğunda işlem başarısızdır
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        #endregion Excel Format Orange

        //#region Excel Format Inwi

        //[HttpPost]
        //    public ActionResult UploadExcelInwi(HttpPostedFileBase file)
        //    {
        //        try
        //        {
        //            if (file != null)
        //            {
        //                string filename = Path.GetFileName(file.FileName);
        //                string folderPath = Server.MapPath("~/excelfolder/");

        //                // Create the folder if it doesn't exist
        //                if (!Directory.Exists(folderPath))
        //                {
        //                    Directory.CreateDirectory(folderPath);
        //                }

        //                string filepath = Path.Combine(folderPath, filename);
        //                file.SaveAs(filepath);

        //                UploadResult uploadResult1 = UploadExcelInwi1(filepath, filename); // UploadResult türünde bir değişken kullan

        //                if (uploadResult1.Success && uploadResult1.ErrorRows.Count == 0)
        //                {
        //                    TempData["sonuc"] = "Upload is Successfully"; // Tüm satırlar başarıyla yüklendi.
        //                    return Json(new { success = true });
        //                }
        //                else if (uploadResult1.Success && uploadResult1.ErrorRows.Count > 0)
        //                {
        //                    TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
        //                    return Json(new { success = true, errorRows = uploadResult1.ErrorRows });
        //                }
        //                else
        //                {
        //                    TempData["sonuc"] = "Upload is not Successfully";
        //                    return Json(new { success = false, errorRows = uploadResult1.ErrorRows });
        //                }
        //            }
        //            else
        //            {
        //                TempData["sonuc"] = "File not selected.";
        //                return RedirectToAction("Index");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            TempData["sonuc"] = "There was an error loading the file: " + e.Message;
        //            return Json(new { success = false, message = e.Message });
        //        }
        //    }

        //    private OleDbConnection EconnInwi;

        //    private void ExcelConnectInwi(string filepath)
        //    {
        //        string cons = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + "Extended Properties=" + "\"" + "Excel 12.0;HDR=YES;" + "\"";
        //        EconnInwi = new OleDbConnection(cons);
        //    }

        //    private UploadResult UploadExcelInwi1(string filepath, string filename)
        //    {
        //        UploadResult result = new UploadResult();
        //        try
        //        {
        //            Thread.Sleep(10000);
        //            string fullpath = Server.MapPath("/excelfolder/") + filename;
        //            ExcelConnectInwi(fullpath);

        //            string query = string.Format("Select * from [{0}]", "inwi$");
        //            OleDbCommand EcomInwi = new OleDbCommand(query, EconnInwi);
        //            EconnInwi.Open();

        //            DataSet ds = new DataSet();
        //            OleDbDataAdapter oda = new OleDbDataAdapter(query, EconnInwi);
        //            oda.Fill(ds);
        //            EconnInwi.Close();

        //            DataTable dt = ds.Tables[0];

        //            List<int> errorRows = new List<int>();
        //            int currentRow = 2;
        //            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
        //            {
        //                con.Open();
        //                using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
        //                {
        //                    objbulk.DestinationTableName = "Tbl_Inwi";

        //                    objbulk.ColumnMappings.Add("Ricon_SN", "Ricon_SN");
        //                    objbulk.ColumnMappings.Add("GSM_No2", "GSM_No2");
        //                    objbulk.ColumnMappings.Add("APN2_Name", "APN2_Name");
        //                    objbulk.ColumnMappings.Add("APN2_Username", "APN2_Username");
        //                    objbulk.ColumnMappings.Add("APN2_Password", "APN2_Password");
        //                    objbulk.ColumnMappings.Add("WAN_ip", "i_WAN_ip");
        //                    objbulk.ColumnMappings.Add("vlanid_TG", "i_vlanid_TG");
        //                    objbulk.ColumnMappings.Add("lan_ip_TG", "i_lan_ip_TG");
        //                    objbulk.ColumnMappings.Add("lan_subnet_TG", "i_lan_subnet_TG");
        //                    objbulk.ColumnMappings.Add("lan_subnetmask_TG", "i_lan_subnetmask_TG");
        //                    objbulk.ColumnMappings.Add("vlanid_Servizi", "i_vlanid_Servizi");
        //                    objbulk.ColumnMappings.Add("lan_ip_Servizi", "i_lan_ip_Servizi");
        //                    objbulk.ColumnMappings.Add("lan_subnet_Servizi", "i_lan_subnet_Servizi");
        //                    objbulk.ColumnMappings.Add("lan_subnetmask_Servizi", "i_lan_subnetmask_Servizi");
        //                    objbulk.ColumnMappings.Add("Tg_dhcp_start", "i_Tg_dhcp_start");
        //                    objbulk.ColumnMappings.Add("Tg_dhcp_end", "i_Tg_dhcp_end");
        //                    objbulk.ColumnMappings.Add("Ser_dhcp_start", "i_Ser_dhcp_start");
        //                    objbulk.ColumnMappings.Add("Ser_dhcp_end", "i_Ser_dhcp_end");

        //                    DataTable filteredTable = dt.Clone(); // Aynı şema ile yeni bir tablo oluştur

        //                    var uniqueGSM_No2Values = ctx.Tbl_Inwi.Select(x => x.GSM_No2).Distinct().ToList();

        //                    Dictionary<string, bool> gsmNo2Dictionary = new Dictionary<string, bool>();
        //                    foreach (DataRow row in dt.Rows)
        //                    {
        //                        string gsmNo2 = row["GSM_No2"].ToString();
        //                        if (uniqueGSM_No2Values.Contains(gsmNo2))
        //                        {
        //                            errorRows.Add(currentRow);
        //                            currentRow++;
        //                            continue;
        //                        }

        //                        if (gsmNo2Dictionary.ContainsKey(gsmNo2) && gsmNo2Dictionary[gsmNo2])
        //                        {
        //                            errorRows.Add(currentRow);
        //                            currentRow++;
        //                            continue;
        //                        }

        //                        string apnName2 = row["APN2_Name"].ToString();
        //                        string vlanIdTg = row["vlanid_TG"].ToString();
        //                        string lanIpTg = row["lan_ip_TG"].ToString();
        //                        string labSubnetTg = row["lan_subnet_TG"].ToString();
        //                        string labSubnetMaskTg = row["lan_subnetmask_TG"].ToString();
        //                        string vlanIdServ = row["vlanid_Servizi"].ToString();
        //                        string lanIpServ = row["lan_ip_Servizi"].ToString();
        //                        string labSubnetServ = row["lan_subnet_Servizi"].ToString();
        //                        string labSubnetMaskServ = row["lan_subnetmask_Servizi"].ToString();
        //                        //
        //                        string tgdhcpStart = row["Tg_dhcp_start"].ToString();
        //                        string tgdhcpEnd = row["Tg_dhcp_end"].ToString();
        //                        string serdhcpStart = row["Ser_dhcp_start"].ToString();
        //                        string serdhcpEnd = row["Ser_dhcp_end"].ToString();

        //                        // Koşulları kontrol et
        //                        if (!string.IsNullOrEmpty(gsmNo2) && !string.IsNullOrEmpty(apnName2) &&
        //                            !string.IsNullOrEmpty(vlanIdTg) && !string.IsNullOrEmpty(lanIpTg) &&
        //                            !string.IsNullOrEmpty(labSubnetTg) && !string.IsNullOrEmpty(labSubnetMaskTg) &&
        //                            !string.IsNullOrEmpty(vlanIdServ) && !string.IsNullOrEmpty(lanIpServ) &&
        //                            !string.IsNullOrEmpty(labSubnetServ) && !string.IsNullOrEmpty(labSubnetMaskServ)
        //                            && !string.IsNullOrEmpty(tgdhcpStart) && !string.IsNullOrEmpty(tgdhcpEnd)
        //                            && !string.IsNullOrEmpty(serdhcpStart) && !string.IsNullOrEmpty(serdhcpEnd))
        //                        {
        //                            filteredTable.ImportRow(row);
        //                        }
        //                        else
        //                        {
        //                            // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
        //                            errorRows.Add(currentRow);
        //                        }
        //                        gsmNo2Dictionary[gsmNo2] = true;
        //                        currentRow++;
        //                    }

        //                    objbulk.WriteToServer(filteredTable);// Filtrelenmiş tabloyu kullanarak veriyi ekleyin
        //                    using (SqlCommand cmd = new SqlCommand())
        //                    {
        //                        cmd.Connection = con;
        //                        cmd.CommandText = "UPDATE Tbl_Inwi SET Status = 1 WHERE Status IS NULL";
        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }

        //                using (SqlCommand cmd = new SqlCommand())
        //                {
        //                    cmd.Connection = con;
        //                    cmd.CommandText = "INSERT INTO GsmNumber (GSM_No, Company_ID, Status) SELECT GSM_No2, 2, 1 FROM Tbl_Inwi WHERE GSM_No2 NOT IN (SELECT GSM_No FROM GsmNumber)";
        //                    cmd.ExecuteNonQuery();
        //                }
        //            }

        //            if (errorRows.Count > 0)
        //            {
        //                result.Success = true; // Hata olmasına rağmen işlem başarılıdır
        //                result.ErrorRows = errorRows;
        //            }
        //            else
        //            {
        //                result.Success = true;
        //                result.ErrorRows = new List<int>();  // Boş bir hata listesi// Hata olmadığında işlem başarılıdır
        //            }

        //            return result;
        //        }
        //        catch (Exception)
        //        {
        //            result.Success = false; // Hata olduğunda işlem başarısızdır
        //            return result;
        //        }
        //    }

        //    #endregion Excel Format Inwi

        #region Manage

        public ActionResult ManageAzure()
        {
            // Oturum kontrolü yapılır, eğer kullanıcı oturum açmışsa devam edilir.
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                var grouplist = ctx.Groups.ToList();

                var viewModel = new GroupsViewModel
                {
                    AzureGroup = grouplist
                };
                return View(viewModel);
            }
            else
            {
                // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                return RedirectToAction("Login");
            }
        }

        public ActionResult ManageAccounts()
        {
            // Oturum kontrolü yapılır, eğer kullanıcı oturum açmışsa devam edilir.
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                var userlist = ctx.UserLogin.Where(u => u.Status == 1).ToList();

                var viewModel = new UserViewModel
                {
                    Users = userlist
                };
                return View(viewModel);
            }
            else
            {
                // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                return RedirectToAction("Login");
            }
        }

        public ActionResult EditUser(UserViewModel usermodel)
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();
                DateTime islemSaati = DateTime.Now;
                // Veri tabanı bağlantısı oluştur

                // Kullanıcıyı veri tabanından al
                var userdata = ctx.UserLogin.FirstOrDefault(u => u.User_ID == usermodel.User_ID);
                if (userdata != null)
                {
                    // Kullanıcının özelliklerini güncelle
                    userdata.Username = usermodel.Username;
                    userdata.Password = usermodel.Password;
                    userdata.IsAdmin = usermodel.IsAdmin;
                    userdata.Creator = kullanici;
                    userdata.Create_DateTime = islemSaati;

                    // Değişiklikleri veri tabanına kaydet
                    ctx.SaveChanges();

                    // Başarılı mesajı göster
                    TempData["SuccessMessage"] = "User information has been successfully updated.";
                    return RedirectToAction("ManageAccounts");
                }
            }

            return RedirectToAction("Login"); // Kullanıcıyı başka bir sayfaya yönlendir
        }

        [HttpPost]
        public ActionResult DeleteUser(int User_ID)
        {
            try
            {
                // User_ID'ye göre veritabanındaki kullanıcıyı bulun ve durumunu 0 olarak güncelleyin
                var user = ctx.UserLogin.SingleOrDefault(u => u.User_ID == User_ID);
                if (user != null)
                {
                    user.Status = 0; // Veya kullanıcıyı veritabanından tamamen kaldırabilirsiniz.
                    ctx.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region User Ekleme

        [HttpPost]
        public JsonResult AddUser(string Username, string Password, string Type)
        {
            try
            {
                // Oturum bilgilerinden Creator alanını al
                string creator = Session["UserName"]?.ToString() ?? "System"; // Eğer oturum bilgisi yoksa varsayılan değeri "System" olarak ayarla

                // CreateDate için şu anki tarih ve saati al ve uygun formata dönüştür
                string createDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                bool isAdmin = Type.ToLower() == "admin";

                if (IsPasswordSecure(Password))
                {
                    // Yeni kullanıcıyı oluştur
                    UserLoginModel newUser = new UserLoginModel
                    {
                        Username = Username,
                        Password = Password,
                        IsAdmin = isAdmin,
                        Creator = creator,
                        CreateDate = createDateTime,
                        Company_ID = 2,
                        Status = 1,
                    };

                    // Yeni kullanıcıyı veritabanına eklemek için uygun bir metodu kullanın
                    using (var context = new RiconApps_FASEntities1())
                    {
                        // UserLoginModel'den SNMPDB.UserLogin türüne dönüşüm yap
                        SNMPDB.UserLogin userLoginEntity = newUser.ToUserLogin();

                        // Yeni kullanıcıyı veritabanına ekleyin
                        context.UserLogin.Add(userLoginEntity);
                        context.SaveChanges();
                    }

                    // Başarılı bir şekilde eklendiğini bildir
                    return Json(new { success = true, message = "User added successfully." });
                }
                return Json(new { success = false, message = Resources.Resources.L338 });
            }
            catch (Exception ex)
            {
                // Hata durumunda geri dönecek JSON yanıtı
                return Json(new { success = false, message = "Error adding a user: " + ex.Message });
            }
        }

        #endregion User Ekleme

        #region Password kontrol

        private bool IsPasswordSecure(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
            {
                return false;
            }

            return true;
        }

        #endregion Password kontrol

        #endregion Manage

        #region view data

        [HttpGet]
        public ActionResult GetOperatorDetails(string operatorName, string gsmNo)
        {
            try
            {
                List<object> combinedData = new List<object>();
                // Operator değerine göre ilgili tabloyu seçin ve verileri alın
                if (operatorName.ToLower() == "orange")
                {
                    // "Install" tablosundan seri numarasını çekin
                    var seri = ctx.Install
                        .Where(x => x.GSM_No == gsmNo)
                        .Select(x => x.Ricon_SN)
                        .FirstOrDefault();

                    if (seri != null)
                    {
                        List<OrangeModel> data = ctx.Tbl_Orange
                            .Where(x => x.GSM_No1 == gsmNo)
                            .Select(x => new OrangeModel
                            {
                                Ricon_SN = seri, // Seri numarasını burada kullanın
                                GSM_No1 = x.GSM_No1,
                                APN1_Name = x.APN1_Name,
                                APN1_Username = x.APN1_Username,
                                APN1_Password = x.APN1_Password,
                                WAN_ip = x.WAN_ip,
                                vlanid_TG = x.vlanid_TG,
                                lan_ip_TG = x.lan_ip_TG,
                                lan_subnet_TG = x.lan_subnet_TG,
                                lan_subnetmask_TG = x.lan_subnetmask_TG,
                                vlanid_Servizi = x.lan_subnetmask_TG,
                                lan_ip_Servizi = x.lan_ip_Servizi,
                                lan_subnet_Servizi = x.lan_subnet_Servizi,
                                lan_subnetmask_Servizi = x.lan_subnetmask_Servizi,
                                Tunnel_dc1_r1 = x.Tunnel_dc1_r1,
                                Tunnel_dc2_r1 = x.Tunnel_dc2_r1,
                                Tunnel_ig_r1 = x.Tunnel_ig_r1,
                                Tg_dhcp_start = x.Tg_dhcp_start,
                                Tg_dhcp_end = x.Tg_dhcp_end,
                                Ser_dhcp_start = x.Ser_dhcp_start,
                                Ser_dhcp_end = x.Ser_dhcp_end,
                                // Diğer özellikleri de burada doldurun
                            })
                            .ToList();

                        combinedData.AddRange(data);
                    }
                }
                else if (operatorName.ToLower() == "inwi")
                {
                    // "Install" tablosundan seri numarasını çekin
                    var serial = ctx.Install
                        .Where(x => x.GSM_No == gsmNo)
                        .Select(x => x.Ricon_SN)
                        .FirstOrDefault();
                    if (serial != null)
                    {
                        List<InwiModel> data = ctx.Tbl_Inwi
                         .Where(x => x.GSM_No2 == gsmNo)
                          .Select(x => new InwiModel
                          {
                              i_Ricon_SN = serial,
                              GSM_No2 = x.GSM_No2,
                              APN2_Name = x.APN2_Name,
                              APN2_Username = x.APN2_Username,
                              APN2_Password = x.APN2_Password,
                              i_WAN_ip = x.i_WAN_ip,
                              i_vlanid_TG = x.i_vlanid_TG,
                              i_lan_ip_TG = x.i_lan_ip_TG,
                              i_lan_subnet_TG = x.i_lan_subnet_TG,
                              i_lan_subnetmask_TG = x.i_lan_subnetmask_TG,
                              i_vlanid_Servizi = x.i_lan_subnetmask_TG,
                              i_lan_ip_Servizi = x.i_lan_ip_Servizi,
                              i_lan_subnet_Servizi = x.i_lan_subnet_Servizi,
                              i_lan_subnetmask_Servizi = x.i_lan_subnetmask_Servizi,
                              i_Tg_dhcp_start = x.i_Tg_dhcp_start,
                              i_Tg_dhcp_end = x.i_Tg_dhcp_end,
                              i_Ser_dhcp_start = x.i_Ser_dhcp_start,
                              i_Ser_dhcp_end = x.i_Ser_dhcp_end,

                              // Diğer özellikleri de burada doldurun
                          })
                          .ToList();
                        combinedData.AddRange(data);
                    }
                    //  return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Belirtilen operatör desteklenmiyorsa hata döndürün
                    return Json(new { success = false, message = "Invalid Operator" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, data = combinedData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Hata işleme kodu burada
                return Json(new { success = false, message = " " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion view data

        #region DIL

        //public ActionResult SetCulture(string culture)
        //{
        //    culture = HttpContext.Request.Form["cultureText"];
        //    CultureHelper.setCulture(culture);
        //    return RedirectToAction("Home");
        //}

        #endregion DIL

        #region grafik

        public ActionResult GetInstallRatio()
        {
            if (Session["UserName"] != null)
            {
                //        SimCards tablosundaki veri sayısını al
                var orangeCount = ctx.Tbl_Orange.Count();
                var inwiCount = ctx.Tbl_Inwi.Count();
                var totalCount = orangeCount + inwiCount;

                //        Install tablosundaki veri sayısını al
                var installCount = ctx.Install.Count();

                var other = totalCount - installCount;

                var abc = installCount * 100;
                var ratio = abc / totalCount;

                //        Sonucu JSON formatında döndür
                var data = new
                {
                    InstallCount = installCount,
                    TotalCount = totalCount,
                    Ratio = ratio
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion grafik

        #region RiconAdmin İçin kurulum

        // RiconAzure Action, ricon sayfasını doldurur.
        public ActionResult RiconAdmin()
        {
            // Oturum kontrolü yapılır, eğer kullanıcı oturum açmışsa devam edilir.
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                string kullanici = Session["UserName"].ToString();

                // Oturumda "Sonuc" adında bir değişken varsa, bu değer ViewBag'e aktarılır.
                if (Session["Sonuc"] != null)
                {
                    ViewBag.Sonuc = Session["Sonuc"].ToString();
                    Session["Sonuc"] = null; // Sonuc değerini temizle
                }

                // Eğer "InfoTitle" ve "SmsMessage" oturum değişkenleri mevcutsa, bunlar da ViewBag'e aktarılır.
                if ((Session["InfoTitle"] != null) && (Session["SmsMessage"] != null))
                {
                    ViewBag.InfoTitle = Session["InfoTitle"].ToString();
                    ViewBag.SmsMessage = Session["SmsMessage"].ToString();
                    Session["InfoTitle"] = null; // Sonuc değerini temizle
                    Session["SmsMessage"] = null; // Sonuc değerini temizle
                }

                // Kullanıcı adı boş değilse devam edilir.
                if (kullanici != "")
                {
                    // SelectList nesneleri oluşturulur ve ViewBag'e eklenir.
                    ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");

                    // KurulumViewModel sınıfından bir nesne oluşturulur ve bazı alanlar başlangıç değerleriyle doldurulur.
                    var viewModel = new KurulumViewModel
                    {
                        Ricon_SN = "", // Boş değerle başlatılıyor
                        Site_Name = "",
                        Gsm_Number = "",
                    };

                    // Oluşturulan view model, görüntülenmek üzere View'a gönderilir.
                    return View(viewModel);
                }
                else
                {
                    // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpGet]
        public ActionResult GetDetails(string Operator, string gsmNumber, string Details)
        {
            using (var context = new RiconApps_FASEntities1()) // YourDbContext, projenizdeki DbContext sınıfını temsil eder
            {
                // Veritabanından belirli operatör ve GSM numarasına sahip kaydı bulun
                var install = context.Install
                    .FirstOrDefault(i => i.Operator == Operator && i.GSM_No == gsmNumber && i.Username == Details);

                if (install != null)
                {
                    // Eğer veri bulunduysa, detay nesnesini oluşturun
                    DeviceDetails details = new DeviceDetails
                    {
                        Operator = install.Operator,
                        GSMNumber = install.GSM_No,
                        Details = install.Username // Install modelindeki diğer detay alanını kullanabilirsiniz
                    };

                    // JSON formatında detay verilerini döndür
                    return Json(details);
                }

                // Veri bulunamadıysa boş bir JSON döndürün veya uygun bir hata durumu işleyin
                return Json(new { error = "No data found" });
            }
        }

        [HttpPost]
        public ActionResult RiconAdmin(KurulumViewModel model)
        {
            try
            {
                // Kullanıcı girişi yapılmış mı kontrol ediliyor
                if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
                {
                    string kullanici = Session["UserName"].ToString();

                    string company_id = Session["Operator"].ToString();

                    if (kullanici != "")
                    {
                        // Seçilen Seri No ve Konum kontrol ediliyor
                        if (model.SelectedSeriNoID == null || model.SelectedGsmNoID == null)
                        {
                            // Her ikisi de seçilmemişse hata mesajları ayarlanıyor
                            if (model.SelectedLocationID == null)
                            {
                                ViewBag.LocationID = CultureHelper.GetResourceKey("L119");
                            }
                            if (model.SelectedSeriNoID == null)
                            {
                                ViewBag.SeriNoID = CultureHelper.GetResourceKey("L120");
                            }
                            if (model.SelectedGsmNoID == null)
                            {
                                ViewBag.GsmNoID = CultureHelper.GetResourceKey("L315");
                            }

                            ViewBag.Sonuc = CultureHelper.GetResourceKey("L122");
                        }
                        else
                        {
                            ViewBag.Sonuc = "";
                            Session["SelectedSeriNoID"] = model.SelectedSeriNoID;
                            Session["SelectedLocationID"] = model.SelectedLocationID;
                            Session["SelectedGsmNoID"] = model.SelectedGsmNoID;

                            //kullanici = Session["UserName"].ToString();
                            //company_id = Session["Operator"].ToString();
                            //string secilen_gsm = Session["SelectedGsmNoID"].ToString();
                            //string secilen_operator;
                            // Seçilen Seri No'ya göre veritabanından kayıt sorgulanıyor
                            var query_ricon_sr = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).FirstOrDefault();

                            if (query_ricon_sr != null)
                            {
                                // Seçilen GSM No belirleniyor
                                string secilen_gsm = model.SelectedGsmNoID;
                                var orange = ctx.Tbl_Orange.Where(x => x.GSM_No1 == model.SelectedGsmNoID).FirstOrDefault();
                                var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == model.SelectedGsmNoID).FirstOrDefault();

                                if (orange != null)
                                {
                                    // Seçilen GSM Tbl_Orange tablosunda bulunuyor
                                    secilen_operator = "orange";
                                }
                                else if (inwi != null)
                                {
                                    // Seçilen GSM Tbl_Inwi tablosunda bulunuyor
                                    secilen_operator = "inwi";
                                }
                                else
                                {
                                    secilen_operator = "null";
                                }

                                if (model.chechMulti2)
                                {
                                    // chkMulti2 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-2";
                                }
                                else if (model.chechMulti3)
                                {
                                    // chkMulti3 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-3";
                                }
                                else
                                {
                                    model.SelectedLocationID = model.SelectedLocationID + "-1";
                                }

                                a.selected_riconsn = model.SelectedSeriNoID;
                                a.selected_operator = secilen_operator;
                                a.selected_gsm = secilen_gsm;
                                Session["SelectedGMSNO"] = secilen_gsm;
                                Session["SelectedOperator"] = secilen_operator;
                                Session["SelectedLocation"] = model.SelectedLocationID;
                                // Seçilen konumun veritabanında kontrolü yapılıyor
                                var gsmRecords = ctx.Install.Where(x => x.GSM_No == model.SelectedGsmNoID).ToList();

                                // Seçilen seri numarası ile ilgili kayıtları çek
                                var serialNumberRecords = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).ToList();

                                // Hem GSM numarası hem de seri numarası ile ilgili kayıtları çek
                                var commonRecords = ctx.Install
                                    .Where(x => x.GSM_No == model.SelectedGsmNoID && x.Ricon_SN == model.SelectedSeriNoID)
                                    .ToList();
                                if (secilen_operator == "null" && gsmRecords.Count == 0)
                                {
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.Operator = CultureHelper.GetResourceKey("L353");
                                }
                                // İlgili kayıtların sayısına bakarak kontrol yap
                                else if (gsmRecords.Count > 0 && serialNumberRecords.Count > 0)
                                {
                                    // Hem GSM hem de seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.CaseMessageYesNo = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else if (gsmRecords.Count > 0 || serialNumberRecords.Count > 0)
                                {
                                    // Sadece GSM veya sadece seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.StartOKMessage = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else
                                {
                                    { // case 1
                                        ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                        ViewBag.StartOKMessage = CultureHelper.GetResourceKey("L239");
                                    }
                                }
                            }
                            else
                            {
                                string secilen_gsm = model.SelectedGsmNoID;
                                var orange = ctx.Tbl_Orange.Where(x => x.GSM_No1 == model.SelectedGsmNoID).FirstOrDefault();
                                var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == model.SelectedGsmNoID).FirstOrDefault();

                                if (orange != null)
                                {
                                    // Seçilen GSM Tbl_Orange tablosunda bulunuyor
                                    secilen_operator = "orange";
                                }
                                else if (inwi != null)
                                {
                                    // Seçilen GSM Tbl_Inwi tablosunda bulunuyor
                                    secilen_operator = "inwi";
                                }
                                else
                                {
                                    secilen_operator = "null";
                                }

                                if (model.chechMulti2)
                                {
                                    // chkMulti2 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-2";
                                }
                                else if (model.chechMulti3)
                                {
                                    // chkMulti3 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-3";
                                }
                                else
                                {
                                    model.SelectedLocationID = model.SelectedLocationID + "-1";
                                }

                                a.selected_riconsn = model.SelectedSeriNoID;
                                a.selected_operator = secilen_operator;
                                a.selected_gsm = secilen_gsm;
                                Session["SelectedGMSNO"] = secilen_gsm;
                                Session["SelectedOperator"] = secilen_operator;
                                Session["SelectedLocation"] = model.SelectedLocationID;

                                var gsmRecords = ctx.Install.Where(x => x.GSM_No == model.SelectedGsmNoID).ToList();

                                // Seçilen seri numarası ile ilgili kayıtları çek
                                var serialNumberRecords = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).ToList();

                                // Hem GSM numarası hem de seri numarası ile ilgili kayıtları çek
                                var commonRecords = ctx.Install
                                    .Where(x => x.GSM_No == model.SelectedGsmNoID && x.Ricon_SN == model.SelectedSeriNoID)
                                    .ToList();

                                if (secilen_operator == "null" && gsmRecords.Count == 0)
                                {
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.Operator = CultureHelper.GetResourceKey("L353");
                                }
                                // İlgili kayıtların sayısına bakarak kontrol yap
                                else if (gsmRecords.Count > 0 && serialNumberRecords.Count > 0)
                                {
                                    // Hem GSM hem de seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.CaseMessageYesNo = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else if (gsmRecords.Count > 0 || serialNumberRecords.Count > 0)
                                {
                                    // Sadece GSM veya sadece seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.StartOKMessage = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else
                                {
                                    { // case 1
                                        ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                        ViewBag.StartOKMessage = CultureHelper.GetResourceKey("L239");
                                    }
                                }
                            }
                        }

                        // SelectList nesneleri oluşturulur ve ViewBag'e eklenir.
                        ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        var viewModel = new KurulumViewModel
                        {
                            Ricon_SN = "", // Boş değerle başlatılıyor
                            Site_Name = "",
                            Gsm_Number = "",
                        };

                        Session["Sonuc"] = " ";
                        Session["SmsMessage"] = null;
                        return View(viewModel);
                    }
                    else
                    {
                        // Kullanıcı girişi yapılmamışsa giriş sayfasına yönlendiriliyor
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception)
            {
                // Hata durumu
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L234");
                string kullanici = Session["UserName"].ToString();
                string company_id = Session["Operator"].ToString();
                ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");

                var viewModel = new KurulumViewModel
                {
                    Ricon_SN = "", // Boş değerle başlatılıyor
                    Site_Name = "",
                    Gsm_Number = "",
                };
                return View(viewModel);
            }
        }

        #endregion RiconAdmin İçin kurulum

        #region Ricon Kurulum

        public ActionResult Ricon() // ricon page dolduruyoruz
        {
            // Oturum kontrolü yapılır, eğer kullanıcı oturum açmışsa devam edilir.
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Oturumda "Sonuc" adında bir değişken varsa, bu değer ViewBag'e aktarılır.
                if (Session["Sonuc"] != null)
                {
                    ViewBag.Sonuc = Session["Sonuc"].ToString();
                    Session["Sonuc"] = null; // Sonuc değerini temizle
                }

                // Eğer "InfoTitle" ve "SmsMessage" oturum değişkenleri mevcutsa, bunlar da ViewBag'e aktarılır.
                if ((Session["InfoTitle"] != null) && (Session["SmsMessage"] != null))
                {
                    ViewBag.InfoTitle = Session["InfoTitle"].ToString();
                    ViewBag.SmsMessage = Session["SmsMessage"].ToString();
                    Session["InfoTitle"] = null; // Sonuc değerini temizle
                    Session["SmsMessage"] = null; // Sonuc değerini temizle
                }

                // Kullanıcı adı boş değilse devam edilir.
                if (kullanici != "")
                {
                    // SelectList nesneleri oluşturulur ve ViewBag'e eklenir.
                    ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");

                    // KurulumViewModel sınıfından bir nesne oluşturulur ve bazı alanlar başlangıç değerleriyle doldurulur.
                    var viewModel = new KurulumViewModel
                    {
                        Ricon_SN = "", // Boş değerle başlatılıyor
                        Site_Name = "",
                        Gsm_Number = "",
                    };

                    // Oluşturulan view model, görüntülenmek üzere View'a gönderilir.
                    return View(viewModel);
                }
                else
                {
                    // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                    return RedirectToAction("Login", "Login");
                }
            }
            else
            {
                // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                return RedirectToAction("Login", "Login");
            }
        }

        [HttpPost]
        public ActionResult Ricon(KurulumViewModel model)
        {
            try
            {
                // Kullanıcı girişi yapılmış mı kontrol ediliyor
                if (Session["UserName"] != null)
                {
                    string kullanici = Session["UserName"].ToString();

                    string company_id = Session["Operator"].ToString();

                    if (kullanici != "")
                    {
                        // Seçilen Seri No ve Konum kontrol ediliyor
                        if (model.SelectedSeriNoID == null || model.SelectedGsmNoID == null)
                        {
                            // Her ikisi de seçilmemişse hata mesajları ayarlanıyor
                            if (model.SelectedLocationID == null)
                            {
                                ViewBag.LocationID = CultureHelper.GetResourceKey("L119");
                            }
                            if (model.SelectedSeriNoID == null)
                            {
                                ViewBag.SeriNoID = CultureHelper.GetResourceKey("L120");
                            }
                            if (model.SelectedGsmNoID == null)
                            {
                                ViewBag.GsmNoID = CultureHelper.GetResourceKey("L315");
                            }

                            ViewBag.Sonuc = CultureHelper.GetResourceKey("L122");
                        }
                        else
                        {
                            ViewBag.Sonuc = "";
                            Session["SelectedSeriNoID"] = model.SelectedSeriNoID;
                            Session["SelectedLocationID"] = model.SelectedLocationID;
                            Session["SelectedGsmNoID"] = model.SelectedGsmNoID;

                            //kullanici = Session["UserName"].ToString();
                            //company_id = Session["Operator"].ToString();
                            //string secilen_gsm = Session["SelectedGsmNoID"].ToString();
                            //string secilen_operator;
                            // Seçilen Seri No'ya göre veritabanından kayıt sorgulanıyor
                            var query_ricon_sr = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).FirstOrDefault();

                            if (query_ricon_sr != null)
                            {
                                // Seçilen GSM No belirleniyor
                                string secilen_gsm = model.SelectedGsmNoID;
                                var orange = ctx.Tbl_Orange.Where(x => x.GSM_No1 == model.SelectedGsmNoID).FirstOrDefault();
                                var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == model.SelectedGsmNoID).FirstOrDefault();
                                if (orange != null)
                                {
                                    // Seçilen GSM Tbl_Orange tablosunda bulunuyor
                                    secilen_operator = "orange";
                                }
                                else if (inwi != null)
                                {
                                    // Seçilen GSM Tbl_Inwi tablosunda bulunuyor
                                    secilen_operator = "inwi";
                                }
                                else
                                {
                                    secilen_operator = "null";
                                }

                                if (model.chechMulti2)
                                {
                                    // chkMulti2 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-2";
                                }
                                else if (model.chechMulti3)
                                {
                                    // chkMulti3 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-3";
                                }
                                else
                                {
                                    model.SelectedLocationID = model.SelectedLocationID + "-1";
                                }
                                a.selected_riconsn = model.SelectedSeriNoID;
                                a.selected_operator = secilen_operator;
                                a.selected_gsm = secilen_gsm;
                                Session["SelectedGMSNO"] = secilen_gsm;
                                Session["SelectedOperator"] = secilen_operator;
                                Session["SelectedLocation"] = model.SelectedLocationID;
                                // Seçilen konumun veritabanında kontrolü yapılıyor
                                var gsmRecords = ctx.Install.Where(x => x.GSM_No == model.SelectedGsmNoID).ToList();

                                // Seçilen seri numarası ile ilgili kayıtları çek
                                var serialNumberRecords = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).ToList();

                                // Hem GSM numarası hem de seri numarası ile ilgili kayıtları çek
                                var commonRecords = ctx.Install
                                    .Where(x => x.GSM_No == model.SelectedGsmNoID && x.Ricon_SN == model.SelectedSeriNoID)
                                    .ToList();
                                if (secilen_operator == "null" && gsmRecords.Count == 0)
                                {
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.Operator = CultureHelper.GetResourceKey("L353");
                                }
                                // İlgili kayıtların sayısına bakarak kontrol yap
                                else if (gsmRecords.Count > 0 && serialNumberRecords.Count > 0)
                                {
                                    // Hem GSM hem de seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.CaseMessageYesNo = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else if (gsmRecords.Count > 0 || serialNumberRecords.Count > 0)
                                {
                                    // Sadece GSM veya sadece seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.StartOKMessage = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else
                                {
                                    { // case 1
                                        ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                        ViewBag.StartOKMessage = CultureHelper.GetResourceKey("L239");
                                    }
                                }
                            }
                            else
                            {
                                string secilen_gsm = model.SelectedGsmNoID;
                                var orange = ctx.Tbl_Orange.Where(x => x.GSM_No1 == model.SelectedGsmNoID).FirstOrDefault();
                                var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == model.SelectedGsmNoID).FirstOrDefault();

                                if (orange != null)
                                {
                                    // Seçilen GSM Tbl_Orange tablosunda bulunuyor
                                    secilen_operator = "orange";
                                }
                                else if (inwi != null)
                                {
                                    // Seçilen GSM Tbl_Inwi tablosunda bulunuyor
                                    secilen_operator = "inwi";
                                }
                                else
                                {
                                    secilen_operator = "null";
                                }

                                if (model.chechMulti2)
                                {
                                    // chkMulti2 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-2";
                                }
                                else if (model.chechMulti3)
                                {
                                    // chkMulti3 seçiliyse yapılacak işlemler
                                    model.SelectedLocationID = model.SelectedLocationID + "-3";
                                }
                                else
                                {
                                    model.SelectedLocationID = model.SelectedLocationID + "-1";
                                }
                                a.selected_riconsn = model.SelectedSeriNoID;
                                a.selected_operator = secilen_operator;
                                a.selected_gsm = secilen_gsm;
                                Session["SelectedGMSNO"] = secilen_gsm;
                                Session["SelectedOperator"] = secilen_operator;
                                Session["SelectedLocation"] = model.SelectedLocationID;

                                var gsmRecords = ctx.Install.Where(x => x.GSM_No == model.SelectedGsmNoID).ToList();

                                // Seçilen seri numarası ile ilgili kayıtları çek
                                var serialNumberRecords = ctx.Install.Where(x => x.Ricon_SN == model.SelectedSeriNoID).ToList();

                                // Hem GSM numarası hem de seri numarası ile ilgili kayıtları çek
                                var commonRecords = ctx.Install
                                    .Where(x => x.GSM_No == model.SelectedGsmNoID && x.Ricon_SN == model.SelectedSeriNoID)
                                    .ToList();
                                if (secilen_operator == "null" && gsmRecords.Count == 0)
                                {
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.Operator = CultureHelper.GetResourceKey("L353");
                                }
                                // İlgili kayıtların sayısına bakarak kontrol yap
                                else if (gsmRecords.Count > 0 && serialNumberRecords.Count > 0)
                                {
                                    // Hem GSM hem de seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.CaseMessageYesNo = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else if (gsmRecords.Count > 0 || serialNumberRecords.Count > 0)
                                {
                                    // Sadece GSM veya sadece seri numarası ile ilgili kayıt varsa, burada işlem yapabilirsiniz.
                                    // Örneğin, ViewBag.StartOKMessage = ... gibi bir şey yapabilirsiniz.
                                    ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                    ViewBag.CaseMessageYesNo = CultureHelper.GetResourceKey("L210");
                                }
                                else
                                {
                                    { // case 1
                                        ViewBag.InfoTitle = CultureHelper.GetResourceKey("L201");
                                        ViewBag.StartOKMessage = CultureHelper.GetResourceKey("L239");
                                    }
                                }
                            }
                        }

                        // SelectList nesneleri oluşturulur ve ViewBag'e eklenir.
                        ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                        var viewModel = new KurulumViewModel
                        {
                            Ricon_SN = "", // Boş değerle başlatılıyor
                            Site_Name = "",
                            Gsm_Number = "",
                        };

                        Session["Sonuc"] = " ";
                        Session["SmsMessage"] = null;
                        return View(viewModel);
                    }
                    else
                    {
                        // Kullanıcı girişi yapılmamışsa giriş sayfasına yönlendiriliyor
                        return RedirectToAction("Login", "Login");
                    }
                }
                else
                {
                    // Kullanıcı oturumu yoksa, giriş sayfasına yönlendirilir.
                    return RedirectToAction("Login", "Login");
                }
            }
            catch (Exception)
            {
                // Hata durumu
                ViewBag.Sonuc = CultureHelper.GetResourceKey("L234");
                string kullanici = Session["UserName"].ToString();
                string company_id = Session["Operator"].ToString();
                ViewBag.SeriNo = new SelectList(new List<SelectListItem>(), "Value", "Text");
                ViewBag.Location = new SelectList(new List<SelectListItem>(), "Value", "Text");
                ViewBag.GsmNo = new SelectList(new List<SelectListItem>(), "Value", "Text");

                var viewModel = new KurulumViewModel
                {
                    Ricon_SN = "", // Boş değerle başlatılıyor
                    Site_Name = "",
                    Gsm_Number = "",
                };
                return View(viewModel);
            }
        }

        #endregion Ricon Kurulum

        //#region EXCELL

        ////#region Excel Format SITE

        ////public SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString);

        ////[HttpPost]
        ////public ActionResult UploadExcel(HttpPostedFileBase file)
        ////{
        ////    try
        ////    {
        ////        if (file != null)
        ////        {
        ////            string filename = Path.GetFileName(file.FileName);
        ////            string folderPath = Server.MapPath("~/excelfolder/");

        ////            // Create the folder if it doesn't exist
        ////            if (!Directory.Exists(folderPath))
        ////            {
        ////                Directory.CreateDirectory(folderPath);
        ////            }

        ////            string filepath = Path.Combine(folderPath, filename);
        ////            file.SaveAs(filepath);

        ////            UploadResult uploadResult = UploadExcel1(filepath, filename);
        ////            // UploadResult türünde bir değişken kullan
        ////            if (uploadResult.Success && uploadResult.ErrorRows.Count == 0)
        ////            {
        ////                TempData["sonuc"] = "Upload is Successfully"; // Tüm satırlar başarıyla yüklendi.
        ////                return Json(new { success = true });
        ////            }
        ////            else if (uploadResult.Success && uploadResult.ErrorRows.Count > 0)
        ////            {
        ////                TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
        ////                return Json(new { success = true, errorRows = uploadResult.ErrorRows });
        ////            }
        ////            else
        ////            {
        ////                TempData["sonuc"] = "Upload is not Successfully";
        ////                return Json(new { success = false, errorRows = uploadResult.ErrorRows });
        ////            }
        ////        }
        ////        else
        ////        {
        ////            TempData["sonuc"] = "File not selected.";
        ////            return RedirectToAction("Index");
        ////        }
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        TempData["sonuc"] = "There was an error loading the file: " + e.Message;
        ////        return Json(new { success = false, message = e.Message });
        ////    }
        ////}

        ////private OleDbConnection Econ;

        ////private void ExcelCon(string filepath)
        ////{
        ////    string constr = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + "Extended Properties=" + "\"" + "Excel 12.0;HDR=YES;" + "\"";
        ////    Econ = new OleDbConnection(constr);
        ////}

        ////private UploadResult UploadExcel1(string filepath, string filename)
        ////{
        ////    UploadResult result = new UploadResult();
        ////    try
        ////    {
        ////        Thread.Sleep(10000);
        ////        string fullpath = Server.MapPath("/excelfolder/") + filename;

        ////        ExcelCon(fullpath);

        ////        string query = string.Format("Select * from [{0}]", "Sayfa1$");

        ////        OleDbCommand Ecom = new OleDbCommand(query, Econ);

        ////        Econ.Open();

        ////        DataSet ds = new DataSet();

        ////        OleDbDataAdapter oda = new OleDbDataAdapter(query, Econ);

        ////        oda.Fill(ds);

        ////        Econ.Close();

        ////        DataTable dt = ds.Tables[0];
        ////        List<int> errorRows = new List<int>();
        ////        int currentRow = 2;
        ////        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
        ////        {
        ////            con.Open();

        ////            using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
        ////            {
        ////                objbulk.DestinationTableName = "Sites";
        ////                objbulk.ColumnMappings.Add("Site_Name", "Site_Name");
        ////                objbulk.ColumnMappings.Add("Company_ID", "Company_ID");
        ////                objbulk.ColumnMappings.Add("Status", "Status");
        ////                DataTable filteredTable = dt.Clone(); // Aynı şema ile yeni bir tablo oluştur
        ////                filteredTable.Columns.Add("Company_ID", typeof(int));
        ////                filteredTable.Columns.Add("Status", typeof(int));

        ////                var uniqueLoc = ctx.Sites.Select(x => x.Site_Name).Distinct().ToList();
        ////                Dictionary<string, bool> locDictionary = new Dictionary<string, bool>();
        ////                foreach (DataRow row in dt.Rows)
        ////                {
        ////                    string sitename = row["Site_Name"].ToString();

        ////                    // Eğer bu Seri numarası daha önce eklenmişse, bu satırı hatalı olarak işaretle ve atla
        ////                    if (uniqueLoc.Contains(sitename))
        ////                    {
        ////                        errorRows.Add(currentRow);
        ////                        currentRow++;
        ////                        continue;
        ////                    }

        ////                    if (locDictionary.ContainsKey(sitename) && locDictionary[sitename])
        ////                    {
        ////                        errorRows.Add(currentRow);
        ////                        currentRow++;
        ////                        continue;
        ////                    }

        ////                    // Koşulları kontrol et
        ////                    if (!string.IsNullOrEmpty(sitename))
        ////                    {
        ////                        sitename = row["Site_Name"].ToString();
        ////                        DataRow newRow = filteredTable.NewRow();
        ////                        newRow["Site_Name"] = sitename;
        ////                        newRow["Company_ID"] = 2; // Company_ID'yi sabit bir değerle doldurabilirsiniz
        ////                        newRow["Status"] = 1; // Company_ID'yi sabit bir değerle doldurabilirsiniz

        ////                        filteredTable.Rows.Add(newRow);
        ////                    }
        ////                    else
        ////                    {
        ////                        // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
        ////                        errorRows.Add(currentRow);
        ////                    }
        ////                    locDictionary[sitename] = true;
        ////                    currentRow++;
        ////                }

        ////                objbulk.WriteToServer(filteredTable); // Filtrelenmiş tabloyu kullanarak veriyi ekleyin
        ////            }
        ////            con.Close();
        ////        }

        ////        if (errorRows.Count > 0)
        ////        {
        ////            result.Success = true; // Hata olmasına rağmen işlem başarılıdır
        ////            result.ErrorRows = errorRows;
        ////        }
        ////        else
        ////        {
        ////            result.Success = true;
        ////            result.ErrorRows = new List<int>();  // Boş bir hata listesi
        ////        }

        ////        return result;
        ////    }
        ////    catch (Exception)
        ////    {
        ////        result.Success = false; // Hata olduğunda işlem başarısızdır
        ////        return result;
        ////    }
        ////}

        ////#endregion Excel Format SITE

        //#region Excel Format SIM

        //[HttpPost]
        //public ActionResult UploadExcelSIM(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (file != null)
        //        {
        //            string filename = Path.GetFileName(file.FileName);
        //            string folderPath = Server.MapPath("~/excelfolder/");

        //            // Create the folder if it doesn't exist
        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //            string filepath = Path.Combine(folderPath, filename);
        //            file.SaveAs(filepath);

        //            UploadResult uploadResult1 = UploadExcelSIM1(filepath, filename); // UploadResult türünde bir değişken kullan

        //            if (uploadResult1.Success && uploadResult1.ErrorRows.Count == 0)
        //            {
        //                TempData["sonuc"] = "Upload is Successfully"; // Tüm satırlar başarıyla yüklendi.
        //                return Json(new { success = true });
        //            }
        //            else if (uploadResult1.Success && uploadResult1.ErrorRows.Count > 0)
        //            {
        //                TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
        //                return Json(new { success = true, errorRows = uploadResult1.ErrorRows });
        //            }
        //            else
        //            {
        //                TempData["sonuc"] = "Upload is not Successfully";
        //                return Json(new { success = false, errorRows = uploadResult1.ErrorRows });
        //            }
        //        }
        //        else
        //        {
        //            TempData["sonuc"] = "File not selected.";
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["sonuc"] = "There was an error loading the file: " + e.Message;
        //        return Json(new { success = false, message = e.Message });
        //    }
        //}

        //private OleDbConnection Econn;

        //private void ExcelConnec(string filepath)
        //{
        //    string cons = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + "Extended Properties=" + "\"" + "Excel 12.0;HDR=YES;" + "\"";
        //    Econn = new OleDbConnection(cons);
        //}

        //private UploadResult UploadExcelSIM1(string filepath, string filename)
        //{
        //    UploadResult result = new UploadResult();
        //    try
        //    {
        //        Thread.Sleep(10000);
        //        string fullpath = Server.MapPath("/excelfolder/") + filename;
        //        ExcelConnec(fullpath);

        //        string query = string.Format("Select * from [{0}]", "Sayfa1$");
        //        OleDbCommand Ecom = new OleDbCommand(query, Econn);
        //        Econn.Open();

        //        DataSet ds = new DataSet();
        //        OleDbDataAdapter oda = new OleDbDataAdapter(query, Econn);
        //        oda.Fill(ds);
        //        Econn.Close();

        //        DataTable dt = ds.Tables[0];

        //        List<int> errorRows = new List<int>();
        //        int currentRow = 2;
        //        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
        //        {
        //            con.Open();
        //            using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
        //            {
        //                objbulk.DestinationTableName = "SIMCards3";

        //                objbulk.ColumnMappings.Add("GSM_No1", "GSM_No1");
        //                objbulk.ColumnMappings.Add("APN1_Name", "APN1_Name");
        //                objbulk.ColumnMappings.Add("APN1_Username", "APN1_Username");
        //                objbulk.ColumnMappings.Add("APN1_Password", "APN1_Password");
        //                objbulk.ColumnMappings.Add("GSM_No2", "GSM_No2");
        //                objbulk.ColumnMappings.Add("APN2_Name", "APN2_Name");
        //                objbulk.ColumnMappings.Add("APN2_Username", "APN2_Username");
        //                objbulk.ColumnMappings.Add("APN2_Password", "APN2_Password");
        //                objbulk.ColumnMappings.Add("Lan1_IP", "Lan1_IP");
        //                objbulk.ColumnMappings.Add("Lan1_SubnetMask", "Lan1_SubnetMask");
        //                objbulk.ColumnMappings.Add("Lan_Subnet", "Lan_Subnet");
        //                objbulk.ColumnMappings.Add("Ricon_SN", "Ricon_SN");

        //                DataTable filteredTable = dt.Clone(); // Aynı şema ile yeni bir tablo oluştur

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    string gsmNo1 = row["GSM_No1"].ToString();
        //                    string apn1Name = row["APN1_Name"].ToString();
        //                    string lan1IP = row["Lan1_IP"].ToString();
        //                    string lanSubnet = row["Lan_Subnet"].ToString();

        //                    // Koşulları kontrol et
        //                    if (!string.IsNullOrEmpty(gsmNo1) && !string.IsNullOrEmpty(apn1Name) &&
        //                        !string.IsNullOrEmpty(lan1IP) && !string.IsNullOrEmpty(lanSubnet))
        //                    {
        //                        filteredTable.ImportRow(row);
        //                    }
        //                    else
        //                    {
        //                        // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
        //                        errorRows.Add(currentRow);
        //                    }

        //                    currentRow++;
        //                }

        //                objbulk.WriteToServer(filteredTable); // Filtrelenmiş tabloyu kullanarak veriyi ekleyin
        //            }
        //            con.Close();
        //        }
        //        if (errorRows.Count > 0)
        //        {
        //            result.Success = true; // Hata olmasına rağmen işlem başarılıdır
        //            result.ErrorRows = errorRows;
        //        }
        //        else
        //        {
        //            result.Success = true;
        //            result.ErrorRows = new List<int>();  // Boş bir hata listesi// Hata olmadığında işlem başarılıdır
        //        }

        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        result.Success = false; // Hata olduğunda işlem başarısızdır
        //        return result;
        //    }
        //}

        //#endregion Excel Format SIM

        //#region Excel Format Seri

        //[HttpPost]
        //public ActionResult UploadExcelSeri(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (file != null)
        //        {
        //            string filename = Path.GetFileName(file.FileName);
        //            string folderPath = Server.MapPath("~/excelfolder/");

        //            // Create the folder if it doesn't exist
        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //            string filepath = Path.Combine(folderPath, filename);
        //            file.SaveAs(filepath);

        //            UploadResult uploadResult = UploadExcelSeri1(filepath, filename); // UploadResult türünde bir değişken kullan
        //            if (uploadResult.Success && uploadResult.ErrorRows.Count == 0)
        //            {
        //                TempData["sonuc"] = "Upload is Successfully"; // Tüm satırlar başarıyla yüklendi.
        //                return Json(new { success = true });
        //            }
        //            else if (uploadResult.Success && uploadResult.ErrorRows.Count > 0)
        //            {
        //                TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
        //                return Json(new { success = true, errorRows = uploadResult.ErrorRows });
        //            }
        //            else
        //            {
        //                TempData["sonuc"] = "Upload is not Successfully";
        //                return Json(new { success = false, errorRows = uploadResult.ErrorRows });
        //            }
        //        }
        //        else
        //        {
        //            TempData["sonuc"] = "File not selected.";
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["sonuc"] = "There was an error loading the file: " + e.Message;
        //        return Json(new { success = false, message = e.Message });
        //    }
        //}

        //private OleDbConnection EconSeri;

        //private void ExcelConnecSeri(string filepath)
        //{
        //    string conseri = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + "Extended Properties=" + "\"" + "Excel 12.0;HDR=YES;" + "\"";
        //    EconSeri = new OleDbConnection(conseri);
        //}

        //private UploadResult UploadExcelSeri1(string filepath, string filename)
        //{
        //    UploadResult result = new UploadResult();
        //    try
        //    {
        //        Thread.Sleep(10000);
        //        string fullpath = Server.MapPath("/excelfolder/") + filename;
        //        ExcelConnecSeri(fullpath);

        //        string query = string.Format("Select * from [{0}]", "Sayfa1$");
        //        OleDbCommand Ecom = new OleDbCommand(query, EconSeri);
        //        EconSeri.Open();

        //        DataSet ds = new DataSet();
        //        OleDbDataAdapter oda = new OleDbDataAdapter(query, EconSeri);
        //        oda.Fill(ds);
        //        EconSeri.Close();

        //        DataTable dt = ds.Tables[0];

        //        List<int> errorRows = new List<int>();
        //        int currentRow = 2;
        //        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
        //        {
        //            con.Open();
        //            using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
        //            {
        //                objbulk.DestinationTableName = "Device";

        //                objbulk.ColumnMappings.Add("Ricon_SN", "Ricon_SN");
        //                objbulk.ColumnMappings.Add("Company_ID", "Company_ID");
        //                objbulk.ColumnMappings.Add("Status", "Status");
        //                objbulk.ColumnMappings.Add("Device_Type_ID", "Device_Type_ID");

        //                DataTable filteredTable = dt.Clone(); // Aynı şema ile yeni bir tablo oluştur
        //                filteredTable.Columns.Add("Company_ID", typeof(int));
        //                filteredTable.Columns.Add("Status", typeof(int));
        //                filteredTable.Columns.Add("Device_Type_ID", typeof(int));

        //                var uniqueSeri = ctx.Device.Select(x => x.Ricon_SN).Distinct().ToList();
        //                Dictionary<string, bool> seriDictionary = new Dictionary<string, bool>(); // Okunan GSM numaralarını ve eklenip eklenmediğini takip etmek için bir sözlük oluşturun

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    string serino1 = row["Ricon_SN"].ToString();

        //                    // Eğer bu Seri numarası daha önce eklenmişse, bu satırı hatalı olarak işaretle ve atla
        //                    if (uniqueSeri.Contains(serino1))
        //                    {
        //                        errorRows.Add(currentRow);
        //                        currentRow++;
        //                        continue;
        //                    }

        //                    if (seriDictionary.ContainsKey(serino1) && seriDictionary[serino1])
        //                    {
        //                        errorRows.Add(currentRow);
        //                        currentRow++;
        //                        continue;
        //                    }
        //                    // Koşulları kontrol et
        //                    if (!string.IsNullOrEmpty(serino1))
        //                    {
        //                        serino1 = row["Ricon_SN"].ToString();
        //                        DataRow newRow = filteredTable.NewRow();
        //                        newRow["Ricon_SN"] = serino1;
        //                        newRow["Company_ID"] = 2; // Company_ID'yi sabit bir değerle doldurabilirsiniz
        //                        newRow["Status"] = 1; // Status sabit bir değerle doldurabilirsiniz
        //                        newRow["Device_Type_ID"] = 1; // Device_Type_ID'yi sabit bir değerle doldurabilirsiniz
        //                        filteredTable.Rows.Add(newRow);
        //                    }
        //                    else
        //                    {
        //                        // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
        //                        errorRows.Add(currentRow);
        //                    }
        //                    seriDictionary[serino1] = true;
        //                    currentRow++;
        //                }

        //                objbulk.WriteToServer(filteredTable); // Filtrelenmiş tabloyu kullanarak veriyi ekleyin
        //            }
        //            con.Close();
        //        }
        //        if (errorRows.Count > 0)
        //        {
        //            result.Success = true; // Hata olmasına rağmen işlem başarılıdır
        //            result.ErrorRows = errorRows;
        //        }
        //        else
        //        {
        //            result.Success = true;
        //            result.ErrorRows = new List<int>();  // Boş bir hata listesi
        //        }

        //        return result;
        //    }
        //    catch (Exception)
        //    {
        //        result.Success = false; // Hata olduğunda işlem başarısızdır
        //        return result;
        //    }
        //}

        //#endregion Excel Format Seri

        //#region Excel Format Account

        //[HttpPost]
        //public ActionResult UploadExcelAccount(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (file != null)
        //        {
        //            string filename = Path.GetFileName(file.FileName);
        //            string folderPath = Server.MapPath("~/excelfolder/");

        //            // Create the folder if it doesn't exist
        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //            string filepath = Path.Combine(folderPath, filename);
        //            file.SaveAs(filepath);

        //            UploadResult uploadResult = UploadExcelAccount1(filepath, filename); // UploadResult türünde bir değişken kullan
        //            if (uploadResult.Success && uploadResult.ErrorRows.Count == 0)
        //            {
        //                TempData["sonuc"] = "Upload is Successfully"; // Tüm satırlar başarıyla yüklendi.
        //                return Json(new { success = true });
        //            }
        //            else if (uploadResult.Success && uploadResult.ErrorRows.Count > 0)
        //            {
        //                TempData["sonuc"] = "Some rows were uploaded successfully, but there are errors in other rows.";
        //                return Json(new { success = true, errorRows = uploadResult.ErrorRows });
        //            }
        //            else
        //            {
        //                TempData["sonuc"] = "Upload is not Successfully";
        //                return Json(new { success = false, errorRows = uploadResult.ErrorRows });
        //            }
        //        }
        //        else
        //        {
        //            TempData["sonuc"] = "File not selected.";
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["sonuc"] = "There was an error loading the file: " + e.Message;
        //        return Json(new { success = false, message = e.Message });
        //    }
        //}

        //private OleDbConnection EconAccount;

        //private void ExcelConnecAccount(string filepath)
        //{
        //    string conacc = @"Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filepath + ";" + "Extended Properties=" + "\"" + "Excel 12.0;HDR=YES;" + "\"";
        //    EconAccount = new OleDbConnection(conacc);
        //}

        //private UploadResult UploadExcelAccount1(string filepath, string filename)
        //{
        //    UploadResult result = new UploadResult();
        //    string kullanici = Session["UserName"].ToString();
        //    DateTime islemSaati = DateTime.Now;
        //    try
        //    {
        //        Thread.Sleep(10000);
        //        string fullpath = Server.MapPath("/excelfolder/") + filename;
        //        ExcelConnecAccount(fullpath);

        //        string query = string.Format("Select * from [{0}]", "Sayfa1$");
        //        OleDbCommand Ecom = new OleDbCommand(query, EconAccount);
        //        EconAccount.Open();

        //        DataSet ds = new DataSet();
        //        OleDbDataAdapter oda = new OleDbDataAdapter(query, EconAccount);
        //        oda.Fill(ds);
        //        EconAccount.Close();

        //        DataTable dt = ds.Tables[0];

        //        List<int> errorRows = new List<int>();
        //        int currentRow = 2;
        //        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
        //        {
        //            con.Open();
        //            using (SqlBulkCopy objbulk = new SqlBulkCopy(con))
        //            {
        //                objbulk.DestinationTableName = "UserLogin";

        //                objbulk.ColumnMappings.Add("Username", "Username");
        //                objbulk.ColumnMappings.Add("Password", "Password");
        //                objbulk.ColumnMappings.Add("Type", "IsAdmin"); // New column mapping
        //                objbulk.ColumnMappings.Add("Company_ID", "Company_ID");
        //                objbulk.ColumnMappings.Add("Status", "Status");
        //                objbulk.ColumnMappings.Add("Creator", "Creator");
        //                objbulk.ColumnMappings.Add("Create_DateTime", "Create_DateTime");

        //                DataTable filteredTable = dt.Clone(); // Aynı şema ile yeni bir tablo oluştur
        //                filteredTable.Columns.Add("Company_ID", typeof(int));
        //                filteredTable.Columns.Add("Status", typeof(int));
        //                filteredTable.Columns.Add("Creator", typeof(string));
        //                filteredTable.Columns.Add("Create_DateTime", typeof(DateTime));

        //                var uniqueSeri = ctx.UserLogin.Select(x => x.Username).Distinct().ToList();
        //                Dictionary<string, bool> seriDictionary = new Dictionary<string, bool>(); // Okunan GSM numaralarını ve eklenip eklenmediğini takip etmek için bir sözlük oluşturun

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    string uname = row["Username"].ToString();
        //                    string pass = row["Password"].ToString();
        //                    string type = row["Type"].ToString();
        //                    // Eğer bu Seri numarası daha önce eklenmişse, bu satırı hatalı olarak işaretle ve atla
        //                    bool isAdmin = false; // Varsayılan olarak "false" yapalım.

        //                    // "Type" değeri "Admin" ise "IsAdmin"ı "true" olarak ayarla
        //                    if (!string.IsNullOrEmpty(type) && type.ToLower() == "admin")
        //                    {
        //                        if (type.ToLower() == "admin")
        //                        {
        //                            isAdmin = true;
        //                        }
        //                    }

        //                    if (uniqueSeri.Contains(uname))
        //                    {
        //                        errorRows.Add(currentRow);
        //                        currentRow++;
        //                        continue;
        //                    }

        //                    if (seriDictionary.ContainsKey(uname) && seriDictionary[uname])
        //                    {
        //                        errorRows.Add(currentRow);
        //                        currentRow++;
        //                        continue;
        //                    }
        //                    // Koşulları kontrol et
        //                    if (!string.IsNullOrEmpty(uname) && !string.IsNullOrEmpty(pass))
        //                    {
        //                        DataRow newRow = filteredTable.NewRow();
        //                        newRow["Username"] = uname;
        //                        newRow["Password"] = pass;
        //                        newRow["Type"] = isAdmin;
        //                        newRow["Company_ID"] = 2; // Company_ID'yi sabit bir değerle doldurabilirsiniz
        //                        newRow["Status"] = 1; // Status sabit bir değerle doldurabilirsiniz
        //                        newRow["Creator"] = kullanici; // Device_Type_ID'yi sabit bir değerle doldurabilirsiniz
        //                        newRow["Create_DateTime"] = islemSaati; // Device_Type_ID'yi sabit bir değerle doldurabilirsiniz

        //                        filteredTable.Rows.Add(newRow);
        //                    }
        //                    else
        //                    {
        //                        // Koşulları sağlamayan satırın numarasını hata listesine ekleyin
        //                        errorRows.Add(currentRow);
        //                    }
        //                    seriDictionary[uname] = true;
        //                    currentRow++;
        //                }

        //                objbulk.WriteToServer(filteredTable); // Filtrelenmiş tabloyu kullanarak veriyi ekleyin
        //            }
        //            con.Close();
        //        }
        //        if (errorRows.Count > 0)
        //        {
        //            result.Success = true; // Hata olmasına rağmen işlem başarılıdır
        //            result.ErrorRows = errorRows;
        //        }
        //        else
        //        {
        //            result.Success = true;
        //            result.ErrorRows = new List<int>();  // Boş bir hata listesi
        //        }

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        EventLog log = new EventLog("Application");
        //        log.Source = "My Application";
        //        log.WriteEntry("Hata: " + ex.Message, EventLogEntryType.Error);
        //        result.Success = false; // Hata olduğunda işlem başarısızdır
        //        return result;
        //    }
        //}

        //#endregion Excel Format Account

        //#endregion EXCELL

        #region EDITT

        #region Edit RiconOrange

        [HttpGet]
        public ActionResult EditRiconOrange()
        {
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                string useRname = Session["UserName"].ToString();
                if (useRname != null)
                {
                    ViewBag.Gsmno = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    var orangemodel = new OrangeDataModel
                    {
                        GSM_No1 = "",
                        Ricon_SN = "",
                        APN1_Name = "",
                        APN1_Username = "",
                        APN1_Password = "",
                        WAN_ip = "",
                        vlanid_TG = "", // Boş değerle başlat
                        lan_ip_TG = "",
                        lan_subnet_TG = "",
                        lan_subnetmask_TG = "",
                        vlanid_Servizi = "",
                        lan_ip_Servizi = "",
                        lan_subnet_Servizi = "",
                        lan_subnetmask_Servizi = "",
                        Tunnel_dc1_r1 = "",
                        Tunnel_dc2_r1 = "",
                        Tunnel_ig_r1 = "",
                        Tg_dhcp_start = "",
                        Tg_dhcp_end = "",
                        Ser_dhcp_start = "",
                        Ser_dhcp_end = "",
                    };

                    return View(orangemodel);
                }
            }

            return RedirectToAction("EditRiconOrange", "Home");
        }

        [HttpPost]
        public ActionResult EditRiconOrange(OrangeDataModel orangemodel)
        {
            if (ModelState.IsValid)
            {   // Retrieve the existing SIMCards3 data from the database
                var orangeData = ctx.Tbl_Orange.FirstOrDefault(x => x.GSM_No1 == orangemodel.Gsmno);
                if (orangeData != null)
                {
                    string oldorange = orangeData.GSM_No1;

                    orangeData.GSM_No1 = orangemodel.GSM_No1;
                    orangeData.Ricon_SN = orangemodel.Ricon_SN;
                    orangeData.APN1_Name = orangemodel.APN1_Name;
                    orangeData.APN1_Username = orangemodel.APN1_Username;
                    orangeData.APN1_Password = orangemodel.APN1_Password;
                    orangeData.WAN_ip = orangemodel.WAN_ip;
                    orangeData.vlanid_TG = orangemodel.vlanid_TG;
                    orangeData.lan_ip_TG = orangemodel.lan_ip_TG;
                    orangeData.lan_subnet_TG = orangemodel.lan_subnet_TG;
                    orangeData.lan_subnetmask_TG = orangemodel.lan_subnetmask_TG;
                    orangeData.vlanid_Servizi = orangemodel.vlanid_Servizi;
                    orangeData.lan_ip_Servizi = orangemodel.lan_ip_Servizi;
                    orangeData.lan_subnet_Servizi = orangemodel.lan_subnet_Servizi;
                    orangeData.lan_subnetmask_Servizi = orangemodel.lan_subnetmask_Servizi;
                    orangeData.Tunnel_dc2_r1 = orangemodel.Tunnel_dc2_r1;
                    orangeData.Tunnel_ig_r1 = orangemodel.Tunnel_ig_r1;
                    orangeData.Tg_dhcp_start = orangemodel.Tg_dhcp_start;
                    orangeData.Tg_dhcp_end = orangemodel.Tg_dhcp_end;
                    orangeData.Ser_dhcp_start = orangemodel.Ser_dhcp_start;
                    orangeData.Ser_dhcp_end = orangemodel.Ser_dhcp_end;

                    // Save the changes to the database
                    ctx.SaveChanges();
                    if (oldorange != orangemodel.GSM_No1)
                    {
                        var gsmNumber = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == oldorange);
                        if (gsmNumber != null)
                        {
                            ctx.GsmNumber.Remove(gsmNumber);
                            var newgsmNumber = new GsmNumber
                            {
                                GSM_No = orangemodel.GSM_No1,
                                Status = 1,
                                Company_ID = 2
                            };
                            ctx.GsmNumber.Add(newgsmNumber);
                            ctx.SaveChanges();
                        }
                    }
                    // Redirect to the Edit page or show a success message
                    return RedirectToAction("EditRiconOrange");
                }
            }
            // If the model is not valid or the data doesn't exist, return to the edit page with the model
            return View(orangemodel);
        }

        public ActionResult GetOrangeData(string gsmNo)
        {
            if (Session["UserName"] != null)
            {
                //string useRname = Session["UserName"].ToString();
                var orangeData = ctx.Tbl_Orange.FirstOrDefault(x => x.GSM_No1 == gsmNo);

                //if (useRname != null)
                //{
                if (orangeData != null)
                {
                    var data = new
                    {
                        GSM_No1 = orangeData.GSM_No1,
                        Ricon_SN = orangeData.Ricon_SN,
                        APN1_Name = orangeData.APN1_Name,
                        APN1_Username = orangeData.APN1_Username,
                        APN1_Password = orangeData.APN1_Password,
                        WAN_ip = orangeData.WAN_ip,
                        vlanid_TG = orangeData.vlanid_TG,
                        lan_ip_TG = orangeData.lan_ip_TG,
                        lan_subnet_TG = orangeData.lan_subnet_TG,
                        lan_subnetmask_TG = orangeData.lan_subnetmask_TG,
                        vlanid_Servizi = orangeData.vlanid_Servizi,
                        lan_ip_Servizi = orangeData.lan_ip_Servizi,
                        lan_subnet_Servizi = orangeData.lan_subnet_Servizi,
                        lan_subnetmask_Servizi = orangeData.lan_subnetmask_Servizi,
                        Tunnel_dc1_r1 = orangeData.Tunnel_dc1_r1,
                        Tunnel_dc2_r1 = orangeData.Tunnel_dc2_r1,
                        Tunnel_ig_r1 = orangeData.Tunnel_ig_r1,
                        Tg_dhcp_start = orangeData.Tg_dhcp_start,
                        Tg_dhcp_end = orangeData.Tg_dhcp_end,
                        Ser_dhcp_start = orangeData.Ser_dhcp_start,
                        Ser_dhcp_end = orangeData.Ser_dhcp_end,
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                //}
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFilteredGSMOrange(string selectedValue)

        {
            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();

                if (userName != null)
                {
                    // Veritabanından tüm uygun GSM No'ları al ve SelectList olarak dönüştür
                    var allGSMs = ctx.Tbl_Orange.Where(x => x.Status == 1)
                        .Select(a => new SelectListItem
                        {
                            Text = a.GSM_No1,
                            Value = a.GSM_No1
                        })
                        .ToList();

                    // Eğer bir selectedValue varsa, seçilen değeri filtrele
                    if (!string.IsNullOrEmpty(selectedValue))
                    {
                        allGSMs = allGSMs.Where(x => x.Text.Contains(selectedValue)).ToList();
                    }

                    return Json(allGSMs, JsonRequestBehavior.AllowGet);
                }
            }

            // Oturum yoksa veya kullanıcı adı null ise veya filtreleme başarısız olursa null döndür
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveNewOrange(string GSM_No1, string Ricon_SN, string APN1_Name, string APN1_Username, string APN1_Password,
        string WAN_ip, string vlanid_TG, string lan_ip_TG, string lan_subnet_TG, string lan_subnetmask_TG,
        string vlanid_Servizi, string lan_ip_Servizi, string lan_subnet_Servizi, string lan_subnetmask_Servizi,
        string Tunnel_dc1_r1, string Tunnel_dc2_r1, string Tunnel_ig_r1,
        string Tg_dhcp_start, string Tg_dhcp_end, string Ser_dhcp_start, string Ser_dhcp_end)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(GSM_No1) && !string.IsNullOrWhiteSpace(APN1_Name) &&
                    !string.IsNullOrWhiteSpace(APN1_Username) && !string.IsNullOrWhiteSpace(APN1_Password) &&
                    !string.IsNullOrWhiteSpace(WAN_ip) && !string.IsNullOrWhiteSpace(vlanid_TG) &&
                    !string.IsNullOrWhiteSpace(lan_ip_TG) && !string.IsNullOrWhiteSpace(lan_subnet_TG) &&
                    !string.IsNullOrWhiteSpace(lan_subnetmask_TG) && !string.IsNullOrWhiteSpace(vlanid_Servizi) &&
                    !string.IsNullOrWhiteSpace(lan_ip_Servizi) && !string.IsNullOrWhiteSpace(lan_subnet_Servizi) &&
                    !string.IsNullOrWhiteSpace(lan_subnetmask_Servizi) && !string.IsNullOrWhiteSpace(Tunnel_dc1_r1)
                    && !string.IsNullOrWhiteSpace(Tunnel_dc2_r1) && !string.IsNullOrWhiteSpace(Tunnel_ig_r1) &&
                    !string.IsNullOrWhiteSpace(Tg_dhcp_start) && !string.IsNullOrWhiteSpace(Tg_dhcp_end) &&
                    !string.IsNullOrWhiteSpace(Ser_dhcp_start) && !string.IsNullOrWhiteSpace(Ser_dhcp_end))
                {
                    var existingOrange = ctx.Tbl_Orange.Where(x => x.Status == 1 || (x.Status == 0 && x.GSM_No1 == GSM_No1)).SingleOrDefault(i => i.GSM_No1 == GSM_No1);

                    if (existingOrange == null || existingOrange.Status == 0)
                    {
                        var newOrange = new Tbl_Orange(); // Varsayılan sınıfınıza göre düzenleyin
                        newOrange.GSM_No1 = GSM_No1;
                        newOrange.Ricon_SN = Ricon_SN;
                        newOrange.APN1_Name = APN1_Name;
                        newOrange.APN1_Username = APN1_Username;
                        newOrange.APN1_Password = APN1_Password;
                        newOrange.WAN_ip = WAN_ip;
                        newOrange.vlanid_TG = vlanid_TG;
                        newOrange.lan_ip_TG = lan_ip_TG;
                        newOrange.lan_subnet_TG = lan_subnet_TG;
                        newOrange.lan_subnetmask_TG = lan_subnetmask_TG;
                        newOrange.vlanid_Servizi = vlanid_Servizi;
                        newOrange.lan_ip_Servizi = lan_ip_Servizi;
                        newOrange.lan_subnet_Servizi = lan_subnet_Servizi;
                        newOrange.lan_subnetmask_Servizi = lan_subnetmask_Servizi;
                        newOrange.Tunnel_dc1_r1 = Tunnel_dc1_r1;
                        newOrange.Tunnel_dc2_r1 = Tunnel_dc2_r1;
                        newOrange.Tunnel_ig_r1 = Tunnel_ig_r1;
                        newOrange.Tg_dhcp_start = Tg_dhcp_start;
                        newOrange.Tg_dhcp_end = Tg_dhcp_end;
                        newOrange.Ser_dhcp_start = Ser_dhcp_start;
                        newOrange.Ser_dhcp_end = Ser_dhcp_end;

                        newOrange.Status = 1;

                        ctx.Tbl_Orange.Add(newOrange);
                        ctx.SaveChanges();

                        var existingGsm = ctx.GsmNumber.SingleOrDefault(g => g.GSM_No == GSM_No1);

                        if (existingGsm == null)
                        {
                            // GsmNumber tablosuna GsmNo2'yi ekle
                            var newGsm = new GsmNumber();
                            newGsm.GSM_No = GSM_No1;
                            newGsm.Status = 1;
                            newGsm.Company_ID = 2;

                            ctx.GsmNumber.Add(newGsm);
                            ctx.SaveChanges();
                        }
                        else if (existingGsm.Status == 0)
                        {
                            // Eğer GsmNumber tablosunda aynı GSM_No2 varsa ve status 0 ise, status'u 1'e çek
                            existingGsm.Status = 1;
                            ctx.SaveChanges();
                        }
                        return Json(new { success = true, message = Resources.Resources.L330 });
                    }
                    else
                    {
                        // Aynı GSM_No2 zaten Tbl_Inwi tablosunda var, hata mesajı döndür
                        return Json(new { success = false, message = Resources.Resources.L355 });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteOrange(string Gsmno)
        {
            try
            {
                // Veritabanında SIM kartı bul
                var orangeToDelete = ctx.Tbl_Orange.FirstOrDefault(s => s.GSM_No1 == Gsmno);

                if (orangeToDelete != null)
                {
                    // SIM kartını silmek yerine durumu 0 olarak işaretle
                    orangeToDelete.Status = 0;
                    ctx.SaveChanges();
                    var gsmOrange = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == orangeToDelete.GSM_No1);
                    if (gsmOrange != null)
                    {
                        gsmOrange.Status = 0;
                        ctx.SaveChanges();
                    }

                    return Json(new { success = true, message = Resources.Resources.L329 });
                }
                else
                {
                    return Json(new { success = false, message = "No Data" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult EditInwi()
        {
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                string useRname = Session["UserName"].ToString();
                if (useRname != null)
                {
                    ViewBag.Gsmno = new SelectList(new List<SelectListItem>(), "Value", "Text");
                    var inwimodel = new InwiDataModel
                    {
                        GSM_No2 = "",
                        i_Ricon_SN = "",
                        APN2_Name = "",
                        APN2_Username = "",
                        APN2_Password = "",
                        i_WAN_ip = "",
                        i_vlanid_TG = "", // Boş değerle başlat
                        i_lan_ip_TG = "",
                        i_lan_subnet_TG = "",
                        i_lan_subnetmask_TG = "",
                        i_vlanid_Servizi = "",
                        i_lan_ip_Servizi = "",
                        i_lan_subnet_Servizi = "",
                        i_lan_subnetmask_Servizi = "",
                        i_Tg_dhcp_start = "",
                        i_Tg_dhcp_end = "",
                        i_Ser_dhcp_start = "",
                        i_Ser_dhcp_end = "",
                    };

                    return View(inwimodel);
                }
            }

            return RedirectToAction("EditRiconInwi", "Home");
        }

        [HttpPost]
        public ActionResult EditInwi(InwiDataModel inwimodel)
        {
            if (ModelState.IsValid)
            {   // Retrieve the existing SIMCards3 data from the database
                var inwiData = ctx.Tbl_Inwi.FirstOrDefault(x => x.GSM_No2 == inwimodel.Gsmno);
                if (inwiData != null)
                {
                    string oldinwi = inwiData.GSM_No2;

                    inwiData.GSM_No2 = inwimodel.GSM_No2;
                    inwiData.Ricon_SN = inwimodel.i_Ricon_SN;
                    inwiData.APN2_Name = inwimodel.APN2_Name;
                    inwiData.APN2_Username = inwimodel.APN2_Username;
                    inwiData.APN2_Password = inwimodel.APN2_Password;
                    inwiData.i_WAN_ip = inwimodel.i_WAN_ip;
                    inwiData.i_vlanid_TG = inwimodel.i_vlanid_TG;
                    inwiData.i_lan_ip_TG = inwimodel.i_lan_ip_TG;
                    inwiData.i_lan_subnet_TG = inwimodel.i_lan_subnet_TG;
                    inwiData.i_lan_subnetmask_TG = inwimodel.i_lan_subnetmask_TG;
                    inwiData.i_vlanid_Servizi = inwimodel.i_vlanid_Servizi;
                    inwiData.i_lan_ip_Servizi = inwimodel.i_lan_ip_Servizi;
                    inwiData.i_lan_subnet_Servizi = inwimodel.i_lan_subnet_Servizi;
                    inwiData.i_lan_subnetmask_Servizi = inwimodel.i_lan_subnetmask_Servizi;
                    inwiData.i_Tg_dhcp_start = inwimodel.i_Tg_dhcp_start;
                    inwiData.i_Tg_dhcp_end = inwimodel.i_Tg_dhcp_end;
                    inwiData.i_Ser_dhcp_start = inwimodel.i_Ser_dhcp_start;
                    inwiData.i_Ser_dhcp_end = inwimodel.i_Ser_dhcp_end;

                    // Save the changes to the database
                    ctx.SaveChanges();
                    if (oldinwi != inwimodel.GSM_No2)
                    {
                        var gsmNumber = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == oldinwi);
                        if (gsmNumber != null)
                        {
                            ctx.GsmNumber.Remove(gsmNumber);
                            var newgsmNumber = new GsmNumber
                            {
                                GSM_No = inwimodel.GSM_No2,
                                Status = 1,
                                Company_ID = 2
                            };
                            ctx.GsmNumber.Add(newgsmNumber);
                            ctx.SaveChanges();
                        }
                    }

                    // Redirect to the Edit page or show a success message
                    return RedirectToAction("EditInwi");
                }
            }
            // If the model is not valid or the data doesn't exist, return to the edit page with the model
            return View(inwimodel);
        }

        public ActionResult GetInwiData(string gsmNo)
        {
            if (Session["UserName"] != null)
            {
                //string useRname = Session["UserName"].ToString();
                var inwiData = ctx.Tbl_Inwi.FirstOrDefault(x => x.GSM_No2 == gsmNo);

                //if (useRname != null)
                //{
                if (inwiData != null)
                {
                    var data = new
                    {
                        GSM_No2 = inwiData.GSM_No2,
                        i_Ricon_SN = inwiData.Ricon_SN,
                        APN2_Name = inwiData.APN2_Name,
                        APN2_Username = inwiData.APN2_Username,
                        APN2_Password = inwiData.APN2_Password,
                        WAN_ip = inwiData.i_WAN_ip,
                        vlanid_TG = inwiData.i_vlanid_TG,
                        lan_ip_TG = inwiData.i_lan_ip_TG,
                        lan_subnet_TG = inwiData.i_lan_subnet_TG,
                        lan_subnetmask_TG = inwiData.i_lan_subnetmask_TG,
                        vlanid_Servizi = inwiData.i_vlanid_Servizi,
                        lan_ip_Servizi = inwiData.i_lan_ip_Servizi,
                        lan_subnet_Servizi = inwiData.i_lan_subnet_Servizi,
                        lan_subnetmask_Servizi = inwiData.i_lan_subnetmask_Servizi,
                        i_Tg_dhcp_start = inwiData.i_Tg_dhcp_start,
                        i_Tg_dhcp_end = inwiData.i_Tg_dhcp_end,
                        i_Ser_dhcp_start = inwiData.i_Ser_dhcp_start,
                        i_Ser_dhcp_end = inwiData.i_Ser_dhcp_end,
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                //}
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFilteredGSMInwi(string selectedValue)
        {
            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();

                if (userName != null)
                {
                    // Veritabanından tüm uygun GSM No'ları al ve SelectList olarak dönüştür
                    var allGSMs = ctx.Tbl_Inwi.Where(x => x.Status == 1)
                        .Select(a => new SelectListItem
                        {
                            Text = a.GSM_No2,
                            Value = a.GSM_No2
                        })
                        .ToList();

                    // Eğer bir selectedValue varsa, seçilen değeri filtrele
                    if (!string.IsNullOrEmpty(selectedValue))
                    {
                        allGSMs = allGSMs.Where(x => x.Text.Contains(selectedValue)).ToList();
                    }

                    return Json(allGSMs, JsonRequestBehavior.AllowGet);
                }
            }

            // Oturum yoksa veya kullanıcı adı null ise veya filtreleme başarısız olursa null döndür
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveNewInwi(string GsmNo2, string RiconSn, string Apn2Name, string Apn2UserName, string Apn2Password,
            string i_WAN_ip, string i_vlanid_TG, string i_lan_ip_TG, string i_lan_subnet_TG, string i_lan_subnetmask_TG,
            string i_vlanid_Servizi, string i_lan_ip_Servizi, string i_lan_subnet_Servizi, string i_lan_subnetmask_Servizi,
            string i_Tg_dhcp_start, string i_Tg_dhcp_end, string i_Ser_dhcp_start, string i_Ser_dhcp_end)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(GsmNo2) && !string.IsNullOrWhiteSpace(Apn2Name) &&
                    !string.IsNullOrWhiteSpace(Apn2UserName) && !string.IsNullOrWhiteSpace(Apn2Password) &&
                    !string.IsNullOrWhiteSpace(i_WAN_ip) && !string.IsNullOrWhiteSpace(i_vlanid_TG) &&
                    !string.IsNullOrWhiteSpace(i_lan_ip_TG) && !string.IsNullOrWhiteSpace(i_lan_subnet_TG) &&
                    !string.IsNullOrWhiteSpace(i_lan_subnetmask_TG) && !string.IsNullOrWhiteSpace(i_vlanid_Servizi) &&
                    !string.IsNullOrWhiteSpace(i_lan_ip_Servizi) && !string.IsNullOrWhiteSpace(i_lan_subnet_Servizi)
                    && !string.IsNullOrWhiteSpace(i_lan_subnetmask_Servizi) && !string.IsNullOrWhiteSpace(i_Tg_dhcp_start) &&
                    !string.IsNullOrWhiteSpace(i_Tg_dhcp_end) && !string.IsNullOrWhiteSpace(i_Ser_dhcp_start) && !string.IsNullOrWhiteSpace(i_Ser_dhcp_end))
                { // Tbl_Inwi tablosunda aynı GSM_No2 var mı kontrol et
                    var existingInwi = ctx.Tbl_Inwi.Where(x => x.Status == 1 || (x.Status == 0 && x.GSM_No2 == GsmNo2)).SingleOrDefault(i => i.GSM_No2 == GsmNo2);

                    if (existingInwi == null || existingInwi.Status == 0)
                    {
                        // Tbl_Inwi tablosunda aynı GSM_No2 yoksa, yeni kaydı ekle
                        var newInwi = new Tbl_Inwi(); // Varsayılan sınıfınıza göre düzenleyin
                        newInwi.GSM_No2 = GsmNo2;
                        newInwi.APN2_Name = Apn2Name;
                        newInwi.APN2_Username = Apn2UserName;
                        newInwi.APN2_Password = Apn2Password;
                        newInwi.i_WAN_ip = i_WAN_ip;
                        newInwi.i_vlanid_TG = i_vlanid_TG;
                        newInwi.i_lan_ip_TG = i_lan_ip_TG;
                        newInwi.i_lan_subnet_TG = i_lan_subnet_TG;
                        newInwi.i_lan_subnetmask_TG = i_lan_subnetmask_TG;
                        newInwi.i_vlanid_Servizi = i_vlanid_Servizi;
                        newInwi.i_lan_ip_Servizi = i_lan_ip_Servizi;
                        newInwi.i_lan_subnet_Servizi = i_lan_subnet_Servizi;
                        newInwi.i_lan_subnetmask_Servizi = i_lan_subnetmask_Servizi;
                        newInwi.Ricon_SN = RiconSn;
                        newInwi.i_Tg_dhcp_start = i_Tg_dhcp_start;
                        newInwi.i_Tg_dhcp_end = i_Tg_dhcp_end;
                        newInwi.i_Ser_dhcp_start = i_Ser_dhcp_start;
                        newInwi.i_Ser_dhcp_end = i_Ser_dhcp_end;
                        newInwi.Status = 1;

                        ctx.Tbl_Inwi.Add(newInwi);
                        ctx.SaveChanges();

                        var existingGsm = ctx.GsmNumber.SingleOrDefault(g => g.GSM_No == GsmNo2);

                        if (existingGsm == null)
                        {
                            // GsmNumber tablosuna GsmNo2'yi ekle
                            var newGsm = new GsmNumber();
                            newGsm.GSM_No = GsmNo2;
                            newGsm.Status = 1;
                            newGsm.Company_ID = 2;

                            ctx.GsmNumber.Add(newGsm);
                            ctx.SaveChanges();
                        }
                        else if (existingGsm.Status == 0)
                        {
                            // Eğer GsmNumber tablosunda aynı GSM_No2 varsa ve status 0 ise, status'u 1'e çek
                            existingGsm.Status = 1;
                            ctx.SaveChanges();
                        }

                        return Json(new { success = true, message = Resources.Resources.L332 });
                    }
                    else
                    {
                        // Aynı GSM_No2 zaten Tbl_Inwi tablosunda var, hata mesajı döndür
                        return Json(new { success = false, message = Resources.Resources.L354 });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Invalid data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteInwi(string Gsmno)
        {
            try
            {
                // Veritabanında SIM kartı bul
                var inwiToDelete = ctx.Tbl_Inwi.FirstOrDefault(s => s.GSM_No2 == Gsmno);

                if (inwiToDelete != null)
                {
                    // SIM kartını silmek yerine durumu 0 olarak işaretle
                    inwiToDelete.Status = 0;
                    ctx.SaveChanges();
                    var gsmInwi = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == inwiToDelete.GSM_No2);
                    if (gsmInwi != null)
                    {
                        gsmInwi.Status = 0;
                        ctx.SaveChanges();
                    }

                    return Json(new { success = true, message = Resources.Resources.L331 });
                }
                else
                {
                    return Json(new { success = false, message = "No Data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion Edit RiconOrange

        #region Edit Ricon Lokasyon

        [HttpGet]
        public ActionResult EditRiconLocation()
        {
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                ViewBag.SiteName = new SelectList(new List<SelectListItem>(), "Value", "Text");

                var viewModel = new AllModel
                {
                    Site_Name = "", // Boş değerle başlat
                    Company_ID = Convert.ToInt32(null),
                };

                return View(viewModel);
            }

            return RedirectToAction("EditRiconLocation", "Home");
        }

        [HttpPost]
        public ActionResult EditRiconLocation(AllModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing Site data from the database
                var siteData = ctx.Sites.FirstOrDefault(x => x.Site_Name == model.SiteName && x.Status == 1);

                if (siteData != null)
                {
                    siteData.Site_Name = model.Site_Name;
                    siteData.Company_ID = 2;

                    ctx.SaveChanges();

                    return RedirectToAction("EditRiconLocation");
                }
            }
            return View(model);
        }

        #endregion Edit Ricon Lokasyon

        #region Edit Ricon SeriNumarası

        [HttpGet]
        public ActionResult EditRiconSerialNumber()
        {
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                ViewBag.RiconSeri = new SelectList(new List<SelectListItem>(), "Value", "Text");

                var viewModel = new AllModel
                {
                    Ricon_SN = "", // Boş değerle başlat
                    Company_ID = Convert.ToInt32(null),
                };

                return View(viewModel);
            }

            return RedirectToAction("EditRiconSerialNumber", "Home");
        }

        [HttpPost]
        public ActionResult EditRiconSerialNumber(AllModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing Site data from the database
                var serialdata = ctx.Device.FirstOrDefault(x => x.Ricon_SN == model.RiconSeri);

                if (serialdata != null)
                {
                    serialdata.Ricon_SN = model.Ricon_SN;
                    serialdata.Company_ID = 2;

                    ctx.SaveChanges();

                    return RedirectToAction("EditRiconSerialNumber");
                }
            }
            return View(model);
        }

        public ActionResult GetSeriDataRicon(string riconseri)
        {
            if (Session["UserName"] != null)
            {
                var serialdata = ctx.Device.FirstOrDefault(x => x.Ricon_SN == riconseri);
                if (serialdata != null)
                {
                    var data = new
                    {
                        Ricon_SN = serialdata.Ricon_SN,
                        Company_ID = serialdata.Company_ID,
                        Device_Type_ID = serialdata.Device_Type_ID,
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSiteData(string sitename)
        {
            if (Session["UserName"] != null)
            {
                var siteData = ctx.Sites.FirstOrDefault(x => x.Site_Name == sitename && x.Status == 1);
                if (siteData != null)
                {
                    var data = new
                    {
                        Site_Name = siteData.Site_Name,
                        Company_ID = siteData.Company_ID,
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetFilteredSeriListRicon(string selectedValue)
        {
            // selectedValue'yi büyük harfe dönüştür.
            selectedValue = selectedValue.ToUpper();
            if (Session["UserName"] != null && Session["IsAdmin"] != null && (bool)Session["IsAdmin"] == true)
            {
                string userName = Session["UserName"].ToString();

                if (userName != null)
                {
                    // Veritabanından tüm uygun GSM No'ları al ve SelectList olarak dönüştür
                    var allSeri = ctx.Device
                        .Where(x => x.Company_ID == 2 && x.Device_Type_ID == 1 && x.Status == 1)
                        .Select(a => new SelectListItem
                        {
                            Text = a.Ricon_SN,
                            Value = a.Ricon_SN
                        })
                        .ToList();

                    // Eğer bir selectedValue varsa, seçilen değeri filtrele
                    if (!string.IsNullOrEmpty(selectedValue))
                    {
                        allSeri = allSeri.Where(x => x.Text.Contains(selectedValue)).ToList();
                    }

                    return Json(allSeri, JsonRequestBehavior.AllowGet);
                }
            }

            // Oturum yoksa veya kullanıcı adı null ise veya filtreleme başarısız olursa null döndür
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion Edit Ricon SeriNumarası

        #endregion EDITT

        #region SITES BUTON'S

        [HttpPost]
        public ActionResult SaveNewSeri(string newSeriNumber)
        {
            if (!string.IsNullOrWhiteSpace(newSeriNumber))
            {
                try
                {
                    var newSeri = new Device(); // Varsayılan sınıfınıza göre düzenle
                    newSeri.Ricon_SN = newSeriNumber; // Site_Name alanına yeni site adını ata
                    newSeri.Company_ID = 2;
                    newSeri.Device_Type_ID = 1;
                    newSeri.Status = 1;// Diğer özellikleri de doldur
                    ctx.Device.Add(newSeri);
                    ctx.SaveChanges();

                    return Json(new { success = true, message = Resources.Resources.L312 });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid Site name." });
        }

        [HttpPost]
        public ActionResult DeleteSeri(string RiconSeri)
        {
            try
            {
                var siteToDelete = ctx.Device.FirstOrDefault(s => s.Ricon_SN == RiconSeri);

                if (siteToDelete != null)
                {
                    siteToDelete.Status = 0;
                    ctx.SaveChanges();

                    return Json(new { success = true, message = Resources.Resources.L313 });
                }
                else
                {
                    return Json(new { success = false, message = "No Seri Data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveNewSite(string NewSiteName)
        {
            if (!string.IsNullOrWhiteSpace(NewSiteName))
            {
                try
                {
                    var newSite = new Sites(); // Varsayılan sınıfınıza göre düzenle
                    newSite.Site_Name = NewSiteName; // Site_Name alanına yeni site adını ata
                    newSite.Company_ID = 2;
                    newSite.Status = 1;// Diğer özellikleri de doldur
                    ctx.Sites.Add(newSite);
                    ctx.SaveChanges();

                    return Json(new { success = true, message = Resources.Resources.L297 });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid Location ." });
        }

        [HttpPost]
        public ActionResult DeleteSite(string SiteName)
        {
            try
            {
                var siteToDelete = ctx.Sites.FirstOrDefault(s => s.Site_Name == SiteName);

                if (siteToDelete != null)
                {
                    siteToDelete.Status = 0;
                    ctx.SaveChanges();

                    return Json(new { success = true, message = Resources.Resources.L298 });
                }
                else
                {
                    return Json(new { success = false, message = "No Location Data." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion SITES BUTON'S

        #region Filtre

        #region GetFilteredGSM_NOList

        [HttpGet]
        public ActionResult GetFilteredGSMNOList(string selectedValue)
        {
            if (Session["UserName"] != null)
            {
                string userName = Session["UserName"].ToString();

                if (userName != null)
                {
                    // Veritabanından tüm uygun GSM No'ları al ve SelectList olarak dönüştür
                    var allGSM = ctx.GsmNumber
                        .Where(x => x.Company_ID == 2 && x.Status == 1)
                        .Select(a => new SelectListItem
                        {
                            Text = a.GSM_No ?? "",
                            Value = a.GSM_No ?? ""
                        })
                        .ToList();

                    // Eğer bir selectedValue varsa, seçilen değeri filtrele
                    if (!string.IsNullOrEmpty(selectedValue))
                    {
                        allGSM = allGSM.Where(x => x.Text.Contains(selectedValue)).ToList();
                    }

                    return Json(allGSM, JsonRequestBehavior.AllowGet);
                }
            }

            // Oturum yoksa veya kullanıcı adı null ise veya filtreleme başarısız olursa null döndür
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        #endregion GetFilteredGSM_NOList

        #region GetFilteredSeriNoList

        [HttpGet]
        public ActionResult GetFilteredSeriNoList(string selectedValue)
        {
            // Veritabanından tüm uygun GSM No'ları al ve SelectList olarak dönüştür
            var allSerino = ctx.Device.Where(x => x.Company_ID == 2 && x.Status == 1)
                .Select(a => new SelectListItem
                {
                    Text = a.Ricon_SN ?? "",
                    Value = a.Ricon_SN ?? ""
                })
                .ToList();

            if (!string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = selectedValue.ToLower(); // Küçük harf yap
                allSerino = allSerino.Where(x => x.Text.ToLower().Contains(selectedValue)).ToList();
            }

            return Json(allSerino, JsonRequestBehavior.AllowGet);
        }

        #endregion GetFilteredSeriNoList

        #region GetFilteredLocationList

        [HttpGet]
        public ActionResult GetFilteredLocationList(string selectedValue)
        {
            // Veritabanından tüm uygun Location'ları al ve SelectList olarak dönüştür
            var allLocation = ctx.Sites.Where(x => x.Company_ID == 2 && x.Status == 1)
                .Select(a => new SelectListItem
                {
                    Text = a.Site_Name,
                    Value = a.Site_Name
                })
                .ToList();

            if (!string.IsNullOrEmpty(selectedValue))
            {
                selectedValue = selectedValue.ToLower(); // Küçük harf yap
                allLocation = allLocation.Where(x => x.Text.ToLower().Contains(selectedValue)).ToList();
            }

            return Json(allLocation, JsonRequestBehavior.AllowGet);
        }

        #endregion GetFilteredLocationList

        #endregion Filtre

        #region view orange -Inwi

        [HttpPost]
        public ActionResult SaveOrange(OrangeDataModel orangemodel)
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();
                DateTime islemSaati = DateTime.Now;
                // Veri tabanı bağlantısı oluştur

                // Kullanıcıyı veri tabanından al
                var orangeData = ctx.Tbl_Orange.FirstOrDefault(x => x.Orange_ID == orangemodel.Orange_ID);
                if (orangeData != null)
                {
                    string oldorange = orangeData.GSM_No1;

                    orangeData.GSM_No1 = orangemodel.GSM_No1;
                    orangeData.Ricon_SN = orangemodel.Ricon_SN;
                    orangeData.APN1_Name = orangemodel.APN1_Name;
                    orangeData.APN1_Username = orangemodel.APN1_Username;
                    orangeData.APN1_Password = orangemodel.APN1_Password;
                    orangeData.WAN_ip = orangemodel.WAN_ip;
                    orangeData.vlanid_TG = orangemodel.vlanid_TG;
                    orangeData.lan_ip_TG = orangemodel.lan_ip_TG;
                    orangeData.lan_subnet_TG = orangemodel.lan_subnet_TG;
                    orangeData.lan_subnetmask_TG = orangemodel.lan_subnetmask_TG;
                    orangeData.vlanid_Servizi = orangemodel.vlanid_Servizi;
                    orangeData.lan_ip_Servizi = orangemodel.lan_ip_Servizi;
                    orangeData.lan_subnet_Servizi = orangemodel.lan_subnet_Servizi;
                    orangeData.lan_subnetmask_Servizi = orangemodel.lan_subnetmask_Servizi;
                    orangeData.Tunnel_dc2_r1 = orangemodel.Tunnel_dc2_r1;
                    orangeData.Tunnel_ig_r1 = orangemodel.Tunnel_ig_r1;
                    orangeData.Tg_dhcp_start = orangemodel.Tg_dhcp_start;
                    orangeData.Tg_dhcp_end = orangemodel.Tg_dhcp_end;
                    orangeData.Ser_dhcp_start = orangemodel.Ser_dhcp_start;
                    orangeData.Ser_dhcp_end = orangemodel.Ser_dhcp_end;

                    ctx.SaveChanges();

                    if (oldorange != orangemodel.GSM_No1)
                    {
                        var gsmNumber = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == oldorange);
                        if (gsmNumber != null)
                        {
                            ctx.GsmNumber.Remove(gsmNumber);
                            var newgsmNumber = new GsmNumber
                            {
                                GSM_No = orangemodel.GSM_No1,
                                Status = 1,
                                Company_ID = 2
                            };
                            ctx.GsmNumber.Add(newgsmNumber);
                            ctx.SaveChanges();
                        }
                    }
                    // Başarılı mesajı göster
                    return Json(new { success = true, message = Resources.Resources.L330 });
                }
            }

            return RedirectToAction("Login"); // Kullanıcıyı başka bir sayfaya yönlendir
        }

        [HttpPost]
        public ActionResult DelOrange(int Orange_ID)
        {
            try
            {
                // Veritabanında SIM kartı bul
                var orangeToDelete = ctx.Tbl_Orange.FirstOrDefault(s => s.Orange_ID == Orange_ID);

                if (orangeToDelete != null)
                {
                    // SIM kartını silmek yerine durumu 0 olarak işaretle
                    orangeToDelete.Status = 0;
                    ctx.SaveChanges();
                    var gsmOrange = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == orangeToDelete.GSM_No1);
                    if (gsmOrange != null)
                    {
                        gsmOrange.Status = 0;
                        ctx.SaveChanges();
                    }

                    return Json(new { success = true, message = Resources.Resources.L329 });
                }
                else
                {
                    return Json(new { success = false, message = "No Data" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SaveInwi(InwiDataModel inwimodel)
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();
                DateTime islemSaati = DateTime.Now;

                // Kullanıcıyı veri tabanından al
                var inwiData = ctx.Tbl_Inwi.FirstOrDefault(x => x.Inwi_ID == inwimodel.Inwi_ID);
                if (inwiData != null)
                {
                    string oldinwi = inwiData.GSM_No2;

                    inwiData.GSM_No2 = inwimodel.GSM_No2;
                    inwiData.Ricon_SN = inwimodel.i_Ricon_SN;
                    inwiData.APN2_Name = inwimodel.APN2_Name;
                    inwiData.APN2_Username = inwimodel.APN2_Username;
                    inwiData.APN2_Password = inwimodel.APN2_Password;
                    inwiData.i_WAN_ip = inwimodel.i_WAN_ip;
                    inwiData.i_vlanid_TG = inwimodel.i_vlanid_TG;
                    inwiData.i_lan_ip_TG = inwimodel.i_lan_ip_TG;
                    inwiData.i_lan_subnet_TG = inwimodel.i_lan_subnet_TG;
                    inwiData.i_lan_subnetmask_TG = inwimodel.i_lan_subnetmask_TG;
                    inwiData.i_vlanid_Servizi = inwimodel.i_vlanid_Servizi;
                    inwiData.i_lan_ip_Servizi = inwimodel.i_lan_ip_Servizi;
                    inwiData.i_lan_subnet_Servizi = inwimodel.i_lan_subnet_Servizi;
                    inwiData.i_lan_subnetmask_Servizi = inwimodel.i_lan_subnetmask_Servizi;
                    inwiData.i_Tg_dhcp_start = inwimodel.i_Tg_dhcp_start;
                    inwiData.i_Tg_dhcp_end = inwimodel.i_Tg_dhcp_end;
                    inwiData.i_Ser_dhcp_start = inwimodel.i_Ser_dhcp_start;
                    inwiData.i_Ser_dhcp_end = inwimodel.i_Ser_dhcp_end;

                    ctx.SaveChanges();

                    if (oldinwi != inwimodel.GSM_No2)
                    {
                        var gsmNumber = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == oldinwi);
                        if (gsmNumber != null)
                        {
                            ctx.GsmNumber.Remove(gsmNumber);
                            var newgsmNumber = new GsmNumber
                            {
                                GSM_No = inwimodel.GSM_No2,
                                Status = 1,
                                Company_ID = 2
                            };
                            ctx.GsmNumber.Add(newgsmNumber);
                            ctx.SaveChanges();
                        }
                    }
                    // Başarılı mesajı göster
                    return Json(new { success = true, message = Resources.Resources.L332 });
                }
            }

            return RedirectToAction("Login"); // Kullanıcıyı başka bir sayfaya yönlendir
        }

        [HttpPost]
        public ActionResult DelInwi(int Inwi_ID)
        {
            try
            {
                // Veritabanında SIM kartı bul
                var inwiToDelete = ctx.Tbl_Inwi.FirstOrDefault(s => s.Inwi_ID == Inwi_ID);

                if (inwiToDelete != null)
                {
                    // SIM kartını silmek yerine durumu 0 olarak işaretle
                    inwiToDelete.Status = 0;
                    ctx.SaveChanges();
                    var gsmInwi = ctx.GsmNumber.SingleOrDefault(x => x.GSM_No == inwiToDelete.GSM_No2);
                    if (gsmInwi != null)
                    {
                        gsmInwi.Status = 0;
                        ctx.SaveChanges();
                    }

                    return Json(new { success = true, message = Resources.Resources.L331 });
                }
                else
                {
                    return Json(new { success = false, message = "No Data" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion view orange -Inwi

        #region installation

        public JsonResult DeviceOrangeData()
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Install tablosundan kullanıcıya ait verileri seçin
                var query = ctx.Tbl_Orange
                 .Where(u => u.Status == 1)
                 .OrderByDescending(x => x.Orange_ID)
                    .ToList();

                return Json(query, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeviceInwiData()
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Install tablosundan kullanıcıya ait verileri seçin
                var query = ctx.Tbl_Inwi
                       .Where(u => u.Status == 1)
                    .OrderByDescending(x => x.Inwi_ID)
                    .ToList();

                return Json(query, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeviceInstallationData()
        {
            if (Session["UserName"] != null)
            {
                string kullanici = Session["UserName"].ToString();

                // Install tablosundan kullanıcıya ait verileri seçin
                var query = ctx.Install.OrderByDescending(x => x.Install_ID).ToList();

                return Json(query, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportToExcel()
        {
            try
            {
                // Install tablosundan verileri çekin
                List<Install> installData = ctx.Install.OrderByDescending(x => x.Install_ID).ToList();

                // Excel veri modeli oluşturun
                ExcelDataModel excelModel = new ExcelDataModel
                {
                    Headers = new List<string>
            {
                "Install_ID", "Ricon_SN", "GSM_No", "Site_Name", "WAN_ip", "Operator", "Username", "Company_ID", "Date_Time"
            },
                    Rows = installData.Select(install => new List<string>
            {
                install.Install_ID.ToString(),
                install.Ricon_SN,
                install.GSM_No,
                install.Site_Name,
                install.WAN_ip,
                install.Operator,
                install.Username,
                install.Company_ID.ToString(),
                install.Date_Time.ToString()
            }).ToList()
                };

                // Yeni bir ExcelPackage oluşturun
                using (var package = new ExcelPackage())
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.Add("InstallData");

                    // Sütun başlıklarını ekleyin
                    for (int i = 0; i < excelModel.Headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = excelModel.Headers[i];
                    }

                    // Veri satırlarını ekleyin
                    for (int i = 0; i < excelModel.Rows.Count; i++)
                    {
                        for (int j = 0; j < excelModel.Rows[i].Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1].Value = excelModel.Rows[i][j];
                        }
                    }

                    // Dosyayı sunucuda oluşturun
                    byte[] excelBytes = package.GetAsByteArray();
                    System.IO.File.WriteAllBytes(Server.MapPath("~/Content/InstallData.xlsx"), excelBytes);
                }

                // Başarılı yanıtı döndürün
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Hata olursa hata mesajını döndürün
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult DownloadExcel()
        {
            var fileBytes = System.IO.File.ReadAllBytes(Server.MapPath("~/Content/InstallData.xlsx"));
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InstallData.xlsx");
        }

        #endregion installation

        [HttpPost]
        public ActionResult StartOk()
        {
            string kullanici = Session["UserName"].ToString();
            string company_id = Session["Operator"].ToString();
            string sideid = Session["SelectedLocationID"].ToString();
            string serino = Session["SelectedSeriNoID"].ToString();
            string secilen_gsm = Session["SelectedGMSNO"].ToString();
            string secilen_operator = Session["SelectedOperator"].ToString();
            string secilen_location = Session["SelectedLocation"].ToString();
            var control_gsm = ctx.Install.Where(x => x.GSM_No == secilen_gsm).FirstOrDefault();

            Install install_query = new Install();
            bool deger = false;
            bool deger = true;
            deger = a.ConfigAzure(secilen_gsm, secilen_operator, serino, ViewBag); //Seçilen GSM no a göre Config mesajları gönderiliyor.

            if (deger == true) //Config metodundan dönen değer true ise yani işlem yapılmış ise

            {
                string wanIP = null;
                if (secilen_operator.ToLower() == "orange")
                {
                    var orangeRecord = ctx.Tbl_Orange.FirstOrDefault(x => x.GSM_No1 == secilen_gsm);
                    if (orangeRecord != null)
                    {
                        wanIP = orangeRecord.WAN_ip;// Orange için WAN IP'sini alın
                    }
                }
                else
                {
                    var inwiRecord = ctx.Tbl_Inwi.FirstOrDefault(x => x.GSM_No2 == secilen_gsm);
                    if (inwiRecord != null)
                    {
                        wanIP = inwiRecord.i_WAN_ip; // Inwi için WAN IP'sini alın
                    }
                }

                int lastindex = 1;

                var maxInstallID = ctx.Install.Max(p => (int?)p.Install_ID);

                // Eğer maxInstallID null değilse, en büyük değeri kullan
                if (maxInstallID.HasValue)
                {
                    lastindex = maxInstallID.Value;
                }

                install_query.Username = kullanici;
                install_query.Ricon_SN = serino;
                //install_query.Site_Name = sideid;
                install_query.GSM_No = secilen_gsm;
                install_query.WAN_ip = wanIP;
                install_query.Operator = secilen_operator;
                install_query.Date_Time = DateTime.Now;
                install_query.Company_ID = Convert.ToInt32(company_id);
                install_query.Install_ID = (lastindex + 1);
                install_query.Site_Name = secilen_location;
                //  install_query.Default_GSMNo = default_sim;

                ctx.Install.Add(install_query);
                ctx.SaveChanges(); // güncelle

                Session["Sonuc"] = "<b>SerialNumber:</b> " + serino + "<br /><b>Gsm No:</b> " + secilen_gsm + "<br /><b>Location:</b> " + secilen_location + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");
                ViewBag.Sonuc = "<b>SerialNumber:</b> " + serino + "<br /><b>Gsm No:</b> " + secilen_gsm + "<br /><b>Location:</b> " + secilen_location + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");

                return Json(true);
            }
            else
            {
                Session["Sonuc"] = CultureHelper.GetResourceKey("L125");

                ViewBag.Sonuc = CultureHelper.GetResourceKey("L125");
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult GetConfirm()
        {
            string kullanici = Session["UserName"].ToString();
            string company_id = Session["Operator"].ToString();
            string sideid = Session["SelectedLocationID"].ToString();
            string serino = Session["SelectedSeriNoID"].ToString();
            string secilen_gsm = Session["SelectedGMSNO"].ToString();
            string secilen_operator = Session["SelectedOperator"].ToString();
            string secilen_location = Session["SelectedLocation"].ToString();

            Install install_query = new Install();
            bool deger = false;

            //var control_serino = ctx.SIMCards3.Where(x => x.Ricon_SN == serino).FirstOrDefault();
            var serinoRecords = ctx.Install.Where(x => x.Ricon_SN == serino).ToList();
            ctx.Install.RemoveRange(serinoRecords);
            ctx.SaveChanges();
            var gsmRecords = ctx.Install.Where(x => x.GSM_No == secilen_gsm).ToList();
            ctx.Install.RemoveRange(gsmRecords);
            ctx.SaveChanges();
            deger = a.ConfigAzure(secilen_gsm, secilen_operator, serino, ViewBag); //Seçilen GSM no a göre Config mesajları gönderiliyor.

            if (deger == true) //Config metodundan dönen değer true ise yani işlem yapılmış ise
            {
                string wanIP = null;
                if (secilen_operator.ToLower() == "orange")
                {
                    var orangeRecord = ctx.Tbl_Orange.FirstOrDefault(x => x.GSM_No1 == secilen_gsm);
                    if (orangeRecord != null)
                    {
                        wanIP = orangeRecord.WAN_ip;// Orange için WAN IP'sini alın
                    }
                }
                else
                {
                    var inwiRecord = ctx.Tbl_Inwi.FirstOrDefault(x => x.GSM_No2 == secilen_gsm);
                    if (inwiRecord != null)
                    {
                        wanIP = inwiRecord.i_WAN_ip; // Inwi için WAN IP'sini alın
                    }
                }

                int lastindex = 1;

                var maxInstallID = ctx.Install.Max(p => (int?)p.Install_ID);

                // Eğer maxInstallID null değilse, en büyük değeri kullan
                if (maxInstallID.HasValue)
                {
                    lastindex = maxInstallID.Value;
                }

                if (!ctx.Install.Any(x => x.Ricon_SN == serino || x.GSM_No == secilen_gsm))
                {
                    install_query.Username = kullanici;
                    install_query.Ricon_SN = serino;
                    //install_query.Site_Name = sideid;
                    install_query.GSM_No = secilen_gsm;
                    install_query.WAN_ip = wanIP;
                    install_query.Operator = secilen_operator;
                    install_query.Date_Time = DateTime.Now;
                    install_query.Company_ID = Convert.ToInt32(company_id);
                    install_query.Site_Name = secilen_location;

                    ctx.Install.Add(install_query);
                    ctx.SaveChanges();
                }
                Session["Sonuc"] = "<b>SerialNumber:</b> " + serino + "<br /> <b>Gsm No:</b> " + secilen_gsm + "<br /><b>Location:</b> " + secilen_location + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");
                ViewBag.Sonuc = "<b>SerialNumber:</b> " + serino + "<br /> <b>Gsm No:</b> " + secilen_gsm + "<br /><b>Location:</b> " + secilen_location + "<br /><br />" + CultureHelper.GetResourceKey("L339") + "<br /><br /><br />" + CultureHelper.GetResourceKey("L124");

                return Json(true);
            }
            else
            {
                Session["Sonuc"] = CultureHelper.GetResourceKey("L125");

                ViewBag.Sonuc = CultureHelper.GetResourceKey("L125");

                return Json(false);
            }
        }
    }
}