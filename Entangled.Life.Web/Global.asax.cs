using Dmc.Cms.App;
using Dmc.Cms.Repository.Ef;
using Dmc.Core.DI;
using Entangled.Life.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Entangled.Life.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            //CultureResolver.TrySetCultureFromRequest();
        }

        protected void Application_Start()
        {
#if DEBUG
            using (CmsContext context = new CmsContext())
            {
                context.Database.Initialize(true);
            }
#endif

            AreaRegistration.RegisterAllAreas();

            // Reset ViewEngines
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            // Normal MVC Configuration
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // APPConfiguration. This is not needed. All this can be in configuration in app_data and managed through admin
            // This is big todo
            AppConfiguration.Instance.Configure();

            // Dependency Injection
            var container = new DependencyInjectionContainer();
            DependencyConfiguration.Configure(container);
            ControllerBuilder.Current.SetControllerFactory(new CmsControllerFactory(container));
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();
        }
    }
}
