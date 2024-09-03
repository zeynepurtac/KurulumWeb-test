//using KurulumWeb.Helper;
//using KurulumWeb.Models;
//using SMSApi.Api;
//using SNMPDB;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Web;
//using System.Web.Mvc;

//namespace KurulumWeb
//{
//    public class Methods
//    {
//        private RiconAppsDBEntities2 ctx = new RiconAppsDBEntities2();

//        public string selected_gsm { get; set; }
//        public string selected_riconsn { get; set; }

//        public bool Config(string GSM, string selected_riconserino, dynamic ViewBag) //Configuration yapacak metot
//        {
//            //ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//            //ViewBag.SmsMessage = CultureHelper.GetResourceKey("L220");

//            try
//            {
//                var query = ctx.SIMCards3.Where(x => x.GSM_No1 == GSM || x.GSM_No2 == GSM).FirstOrDefault(); // Gelen GSM numarasına göre veri tabanından veri getir.

//                if (query != null) //veri boş değilse
//                {
//                    var query_device = ctx.Device.Where(x => x.Company_ID == query.Company_ID).FirstOrDefault(); //simcards tablosundan getirdiği kişinin Operatorüne göre device tablosundan deviceları  getiriyor
//                    var query_rms = ctx.RMSC.Where(x => x.Company_ID == query.Company_ID).FirstOrDefault(); //Login tablosundan getirdiği kişinin Operatorüne göre rmcs tablosundan listeyi getiriyor getiriyor

//                    var install_query = ctx.Install.Where(x => x.GSM_No1 == GSM || x.GSM_No2 == GSM).FirstOrDefault();
//                    if (query_device != null)
//                    {
//                        string username = "subscriptions@riconmobile.com";
//                        string password = "Hedef2023!";
//                        string title = "";
//                        string donus = ""; //jet sms metodunun bize gönderdiği değer
//                        int denemeSayisi = 0; //msj göndermek için olan deneme sayısı
//                        if (query_device.Device_Type_ID != 1)
//                        {
//                        }
//                        else if (query_device.Device_Type_ID == 1)
//                        {
//                            var apn_name = ctx.SIMCards3.Where(x => x.Ricon_SN == selected_riconserino).FirstOrDefault();
//                            string log_message = "";
//                            if (apn_name.APN1_Name == "pos1.tim.it")
//                            {
//                                do
//                                {//phase1 p11 ikev1
//                                    string msj1 = "set ipsec-ph1 p11" + "\n" +
//                                       "exchange-mode aggr" + " \n" +
//                                        "ikev ikev1 ";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            string failed_sms = CultureHelper.GetResourceKey("L236");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = CultureHelper.GetResourceKey("L236");
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = CultureHelper.GetResourceKey("L236");
//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                }

//                                while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                log_message = CultureHelper.GetResourceKey("L235") + " \n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {//phase1 p21 ikev1
//                                    string msj1 = "set ipsec-ph1 p21" + "\n" +
//                                        "exchange-mode aggr" + " \n" +
//                                         "ikev ikev1 ";
//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            string failed_sms = CultureHelper.GetResourceKey("L238");
//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + CultureHelper.GetResourceKey("L238");
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;
//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                }

//                                while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                log_message = log_message + CultureHelper.GetResourceKey("L237") + " \n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {//phase2 p12 policy
//                                    string msj1 = "set ipsec-ph2 p12" + "\n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.33.0.0/16 \n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.34.0.0/16 \n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.7.0.0/16";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            string failed_sms = CultureHelper.GetResourceKey("L220");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + CultureHelper.GetResourceKey("L220");
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;
//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                }

//                                while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                log_message = log_message + CultureHelper.GetResourceKey("L221") + " \n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {//phase2 p22 policy
//                                    string msj1 = "set ipsec-ph2 p22" + "\n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.36.0.0/16 \n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.37.0.0/16 \n" +
//                                         "policy " + query.Lan_Subnet + "/" + query.Lan1_SubnetMask + " " + "10.20.0.0/16";
//                                    bool isfast = false;

