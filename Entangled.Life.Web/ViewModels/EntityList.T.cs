using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.ViewModels
{
    public class EntityList<T> where T : ICrudViewModel
    {
        private IEnumerable<T> _InnerList;
        private readonly int _Page;
        private readonly int _PerPage;
        private readonly int _TotalCount;
        private readonly int _TotalPages;


        public EntityList(EntityList entityList)
        {
            _Page = entityList.PageIndex;
            _PerPage = entityList.PageSize;
            _TotalCount = entityList.TotalCount;
            _TotalPages = entityList.TotalPages;
            _InnerList = entityList.Entities.Cast<T>();
        }

        public EntityList(IEnumerable<T> results, int page, int perPage, int totalCount)
        {
            _Page = page;
            _PerPage = perPage;
            _TotalCount = totalCount;
            _TotalPages = (int)Math.Ceiling(_TotalCount / (double)_PerPage);
            _InnerList = results;
        }

        public IEnumerable<T> Results => _InnerList;

        public int PageIndex => _Page;

        public int PageSize => _PerPage;

        public int TotalCount => _TotalCount;

        public int TotalPages => _TotalPages;

        public bool HasPreviousPage => (PageIndex > 1);

        public bool HasNextPage => (PageIndex < TotalPages);
    }
}