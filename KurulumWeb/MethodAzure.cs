using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KurulumWeb.Helper;
using KurulumWeb.Models;
using SMSApi.Api;
using SNMPDB;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace KurulumWeb
{
    public class MethodAzure
    {
        private RiconApps_FASEntities1 ctx = new RiconApps_FASEntities1();

        public string selected_gsm { get; set; }
        public string selected_riconsn { get; set; }
        public string selected_operator { get; set; }

        public bool ConfigAzure(string GSM, string secilen_operator, string selected_riconserino, dynamic ViewBag) //Configuration yapacak metot
        {
            try
            {
                if (secilen_operator.ToLower()=="orange")
                {
                    //var query = ctx.SIMCards3.Where(x => x.GSM_No1 == GSM || x.GSM_No2 == GSM).FirstOrDefault(); // Gelen GSM numarasına göre veri tabanından veri getir.
                    var orangeQuery = ctx.Tbl_Orange.Where(x => x.GSM_No1 == GSM).FirstOrDefault();

                    if (orangeQuery != null) //veri boş değilse
                    {
                        var orange = ctx.Tbl_Orange.Where(x => x.GSM_No1 == GSM).FirstOrDefault();
                        var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == GSM).FirstOrDefault();
                        var query_device = ctx.Device.Where(x => x.Ricon_SN == selected_riconserino).FirstOrDefault(); //simcards tablosundan getirdiği kişinin Operatorüne göre device tablosundan deviceları  getiriyor
                        var query_rms = ctx.RMSC.Where(x => x.Company_ID == 2).FirstOrDefault(); //Login tablosundan getirdiği kişinin Operatorüne göre rmcs tablosundan listeyi getiriyor

                        var install_query = ctx.Install.Where(x => x.Operator == secilen_operator).FirstOrDefault();
                        if (query_device != null)
                        {
                            string username = "subscriptions@riconmobile.com";
                            string password = "Hedef2023!";
                            string title = "";
                            string donus = ""; //jet sms metodunun bize gönderdiği değer
                            int denemeSayisi = 0;
                            // msj göndermek için olan deneme sayısı
                            if (query_device.Device_Type_ID != 1)
                            {
                            }
                            else if (query_device.Device_Type_ID == 1)
                            {
                                var serino = ctx.Tbl_Orange.Where(x => x.Ricon_SN == selected_riconserino).FirstOrDefault();
                                string log_message = "";

                                do
                                {//phase1 p11 ikev1
                                    string msj1 = "set modem 0" + "\n" +
                                       "apn sisaljm.orange.ma" + "\n";


                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 1)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }
                                    denemeSayisi++;
                                }

                                while (donus != "DELIVERED" && denemeSayisi <= 1);
                                log_message = CultureHelper.GetResourceKey("L235") + " \n";
                                denemeSayisi = 0;
                                Thread.Sleep(20000);

                            }
                        }

                    }

                }
                else 
                {
                    var inwiQuery = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == GSM).FirstOrDefault();
                    if (inwiQuery!=null)
                    {


                        var inwi = ctx.Tbl_Inwi.Where(x => x.GSM_No2 == GSM).FirstOrDefault();
                        var query_device = ctx.Device.Where(x => x.Ricon_SN == selected_riconserino).FirstOrDefault(); //simcards tablosundan getirdiği kişinin Operatorüne göre device tablosundan deviceları  getiriyor
                        var query_rms = ctx.RMSC.Where(x => x.Company_ID == 2).FirstOrDefault(); //Login tablosundan getirdiği kişinin Operatorüne göre rmcs tablosundan listeyi getiriyor

                        var install_query = ctx.Install.Where(x => x.Operator == secilen_operator).FirstOrDefault();

                        if (query_device != null)
                        {
                            string username = "subscriptions@riconmobile.com";
                            string password = "Hedef2023!";
                            string title = "";
                            string donus = ""; //jet sms metodunun bize gönderdiği değer
                            int denemeSayisi = 0;
                            // msj göndermek için olan deneme sayısı
                            if (query_device.Device_Type_ID != 1)
                            {
                            }
                            else if (query_device.Device_Type_ID == 1)
                            {
                                var serino = ctx.Tbl_Inwi.Where(x => x.Ricon_SN == selected_riconserino).FirstOrDefault();
                                string log_message = "";


                                do
                                {//phase1 p11 ikev1
                                    string msj1 = "set modem 0" + "\n" +
                                    "apn sisaljma" + "\n" +
                                    "username sisaljma" + "\n" +
                                    "password sisaljma" + "\n";


                                    bool isfast = false;
                                    if (denemeSayisi != 0)
                                    {
                                        isfast = true;
                                    }
                                    donus = sendMessageAzure(GSM, msj1, username, password, title, isfast);
                                    if (denemeSayisi == 1)
                                    {
                                        if (donus == "UNDELIVERED" || donus == "SENT")
                                        {
                                            string failed_sms = CultureHelper.GetResourceKey("L236");

                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
                                            return false;
                                        }
                                    }

                                    denemeSayisi++;
                                }

                                while (donus != "DELIVERED" && denemeSayisi <= 1);
                                log_message = CultureHelper.GetResourceKey("L235") + " \n";
                                denemeSayisi = 0;
                                Thread.Sleep(20000);

                            }


                        }
                    }
                  
                }

                return true;
               
            }

            catch (System.Exception e)
            {
                //install_query.Date_Time = DateTime.Now;
                e.Message.ToString();
                return true;
            }
        }

        public string sendMessageAzure(string GSM_No, string mesaj, string username, string password, string title, bool fast)
        {
            string m_result = "";

            try
            {
                SMSApi.Api.IClient client = new SMSApi.Api.ClientOAuth("MiQksF5buJOGtXvqCvOWpl3PuCDQYABA5C3oUv4f");

                var smsApi = new SMSApi.Api.SMSFactory(client, ProxyAddress.SmsApiCom);

                var result =
                    smsApi.ActionSend()
                        .SetText(mesaj)
                        .SetTo(GSM_No)
                        .SetSender("Ricon")
                        .SetFast(fast)
                        .Execute();

                string[] ids = new string[result.Count];

                for (int i = 0, l = 0; i < result.List.Count; i++)
                {
                    if (!result.List[i].isError())
                    {
                        if (!result.List[i].isFinal())
                        {
                            ids[l] = result.List[i].ID;
                            l++;
                        }
                    }
                }

                result =
                  smsApi.ActionGet()
                        .Ids(ids)
                        .Execute();
                int time_count = 0;
                while ((result.List[0].Status != "DELIVERED") && (result.List[0].Status != "UNDELIVERED") && time_count < 80)
                {
                    Thread.Sleep(1000);
                    time_count++;

                    result =
                    smsApi.ActionGet()
                       .Ids(ids)
                       .Execute();
                }

                m_result = result.List[0].Status;
            }
            catch (SMSApi.Api.ActionException e)
            {
                /**
                 * Action error
                 */
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            catch (SMSApi.Api.ClientException e)
            {
                /**
                 * Error codes (list available in smsapi docs). Example:
                 * 101 	Invalid authorization info
                 * 102 	Invalid username or password
                 * 103 	Insufficient credits on Your account
                 * 104 	No such template
                 * 105 	Wrong IP address (for IP filter turned on)
                 * 110	Action not allowed for your account
                 */
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            catch (SMSApi.Api.HostException e)
            {
                /*
                 * Server errors
                 * SMSApi.Api.HostException.E_JSON_DECODE - problem with parsing data
                 */
                System.Console.WriteLine(e.Message);
            }
            catch (SMSApi.Api.ProxyException e)
            {
                // communication problem between client and sever
                System.Console.WriteLine(e.Message);
                return e.Message;
            }
            return m_result;
        }

    }
}