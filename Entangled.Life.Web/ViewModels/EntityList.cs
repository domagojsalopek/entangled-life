using Dmc.Cms.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.ViewModels
{
    public class EntityList : PagedList<ICrudViewModel>
    {
        #region Constructors

        public EntityList(IEnumerable<ICrudViewModel> results, int page, int perPage, int total) 
            : base(results, page, perPage, total)
        {
        }

        #endregion
    }
}