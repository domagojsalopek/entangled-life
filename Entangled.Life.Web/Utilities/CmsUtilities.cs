using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web
{
    public static class CmsUtilities
    {
        const string CookieConsentScriptCookieName = "cookieconsent_status";
        const string CMSCookieConsentCookieName = "el_cookie_consent_status";

        public static Uri GetBaseUrl(this UrlHelper url)
        {
            Uri contextUri = new Uri(url.RequestContext.HttpContext.Request.Url, url.RequestContext.HttpContext.Request.RawUrl);
            UriBuilder realmUri = new UriBuilder(contextUri) { Path = url.RequestContext.HttpContext.Request.ApplicationPath, Query = null, Fragment = null };
            return realmUri.Uri;
        }

        public static string AbsoluteAction(this UrlHelper url, string actionName, string controllerName, object routeValues)
        {
            return new Uri(GetBaseUrl(url), url.Action(actionName, controllerName, routeValues)).AbsoluteUri;
        }

        public static string AbsoluteContent(this UrlHelper url, string content)
        {
            var baseURI = new Uri(GetBaseUrl(url), content.TrimStart('~'));
            return new Uri(baseURI.AbsoluteUri, UriKind.Absolute).AbsoluteUri;
        }

        public static bool IsCookieConsentGiven(this HtmlHelper htmlHelper)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException(nameof(htmlHelper));
            }

            if (htmlHelper.ViewContext.RequestContext.HttpContext.Request.Cookies[CMSCookieConsentCookieName] != null)
            {
                return true; // our cookie only needs to be present
            }

            return IsCookieScriptCookiePresentAndAccepted(htmlHelper);
        }

        private static bool IsCookieScriptCookiePresentAndAccepted(HtmlHelper htmlHelper)
        {
            var cookie = htmlHelper.ViewContext.RequestContext.HttpContext.Request.Cookies[CookieConsentScriptCookieName];

            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
            {
                return false;
            }

            return cookie.Value.Equals("dismiss", StringComparison.OrdinalIgnoreCase) || cookie.Value.Equals("allow", StringComparison.OrdinalIgnoreCase);
        }
    }
}