using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;

namespace KurulumWeb.Helper
{
    public static class CultureHelper
    {
        // Valid cultures
        private static readonly List<string> _validCultures = new List<string> { "af", "af-ZA", "sq", "sq-AL", "gsw-FR", "am-ET", "ar", "ar-DZ", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "hy", "hy-AM", "as-IN", "az", "az-Cyrl-AZ", "az-Latn-AZ", "ba-RU", "eu", "eu-ES", "be", "be-BY", "bn-BD", "bn-IN", "bs-Cyrl-BA", "bs-Latn-BA", "br-FR", "bg", "bg-BG", "ca", "ca-ES", "zh-HK", "zh-MO", "zh-CN", "zh-Hans", "zh-SG", "zh-TW", "zh-Hant", "co-FR", "hr", "hr-HR", "hr-BA", "cs", "cs-CZ", "da", "da-DK", "prs-AF", "div", "div-MV", "nl", "nl-BE", "nl-NL", "en", "en-AU", "en-BZ", "en-CA", "en-029", "en-IN", "en-IE", "en-JM", "en-MY", "en-NZ", "en-PH", "en-SG", "en-ZA", "en-TT", "en-GB", "en-US", "en-ZW", "et", "et-EE", "fo", "fo-FO", "fil-PH", "fi", "fi-FI", "fr", "fr-BE", "fr-CA", "fr-FR", "fr-LU", "fr-MC", "fr-CH", "fy-NL", "gl", "gl-ES", "ka", "ka-GE", "de", "de-AT", "de-DE", "de-LI", "de-LU", "de-CH", "el", "el-GR", "kl-GL", "gu", "gu-IN", "ha-Latn-NG", "he", "he-IL", "hi", "hi-IN", "hu", "hu-HU", "is", "is-IS", "ig-NG", "id", "id-ID", "iu-Latn-CA", "iu-Cans-CA", "ga-IE", "xh-ZA", "zu-ZA", "it", "it-IT", "it-CH", "ja", "ja-JP", "kn", "kn-IN", "kk", "kk-KZ", "km-KH", "qut-GT", "rw-RW", "sw", "sw-KE", "kok", "kok-IN", "ko", "ko-KR", "ky", "ky-KG", "lo-LA", "lv", "lv-LV", "lt", "lt-LT", "wee-DE", "lb-LU", "mk", "mk-MK", "ms", "ms-BN", "ms-MY", "ml-IN", "mt-MT", "mi-NZ", "arn-CL", "mr", "mr-IN", "moh-CA", "mn", "mn-MN", "mn-Mong-CN", "ne-NP", "no", "nb-NO", "nn-NO", "oc-FR", "or-IN", "ps-AF", "fa", "fa-IR", "pl", "pl-PL", "pt", "pt-BR", "pt-PT", "pa", "pa-IN", "quz-BO", "quz-EC", "quz-PE", "ro", "ro-RO", "rm-CH", "ru", "ru-RU", "smn-FI", "smj-NO", "smj-SE", "se-FI", "se-NO", "se-SE", "sms-FI", "sma-NO", "sma-SE", "sa", "sa-IN", "sr", "sr-Cyrl-BA", "sr-Cyrl-SP", "sr-Latn-BA", "sr-Latn-SP", "nso-ZA", "tn-ZA", "si-LK", "sk", "sk-SK", "sl", "sl-SI", "es", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-DO", "es-EC", "es-SV", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-ES", "es-US", "es-UY", "es-VE", "sv", "sv-FI", "sv-SE", "syr", "syr-SY", "tg-Cyrl-TJ", "tzm-Latn-DZ", "ta", "ta-IN", "tt", "tt-RU", "te", "te-IN", "th", "th-TH", "bo-CN", "tr", "tr-TR", "tk-TM", "ug-CN", "uk", "uk-UA", "wen-DE", "ur", "ur-PK", "uz", "uz-Cyrl-UZ", "uz-Latn-UZ", "vi", "vi-VN", "cy-GB", "wo-SN", "sah-RU", "ii-CN", "yo-NG" };

        // Include ONLY cultures you are implementing
        private static readonly List<string> _cultures = new List<string> {
            "en-US",  //
            "it-IT"  // first culture is the DEFAULT
        };

