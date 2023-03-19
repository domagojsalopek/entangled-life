using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entangled.Life.Web.ViewModels
{
    public interface ICrudViewModel
    {
        int? Id
        {
            get;
            set;
        }
    }
}