//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            string failed_sms = CultureHelper.GetResourceKey("L222");
//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                //  log_message += "phase2 p22 policy Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L223") + "\n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {//lan 1
//                                    string msj1 = "set lan" + "\n" +
//                                       "lan1 " + query.Lan1_IP + "/" + query.Lan1_SubnetMask + "\n" +
//                                       "lan2 192.168.8.1/24" + "\n" +
//                                       "no&lan3";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);

//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            // string failed_sms = "lan1 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L224");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "lan1 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L225") + "\n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);

//                                do
//                                { // sim 2
//                                    string msj1 = "set modem 1" + "\n" +
//                                        "enable " + "\n" +
//                                        "apn " + query.APN2_Name + "\n" +
//                                       "username " + query.APN2_Username + "\n" +
//                                       "password " + query.APN2_Password + "\n" +
//                                       "sim 2";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            // string failed_sms= "sim2 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L228");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "sim2 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L229") + "\n";

//                                denemeSayisi = 0;

//                                Thread.Sleep(20000);
//                                do
//                                { // sim 1
//                                    string msj1 = "set modem 0" + "\n" +
//                                       "apn " + query.APN1_Name + "\n" +
//                                        "username " + query.APN1_Username + "\n" +
//                                         "password " + query.APN1_Password + "\n" +
//                                          "sim 1";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {   //string failed_sms = "sim1 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L226");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;
//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "sim1 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L227") + "\n";

//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {//linkbackup and reboot
//                                    string msj1 = "set linkbackup\n" +
//                                         "enable";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            //  string failed_sms= "linkbackup UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L230");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                //  log_message += "linkbackup Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L231") + "\n";

//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {// reboot
//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, "reboot", username, password, title, isfast);

//                                    if (denemeSayisi == 1)
//                                    {
//                                        return true;
//                                    }

//                                    denemeSayisi++;
//                                }
//                                while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                denemeSayisi = 0;
//                            }
//                            else if (apn_name.APN1_Name == "routed-pos.tim.it")
//                            {
//                                do
//                                {//lan 1
//                                    string msj1 = "set lan" + "\n" +
//                                       "lan1 " + query.Lan1_IP + "/" + query.Lan1_SubnetMask + "\n" +
//                                       "lan2 192.168.8.1/24" + "\n" +
//                                       "no&lan3";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);

//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            // string failed_sms = "lan1 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L224");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "lan1 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L225") + "\n";
//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                { // sim 2
//                                    string msj1 = "set modem 1" + "\n" +
//                                        "enable " + "\n" +
//                                        "apn " + query.APN2_Name + "\n" +
//                                       "username " + query.APN2_Username + "\n" +
//                                       "password " + query.APN2_Password + "\n" +
//                                       "sim 2";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            // string failed_sms= "sim2 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L228");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "sim2 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L229") + "\n";

//                                denemeSayisi = 0;

//                                Thread.Sleep(20000);
//                                do
//                                { // sim 1
//                                    string msj1 = "set modem 0" + "\n" +
//                                       "apn " + query.APN1_Name + "\n" +
//                                        "username " + query.APN1_Username + "\n" +
//                                         "password " + query.APN1_Password + "\n" +
//                                          "sim 1";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {   //string failed_sms = "sim1 UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L226");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;
//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                // log_message += "sim1 Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L227") + "\n";

//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);

//                                do
//                                {//linkbackup and reboot
//                                    string msj1 = "set linkbackup\n" +
//                                         "enable";

//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, msj1, username, password, title, isfast);
//                                    if (denemeSayisi == 1)
//                                    {
//                                        if (donus == "UNDELIVERED" || donus == "SENT")
//                                        {
//                                            //  string failed_sms= "linkbackup UNDELIVERED";
//                                            string failed_sms = CultureHelper.GetResourceKey("L230");

