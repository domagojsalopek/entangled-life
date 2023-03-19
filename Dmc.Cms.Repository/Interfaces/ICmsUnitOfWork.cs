using Dmc.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.Repository
{
    public interface ICmsUnitOfWork
    {
        IContactRepository ContactRepository
        {
            get;
        }

        IPostRepository PostRepository
        {
            get;
        }

        ICategoryRepository CategoryRepository
        {
            get;
        }

        IPageRepository PageRepository
        {
            get;
        }

        ITagRepository TagRepository
        {
            get;
        }

        IImageRepository ImageRepository
        {
            get;
        }

        IRatingRepository RatingRepository
        {
            get;
        }

        Task SaveAsync();
    }
}
