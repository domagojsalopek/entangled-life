using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.ViewModels
{
    public class CategoryWithSubcategoriesListViewModel
    {
        public CategoryWithSubcategoriesListViewModel()
        {
            Subcategories = new List<CategoryWithSubcategoriesListViewModel>();
        }

        public string Name
        {
            get;
            set;
        }

        public string Slug
        {
            get;
            set;
        }

        public bool HasSubcategories => Subcategories.Count > 0;

        public List<CategoryWithSubcategoriesListViewModel> Subcategories
        {
            get;
            set;
        }
    }
}