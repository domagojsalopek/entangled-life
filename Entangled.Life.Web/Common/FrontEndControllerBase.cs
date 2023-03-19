﻿using Dmc.Cms.App;
using Dmc.Cms.App.Identity;
using Dmc.Cms.App.Services;
using Dmc.Cms.Core;
using Dmc.Cms.Model;
using Dmc.Cms.Repository;
using Dmc.Core;
using Dmc.Identity;
using Entangled.Life.Web.ViewModels;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web
{
    public abstract class FrontEndControllerBase<TEntity, TViewModel> : FrontEndControllerBase<TEntity, TViewModel, TViewModel>
        where TEntity : class, IContent
        where TViewModel : class, IContentViewModel, new()
    {
        public FrontEndControllerBase(IContentService<TEntity> service, ApplicationUserManager manager)
            : base(service, manager)
        {
        }
    }

    public abstract class FrontEndControllerBase<TEntity, TBrowseViewModel, TDetailsViewModel> : ControllerBase
        where TEntity : class, IContent
        where TBrowseViewModel : class, IContentViewModel, new()
        where TDetailsViewModel : class, IContentViewModel, new()
    {
        private readonly IContentService<TEntity> _Service;

        protected FrontEndControllerBase(IContentService<TEntity> service, ApplicationUserManager manager) : base(manager)
        {
            _Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected IContentService<TEntity> Service => _Service;

        protected ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View("NotFound");
        }

        protected virtual async Task<ActionResult> ListAsync(int? page, int? perPage)
        {
            EntityList<TBrowseViewModel> viewModelListResult = await GetEntityListAsync(page, perPage);
            return ListView(viewModelListResult);
        }

        protected async Task<EntityList<TBrowseViewModel>> GetEntityListAsync(int? page, int? perPage)
        {
            page = (!page.HasValue || (page.HasValue && page.Value < 1))
                ? 1
                : page;

            perPage = (!perPage.HasValue || (perPage.HasValue && perPage.Value < 1))
                ? AppConfiguration.Instance.DefaultItemsPerPage
                : perPage;

            var routeData = Request.RequestContext.RouteData.Values;
            IEnumerable<TEntity> entities = await GetDisplayablePagedEntityListAsync(page, perPage, routeData);
            int totalNumberOfEntities = await CountDisplayableEntitiesAsync(routeData);

            EntityList<TBrowseViewModel> viewModelListResult = CreateViewModelList(entities, page.Value, perPage.Value, totalNumberOfEntities);
            return viewModelListResult;
        }

        private async Task<int> CountDisplayableEntitiesAsync(IDictionary<string, object> routeData)
        {
            TryResolveLoggedInUserId();
            if (LoggedInUserId.HasValue && CanUserSeeDrafts(LoggedInUserId.Value, User.Identity as ClaimsIdentity))
            {
                return await CountPublishedAndDraftsAsync(routeData);
            }
            return await CountPublishedAsync(routeData);
        }

        protected virtual async Task<int> CountPublishedAsync(IDictionary<string, object> routeData)
        {
            return await _Service.CountAllPublishedAsync();
        }

        protected virtual async Task<int> CountPublishedAndDraftsAsync(IDictionary<string, object> routeData)
        {
            return await _Service.CountAllPublishedAndDraftsAsync();
        }

        private async Task<IEnumerable<TEntity>> GetDisplayablePagedEntityListAsync(int? page, int? perPage, IDictionary<string, object> routeData)
        {
            TryResolveLoggedInUserId();
            if (LoggedInUserId.HasValue && CanUserSeeDrafts(LoggedInUserId.Value, User.Identity as ClaimsIdentity))
            {
                return await GetPagedPublishedAndDraftsAsync(page.Value, perPage.Value, routeData);
            }
            return await GetPagedPublishedAsync(page.Value, perPage.Value, routeData);
        }

        protected virtual async Task<IEnumerable<TEntity>> GetPagedPublishedAsync(int page, int perPage, IDictionary<string, object> routeData)
        {
            return await _Service.GetPagedPublishedAsync(page, perPage);
        }

        protected virtual async Task<IEnumerable<TEntity>> GetPagedPublishedAndDraftsAsync(int page, int perPage, IDictionary<string, object> routeData)
        {
            return await _Service.GetPagedPublishedAndDraftsAsync(page, perPage);
        }

        protected virtual EntityList<TBrowseViewModel> CreateViewModelList(IEnumerable<TEntity> entities, int page, int perPage, int totalNumberOfEntities)
        {
            List<TBrowseViewModel> viewModelListResult = new List<TBrowseViewModel>();

            foreach (var entity in entities)
            {
                if (!IsEntityDisplayable(entity))
                {
                    continue;
                }

                TBrowseViewModel browseViewModel = new TBrowseViewModel();

                TransferFromEntityToBrowseViewModel(entity, browseViewModel);

                browseViewModel.Id = entity.Id;
                browseViewModel.Slug = entity.Slug;
                browseViewModel.Status = entity.Status;
                viewModelListResult.Add(browseViewModel);
            }

            return new EntityList<TBrowseViewModel>(new EntityList(viewModelListResult, page, perPage, totalNumberOfEntities));
        }

        protected virtual async Task<TDetailsViewModel> GetEntityAndLoadViewModelAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            TEntity entity = await _Service.FindBySlugAsync(slug);

            if (!IsEntityDisplayable(entity))
            {
                return null;
            }

            // here we know we can show it

            TDetailsViewModel detailsViewModel = new TDetailsViewModel();

            TransferFromEntityToDetailsViewModel(entity, detailsViewModel);
            detailsViewModel.Id = entity.Id;
            detailsViewModel.Slug = entity.Slug;
            detailsViewModel.Status = entity.Status;

            return detailsViewModel;
        }

        protected virtual async Task<ActionResult> EntityDetailsAsync(string slug)
        {
            var result = await GetEntityAndLoadViewModelAsync(slug);

            if (result == null)
            {
                return NotFound();
            }

            return DetailView(result);
        }

        protected virtual bool IsEntityDisplayable(IContent entity) // should this be called for lists?
        {
            TryResolveLoggedInUserId();
            if (entity.Status == ContentStatus.Published)
            {
                return true;
            }

            if (entity.Status == ContentStatus.Draft && User.Identity.IsAuthenticated)
            {
                return LoggedInUserId.HasValue && CanUserSeeDrafts(LoggedInUserId.Value, User.Identity as ClaimsIdentity);
            }

            return false;
        }

        protected void TransferToImageViewModel(Image source, ImageViewModel destination)
        {
            destination.AltText = source.AltText;
            destination.Description = source.Caption;
            destination.LargeImage = source.LargeImage;
            destination.Name = source.Name;
            destination.SmallImage = source.SmallImage;
        }

        internal virtual bool CanUserSeeDrafts(Guid userId, ClaimsIdentity claimsIdentity)
        {
            return User.IsInRole(RoleKeys.Admin);
        }

        internal virtual ActionResult ListView(EntityList<TBrowseViewModel> entityList)
        {
            return View("List", entityList);
        }

        internal virtual ActionResult DetailView(TDetailsViewModel detailsViewModel)
        {
            return View("Details", detailsViewModel);
        }

        internal abstract void TransferFromEntityToDetailsViewModel(TEntity entity, TDetailsViewModel viewModel);
        internal abstract void TransferFromEntityToBrowseViewModel(TEntity entity, TBrowseViewModel viewModel);
    }
}