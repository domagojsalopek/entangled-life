using Dmc.Cms.App.Identity;
using Dmc.Cms.Model;
using Dmc.Cms.Repository.Ef;
using Dmc.Identity.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web
{
    public class ApplicationUserManagerFactory
    {
        public ApplicationUserManager Create()
        {
            var dbContext = new CmsContext(); // This is terrible. store is completely unnecessarry.
            return new ApplicationUserManager(new ApplicationUserStore(new IdentityUnitOfWork<User>(dbContext)), new CmsUnitOfWork(dbContext));
        }
    }
}