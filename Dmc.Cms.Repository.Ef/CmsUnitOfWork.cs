using Dmc.Repository.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Dmc.Cms.Model;

namespace Dmc.Cms.Repository.Ef
{
    public class CmsUnitOfWork : ICmsUnitOfWork // should actually have some kind of factory, as base unit of work does, but, wht ... 
    {
        #region Fields

        private readonly DbContext _Context;
        private readonly UnitOfWork _UnitOfWork;

        #endregion

        #region Repository Private Fields

        private IRatingRepository _RatingRepository;
        private IPostRepository _PostRepository;
        private ICategoryRepository _CategoryRepository;
        private ITagRepository _TagRepository;
        private IPageRepository _PageRepository;
        private IImageRepository _ImageRepository;
        private IContactRepository _ContactRepository;

        #endregion

        #region Constructor

        public CmsUnitOfWork(DbContext context)
        {
            _Context = context;
            _UnitOfWork = new UnitOfWork(_Context);
        }

        #endregion

        #region Properties

        public IContactRepository ContactRepository
        {
            get
            {
                if (_ContactRepository == null)
                {
                    _ContactRepository = new ContactRepository(_UnitOfWork.Repository<ContactQuery>());
                }
                return _ContactRepository;
            }
        }

        public IRatingRepository RatingRepository
        {
            get
            {
                if (_RatingRepository == null)
                {
                    _RatingRepository = new RatingRepository(_UnitOfWork.Repository<Rating>());
                }
                return _RatingRepository;
            }
        }

        public IImageRepository ImageRepository
        {
            get
            {
                if (_ImageRepository == null)
                {
                    _ImageRepository = new ImageRepository(_UnitOfWork.Repository<Image>());
                }
                return _ImageRepository;
            }
        }

        public IPostRepository PostRepository
        {
            get
            {
                if (_PostRepository == null)
                {
                    _PostRepository = new PostRepository(_UnitOfWork.Repository<Post>(), _Context.Set<Post>());
                }
                return _PostRepository;
            }
        }

        public ICategoryRepository CategoryRepository
        {
            get
            {
                if (_CategoryRepository == null)
                {
                    _CategoryRepository = new CategoryRepository(_UnitOfWork.Repository<Category>());
                }
                return _CategoryRepository;
            }
        }

        public IPageRepository PageRepository
        {
            get
            {
                if (_PageRepository == null)
                {
                    _PageRepository = new PageRepository(_UnitOfWork.Repository<Page>());
                }
                return _PageRepository;
            }
        }

        public ITagRepository TagRepository
        {
            get
            {
                if (_TagRepository == null)
                {
                    _TagRepository = new TagRepository(_UnitOfWork.Repository<Tag>());
                }
                return _TagRepository;
            }
        }

        #endregion

        #region Saving Changes

        public async Task SaveAsync()
        {
            try
            {
                await _Context.SaveChangesAsync();
            }
            catch (Exception) // TODO: Wrap to custom ... 
            {
                throw;
            }
        }

        #endregion
    }
}
