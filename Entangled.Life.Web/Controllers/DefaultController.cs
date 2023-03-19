using Dmc.Cms.App;
using Dmc.Cms.App.Identity;
using Dmc.Cms.App.Services;
using Dmc.Cms.Model;
using Dmc.Cms.Repository;
using Entangled.Life.Web.Mappers;
using Entangled.Life.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web.Controllers
{
    public class DefaultController : FrontEndControllerBase<Post, PostViewModel>
    {
        //TODO: Heavy use of caching!!
        //Rewrite EVERYTHING!! Identity is SHIT
        //Services are too complex ...

        #region Private Fields

        private Category _Category;
        private Tag _Tag;
        private readonly IPostService _PostService;
        private readonly ICategoryService _CategoryService;
        private readonly ITagService _TagService;

        #endregion

        #region Constructors

        public DefaultController(IPostService service, ICategoryService categoryService, ITagService tagService, ApplicationUserManager userManager) 
            : base(service, userManager)
        {
            _PostService = service ?? throw new ArgumentNullException(nameof(service));
            _CategoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _TagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        #endregion

        #region Web Methods

        public async Task<ActionResult> Index(int? page, int? perPage)
        {
            // first get results
            var results = await GetEntityListAsync(page, perPage);

            // then append info based on logged in user ... service should be instead returning these things prepared, but I want to finish this ...
            if (User.Identity.IsAuthenticated)
            {
                await FillViewModelWithUserInfo(results);
            }

            return ListView(results);
        }

        public async Task<ActionResult> Details(string slug)
        {
            return await EntityDetailsAsync(slug);
        }

        public async Task<ActionResult> PostsByCategory(string categoryName, int? page, int? perPage)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return NotFound();
            }

            _Category = await _CategoryService.FindBySlugAsync(categoryName);

            if (_Category == null || !IsEntityDisplayable(_Category))
            {
                return NotFound();
            }

            EntityList<PostViewModel> entityList = await GetEntityListAsync(page, perPage);

            if (User.Identity.IsAuthenticated)
            {
                await FillViewModelWithUserInfo(entityList);
            }

            CategoryViewModel viewModel = CreateCategoryViewModel(_Category, entityList);
            return View(viewModel);
        }

        public async Task<ActionResult> PostsByTag(string tagName, int? page, int? perPage)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return NotFound();
            }

            _Tag = await _TagService.FindBySlugAsync(tagName);

            if (_Tag == null || !IsEntityDisplayable(_Tag))
            {
                return NotFound();
            }

            EntityList<PostViewModel> entityList = await GetEntityListAsync(page, perPage);

            if (User.Identity.IsAuthenticated)
            {
                await FillViewModelWithUserInfo(entityList);
            }

            TagViewModel viewModel = new TagViewModel
            {
                Title = _Tag.Title,
                Posts = entityList,
                Published = _Tag.Published,
                Slug = _Tag.Slug,
                Status = _Tag.Status,
                Id = _Tag.Id
            };

            return View(viewModel);
        }

        #endregion

        #region Abstract Implementation Methods

        internal override void TransferFromEntityToBrowseViewModel(Post entity, PostViewModel viewModel)
        {
            PostMapper.TransferToViewModel(entity, viewModel);
        }

        internal override void TransferFromEntityToDetailsViewModel(Post entity, PostViewModel detailsViewModel)
        {
            PostMapper.TransferToViewModel(entity, detailsViewModel);
        }

        #endregion

        #region Override Methods

        //TODO: maybe different controllers for tag display, etc ... 

        protected override Task<int> CountPublishedAndDraftsAsync(IDictionary<string, object> routeData)
        {
            if (_Tag != null)
            {
                return _PostService.CountPostsWithTagAsync(_Tag.Id, true);
            }

            if (_Category != null)
            {
                return _PostService.CountPostsInCategoryAsync(_Category.Id, true);
            }

            return _PostService.CountAllPublishedAndDraftsAsync();
        }

        protected override Task<int> CountPublishedAsync(IDictionary<string, object> routeData)
        {
            if (_Tag != null)
            {
                return _PostService.CountPostsWithTagAsync(_Tag.Id, false);
            }

            if (_Category != null)
            {
                return _PostService.CountPostsInCategoryAsync(_Category.Id, false);
            }

            return _PostService.CountAllPublishedAsync();
        }

        protected override async Task<IEnumerable<Post>> GetPagedPublishedAndDraftsAsync(int page, int perPage, IDictionary<string, object> routeData)
        {
            if (_Tag != null)
            {
                return await _PostService.GetPostsWithTagAsync(_Tag.Id, page, perPage, true);
            }

            if (_Category != null)
            {
                return await _PostService.GetPostsInCategoryAsync(_Category.Id, page, perPage, true); 
            }

            return await base.GetPagedPublishedAndDraftsAsync(page, perPage, routeData);
        }

        protected override async Task<IEnumerable<Post>> GetPagedPublishedAsync(int page, int perPage, IDictionary<string, object> routeData)
        {
            if (_Tag != null)
            {
                return await _PostService.GetPostsWithTagAsync(_Tag.Id, page, perPage, false);
            }

            if (_Category != null)
            {
                return await _PostService.GetPostsInCategoryAsync(_Category.Id, page, perPage, false);
            }

            return await base.GetPagedPublishedAsync(page, perPage, routeData);
        }

        #endregion

        #region Private Methods

        private CategoryViewModel CreateCategoryViewModel(Category category, EntityList<PostViewModel> entityList)
        {
            CategoryViewModel result = new CategoryViewModel
            {
                Posts = entityList
            };

            result.Description = category.Description;
            result.Title = category.Title;
            result.Slug = category.Slug;
            result.Status = category.Status;
            result.Id = category.Id;

            if (_Category.IntroImage != null)
            {
                result.IntroImage = new ImageViewModel();
                ImageMapper.TransferToViewModel(_Category.IntroImage, result.IntroImage);
            }

            return result;
        }

        #endregion

        #region Private Additional Methods

        private async Task FillViewModelWithUserInfo(EntityList<PostViewModel> entityList)
        {
            User loggedInUser = await GetLoggedInUserAsync(); // this eagerly loads favourite stores. we should not do it this way... favourite stores and shit can be cached for a while ...  

            if (loggedInUser == null)
            {
                return;
            }

            int[] postIds = entityList.Results.Where(o => o.Id.HasValue).Select(o => o.Id.Value).ToArray();
            IEnumerable<Rating> ratingsForCurrentUserForThesePosts = await _PostService.GetUserRatingsForPostIdsAsync(loggedInUser, postIds);

            foreach (var item in entityList.Results)
            {
                AppendRatingAndFavouriteInfo(item, ratingsForCurrentUserForThesePosts, loggedInUser.FavouritePosts);
            }
        }

        private void AppendRatingAndFavouriteInfo(PostViewModel item, IEnumerable<Rating> ratingsForCurrentUserForThesePosts, ICollection<Post> favouritePosts)
        {
            Rating userRatingForTost = ratingsForCurrentUserForThesePosts.FirstOrDefault(o => o.PostId == item.Id);

            if (userRatingForTost != null)
            {
                item.HasRating = true;
                item.Liked = userRatingForTost.IsLike;
            }

            item.IsFavourite = favouritePosts.Any(o => o.Id == item.Id);
        }

        #endregion

        #region Ajax Helper Methods

        [Authorize]
        public async Task<PartialViewResult> GetFavouritePosts()
        {
            if (!Request.IsAjaxRequest() || !User.Identity.IsAuthenticated)
            {
                return null;
            }

            User user = await GetLoggedInUserAsync();

            if (user == null)
            {
                return null;
            }            

            List<PostViewModel> viewModelList = new List<PostViewModel>();

            int[] postsIds = user.FavouritePosts.Select(o => o.Id).ToArray();
            IEnumerable<Post> latestFavouritePosts = await _PostService.GetLatestbyPostIdsAsync(postsIds, AppConfiguration.Instance.LatestUserFavouritePostsInSideBar);

            foreach (var item in latestFavouritePosts)
            {
                PostViewModel viewModel = new PostViewModel();
                TransferFromEntityToBrowseViewModel(item, viewModel);

                viewModelList.Add(viewModel);
            }

            return PartialView("Partials/_FavouritePostsPartial", viewModelList);
        }

        public async Task<PartialViewResult> GetCategoryList()
        {
            if (!Request.IsAjaxRequest())
            {
                return null;
            }

            List<CategoryWithSubcategoriesListViewModel> allCategories = await GetCategoryListAsync();
            return PartialView("Partials/_SidebarCategoriesPartial", allCategories);
        }

        public async Task<PartialViewResult> GetRecentPosts()
        {
            if (!Request.IsAjaxRequest())
            {
                return null;
            }

            // TODO: Specalized call, as this gets too much from DB.
            // TODO: Cache!
            IEnumerable<Post> recentPoosts = await _PostService.GetPagedAsync(1, AppConfiguration.Instance.RecentPostsInSideBar);
            List<PostViewModel> viewModelList = new List<PostViewModel>();

            foreach (var item in recentPoosts)
            {
                PostViewModel viewModel = new PostViewModel();
                TransferFromEntityToBrowseViewModel(item, viewModel);

                viewModelList.Add(viewModel);
            }

            return PartialView("Partials/_RecentPostsPartial", viewModelList);
        }

        public async Task<PartialViewResult> GetTagList()
        {
            if (!Request.IsAjaxRequest())
            {
                return null;
            }

            //TODO: Cache!
            List<TagListViewModel> tags = await GetTagsListAsync();
            return PartialView("Partials/_SidebarTagsPartial", tags);
        }

        #endregion

        #region Private Ajax Helper Methods

        private async Task<List<CategoryWithSubcategoriesListViewModel>> GetCategoryListAsync()
        {
            var categories = await _CategoryService.GetAllCategoriesAsync(); // service already sorts it

            List<CategoryWithSubcategoriesListViewModel> result = new List<CategoryWithSubcategoriesListViewModel>();

            if (categories == null)
            {
                return result;
            }

            FillCategorySelectList(result, categories);
            return result;
        }

        protected void FillCategorySelectList(List<CategoryWithSubcategoriesListViewModel> categorySelectList, IEnumerable<Category> allCategories)
        {
            var rootCategories = allCategories.Where(o => o.ParentId == null);

            foreach (Category rootCategory in rootCategories)
            {
                var viewModel = new CategoryWithSubcategoriesListViewModel
                {
                    Name = rootCategory.Title,
                    Slug = rootCategory.Slug
                };

                if (rootCategory.HasChildren)
                {
                    AppendChildren(viewModel.Subcategories, rootCategory.Children); // children of root are on level 1
                }

                categorySelectList.Add(viewModel);
            }
        }

        private void AppendChildren(List<CategoryWithSubcategoriesListViewModel> subcategories, ICollection<Category> children)
        {
            foreach (var item in children)
            {
                var viewModel = new CategoryWithSubcategoriesListViewModel
                {
                    Name = item.Title,
                    Slug = item.Slug
                };

                if (item.HasChildren)
                {
                    AppendChildren(viewModel.Subcategories, item.Children);
                }

                subcategories.Add(viewModel);
            }
        }

        private async Task<List<TagListViewModel>> GetTagsListAsync()
        {
            var tags = await _TagService.GetAllTagsAsync();

            List<TagListViewModel> result = new List<TagListViewModel>();

            if (tags == null)
            {
                return result;
            }

            foreach (var item in tags)
            {
                result.Add(new TagListViewModel
                {
                    Name = item.Title,
                    Slug = item.Slug
                });
            }

            return result;
        }

        #endregion
    }
}