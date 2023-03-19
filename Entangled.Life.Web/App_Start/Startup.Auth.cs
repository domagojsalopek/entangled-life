using Dmc.Cms.App;
using Dmc.Cms.App.Identity;
using Dmc.Cms.Model;
using Dmc.Cms.Repository.Ef;
using Dmc.Identity;
using Dmc.Identity.Ef;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Entangled.Life.Web
{
    public partial class Startup
    {
        private const int ValidateIntervalInMinutes = 15;

        public void ConfigureAuth(IAppBuilder app)
        {
            // setup cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = IdentityAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = AppConfiguration.Instance.GetLoginDuration(),
                SlidingExpiration = false,
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = o => ApplicationUserValidator.ValidateIdentity(o, TimeSpan.FromMinutes(ValidateIntervalInMinutes), new ApplicationUserManagerFactory())
                }
            });

            app.Use(async (context, next) =>
            {
                await DefaultMembershipProvider.InitializeMembershipIfNeeded();
                await next.Invoke();
            });
        }
    }
}