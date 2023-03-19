using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.Model
{
    public class Link : EntityBase
    {
        public LinkType LinkType
        {
            get;
            set;
        }

        public LinkPosition LinkPosition
        {
            get;
            set;
        }

        public string Location
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public string Tooltip
        {
            get;
            set;
        }

        public string CssClass
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }
    }
}