//                                            ViewBag.InfoTitle = CultureHelper.GetResourceKey("L200");
//                                            ViewBag.SmsMessage = log_message + failed_sms;
//                                            HttpContext.Current.Session["InfoTitle"] = CultureHelper.GetResourceKey("L200");
//                                            HttpContext.Current.Session["SmsMessage"] = log_message + failed_sms;

//                                            return false;
//                                        }
//                                    }

//                                    denemeSayisi++;
//                                } while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                //  log_message += "linkbackup Message sent succesfully \n";
//                                log_message = log_message + CultureHelper.GetResourceKey("L231") + "\n";

//                                denemeSayisi = 0;
//                                Thread.Sleep(20000);
//                                do
//                                {// reboot
//                                    bool isfast = false;
//                                    if (denemeSayisi != 0)
//                                    {
//                                        isfast = true;
//                                    }
//                                    donus = sendMessage(GSM, "reboot", username, password, title, isfast);

//                                    if (denemeSayisi == 1)
//                                    {
//                                        return true;
//                                    }

//                                    denemeSayisi++;
//                                }
//                                while (donus != "DELIVERED" && denemeSayisi <= 1);
//                                denemeSayisi = 0;
//                            }
//                        }
//                    }
//                }

//                return true;
//            }
//            catch (System.Exception e)
//            {
//                //install_query.Date_Time = DateTime.Now;
//                e.Message.ToString();
//                return false;
//            }
//        }

//        public string sendMessage(string GSM_No, string mesaj, string username, string password, string title, bool fast)
//        {
//            string m_result = "";

//            try
//            {
//                SMSApi.Api.IClient client = new SMSApi.Api.ClientOAuth("MiQksF5buJOGtXvqCvOWpl3PuCDQYABA5C3oUv4f");

//                var smsApi = new SMSApi.Api.SMSFactory(client, ProxyAddress.SmsApiCom);

//                var result =
//                    smsApi.ActionSend()
//                        .SetText(mesaj)
//                        .SetTo(GSM_No)
//                        .SetSender("Ricon")
//                        .SetFast(fast)
//                        .Execute();

//                string[] ids = new string[result.Count];

//                for (int i = 0, l = 0; i < result.List.Count; i++)
//                {
//                    if (!result.List[i].isError())
//                    {
//                        if (!result.List[i].isFinal())
//                        {
//                            ids[l] = result.List[i].ID;
//                            l++;
//                        }
//                    }
//                }

//                result =
//                    smsApi.ActionGet()
//                        .Ids(ids)
//                        .Execute();
//                int time_count = 0;
//                while ((result.List[0].Status != "DELIVERED") && (result.List[0].Status != "UNDELIVERED") && time_count < 40)
//                {
//                    Thread.Sleep(1000);
//                    time_count++;

//                    result =
//                   smsApi.ActionGet()
//                       .Ids(ids)
//                       .Execute();
//                }

//                m_result = result.List[0].Status;
//            }
//            catch (SMSApi.Api.ActionException e)
//            {
//                /**
//                 * Action error
//                 */
//                System.Console.WriteLine(e.Message);
//                return e.Message;
//            }
//            catch (SMSApi.Api.ClientException e)
//            {
//                /**
//                 * Error codes (list available in smsapi docs). Example:
//                 * 101 	Invalid authorization info
//                 * 102 	Invalid username or password
//                 * 103 	Insufficient credits on Your account
//                 * 104 	No such template
//                 * 105 	Wrong IP address (for IP filter turned on)
//                 * 110	Action not allowed for your account
//                 */
//                System.Console.WriteLine(e.Message);
//                return e.Message;
//            }
//            catch (SMSApi.Api.HostException e)
//            {
//                /*
//                 * Server errors
//                 * SMSApi.Api.HostException.E_JSON_DECODE - problem with parsing data
//                 */
//                System.Console.WriteLine(e.Message);
//            }
//            catch (SMSApi.Api.ProxyException e)
//            {
//                // communication problem between client and sever
//                System.Console.WriteLine(e.Message);
//                return e.Message;
//            }
//            return m_result;
//        }

//    }
//}