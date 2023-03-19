using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web.Controllers
{
    public class CookieController : Controller
    {
        const string CookieConsentScriptCookieName = "cookieconsent_status";
        const string CMSCookieConsentCookieName = "el_cookie_consent_status";
        const int CookieExpiresInDays = 365;

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Enable()
        {
            if (!Request.IsAjaxRequest())
            {
                return new HttpUnauthorizedResult();
            }

            Response.Cookies.Add(new HttpCookie(CMSCookieConsentCookieName)
            {
                Expires = DateTime.Now.AddDays(CookieExpiresInDays),
                Value = true.ToString()
            });

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { Success = true }),
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json"
            };
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Disable()
        {
            if (!Request.IsAjaxRequest())
            {
                return new HttpUnauthorizedResult();
            }

            //if (Request.Cookies[CookieConsentScriptCookieName] != null)
            //{
            //    var cookie = new HttpCookie(CookieConsentScriptCookieName, "deny")
            //    {
            //        HttpOnly = false,
            //        Secure = false,
            //        Expires = DateTime.Now.AddDays(CookieExpiresInDays)
            //    };
            //    Response.Cookies.Set(cookie);
            //}

            if (Request.Cookies[CMSCookieConsentCookieName] != null)
            {
                var cookieToRemove = new HttpCookie(CMSCookieConsentCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = false,
                    Secure = false,
                };
                Response.Cookies.Set(cookieToRemove);
            }

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { Success = true }),
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json"
            };
        }
    }
}