using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using KurulumWeb.Helper;

namespace KurulumWeb.Controllers
{
    public class BaseController : Controller
    {

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            HttpCookie cultureCookie = Request.Cookies["_culture"];
           
            if (cultureCookie != null)
                cultureName = cultureCookie.Value;
            else
                cultureName = "en-US";


            string[] supportedCultures = new[] { "tr-TR", "en-US", "de-DE", "fr-FR","it-IT" };
            cultureName = supportedCultures.Contains(cultureName) ? cultureName : "en-US";

            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            return base.BeginExecuteCore(callback, state);
        }


    }
}