using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.ViewModels
{
    public class TagViewModel : ContentViewModelBase
    {
        public string Title
        {
            get;
            set;
        }

        public EntityList<PostViewModel> Posts
        {
            get;
            set;
        }
    }
}