        /// <summary>
        /// Returns true if the language is a right-to-left language. Otherwise, false.
        /// </summary>
        public static bool IsRighToLeft()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.IsRightToLeft;
        }

        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        /// <param name="name">Culture's name (e.g. en-US)</param>
        public static string GetImplementedCulture(string name)
        {
            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture(); // return Default culture

            // make sure it is a valid culture first
            if (_validCultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                return GetDefaultCulture(); // return Default culture if it is invalid

            // if it is implemented, accept it
            if (_cultures.Where(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0)
                return name; // accept it

            // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB",
            // the function will return closes match that is "en-US" because at least the language is the same (ie English)
            var n = GetNeutralCulture(name);
            foreach (var c in _cultures)
                if (c.StartsWith(n))
                    return c;

            // else
            // It is not implemented
            return GetDefaultCulture(); // return Default culture as no match found
        }

        public static string GetResourceKey(string key)
        {
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager("KurulumWeb.Resources.Resources", Assembly.GetExecutingAssembly());
            string cultureName = Thread.CurrentThread.CurrentCulture.Name;
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(cultureName);

            return rm.GetString(key, ci);
        }

        /// <summary>
        /// Returns default culture name which is the first name decalared (e.g. en-US)
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultCulture()
        {
            return "it-IT";//Parameters.langParameter();
                           // return _cultures[0]; // return Default culture
        }

        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }

        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
        }

        public static string GetNeutralCulture(string name)
        {
            if (!name.Contains("-")) return name;

            return name.Split('-')[0]; // Read first part only. E.g. "en", "es"
        }

        public static void setCulture(string culture)
        {
            // Validate input
            var _culture = HttpContext.Current.Request.Form["cultureText"];
            culture = CultureHelper.GetImplementedCulture(culture);

            var usr = HttpContext.Current.Request.Cookies["UserGUID"];
            if (usr == null && _culture != null)
                culture = _culture;

            // Save culture in a cookie
            HttpCookie cookie = HttpContext.Current.Request.Cookies["_culture"];
            if (cookie != null)
                cookie.Value = culture;
            //  cookie.Value = culture;   // update cookie value
            else
            {
                cookie = new HttpCookie("_culture");
                cookie.Value = culture;
                //cookie.Value = culture;
                cookie.Expires = DateTime.Now.AddYears(1);
            }
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        //public static void SetCulture(string culture)
        //{
        //    // Validate input
        //    var _culture = HttpContext.Current.Request.Form["cultureText"];
        //    culture = CultureHelper.GetImplementedCulture(culture);

        //    var usr = HttpContext.Current.Request.Cookies["UserGUID"];
        //    if (usr == null && _culture != null)
        //        culture = _culture;

        //    // Create a new cookie
        //    HttpCookie cookie = new HttpCookie("_culture", culture);

        //    // Set SameSite and Secure properties
        //    cookie.SameSite = SameSiteMode.None;
        //    cookie.Secure = true;

        //    // Set the expiration date
        //    cookie.Expires = DateTime.Now.AddYears(1);

        //    // Add the cookie to the response
        //    HttpContext.Current.Response.Cookies.Add(cookie);
        //}
    }

    public class Parameters
    {
        private static Object Kilit = new Object();
        private static List<Parameter> parameters;

        private static void setParameter()
        {
            lock (Kilit)
            {
                if (parameters == null || parameters.Count == 0)
                {
                    try
                    {
                        parameters = new List<Parameter>();
                        parameters.Add(new Parameter("", "DefaultLanguage", "en", 1, true)); //todo DefaultLanguage için val
                    }
                    catch (Exception ex)
                    {
                        ex.Message.ToString();
                    }
                }
            }
        }

        public static string langParameter()
        {
            try
            {
                string lang = "it-IT";
                switch (lang)
                {
                    case "":
                    case "en":
                    case "en-us":
                    case "en-Us":
                    case "en-US":
                    case "En-Us":
                    case "EN-Us":
                    case "En-US":
                    case "EN-US":
                        {
                            var array = lang.Split('-');
                            lang = array[0].ToLower();
                            if (array.Length > 1) lang += "-" + array[1].ToUpper();
                            else lang += "-US";
                            break;
                        }
                    case "tr":
                    case "tr-tr":
                    case "tr-Tr":
                    case "tr-TR":
                    case "Tr-Tr":
                    case "TR-Tr":
                    case "Tr-TR":
                    case "TR-TR":
                        {
                            var array = lang.Split('-');
                            lang = array[0].ToLower();
                            if (array.Length > 1) lang += "-" + array[1].ToUpper();
                            else lang += "-TR";
                            break;
                        }
                    default:
                        {
                            lang = "en-US";
                            break;
                        }
                }
                return lang;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class Parameter
    {
        public string ParameterId { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string PageName { get; set; }

        public int PageType { get; set; }
        public bool isDropBox { get; set; }

        public Parameter()
        { }

        public Parameter(string id, string name, string value, int _type, bool _isDropBox = false)
        {
            ParameterId = id;
            ParameterName = name;
            ParameterValue = value;
            PageType = _type;//setting sayfasında parametreleri ayrı alanlarda yazmaya yarar!
            isDropBox = _isDropBox;
        }
    }
}