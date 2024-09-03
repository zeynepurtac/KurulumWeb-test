using KurulumWeb.Helper;
using KurulumWeb.Models;
using SMSApi.Api.Response;
using SNMPDB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace KurulumWeb.Controllers
{
    public class LoginController : BaseController
    {
        //Ricon001Entities ctx = new Ricon001Entities();
        //Ricon002Entities ctx = new Ricon002Entities();
        private RiconApps_FASEntities1 ctx = new RiconApps_FASEntities1();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {


                var query = ctx.UserLogin.FirstOrDefault(x => x.Username == UserName && x.Password == Password && x.Status==1); //textboxlardan girilen değere göre Login tablosundan kullanıcı getir

                if (query != null) //kullanıcı var ise
                {
                    Session["UserID"] = query.User_ID.ToString(); //Id sini Sessionda tut
                    Session["UserName"] = query.Username.ToString(); //Username Sessionda tut
                    Session["Operator"] = query.Company_ID.ToString();//Operatoru sessionda tut ona göre sayfa yönlendirmesi yap
                    Session["IsAdmin"] = query.IsAdmin;
                    if (query.IsAdmin == true)
                    {
                        return RedirectToAction("RiconAdmin", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Ricon", "Home");
                    }
                }
           
           
            
            else
            {
                // Kimlik doğrulama başarısız, hata mesajını göster
                ViewBag.LoginError = CultureHelper.GetResourceKey("L116");
                return View();
            }
        }  

        public ActionResult SetCulture(string culture)
        {
            CultureHelper.setCulture(culture);
            return RedirectToAction("Login");
        }
    }
}