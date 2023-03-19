using Dmc.Cms.App.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dmc.Cms.App.Identity;
using System.Threading.Tasks;
using Dmc.Cms.Model;

namespace Entangled.Life.Web.Controllers
{
    [Authorize]
    public class ServiceController : ControllerBase //TODO: these things should be moved to web api ... 
    {
        #region Fields

        private readonly IPostService _PostService;

        #endregion

        #region Constructors

        public ServiceController(IPostService postService, ApplicationUserManager manager) : base(manager)
        {
            _PostService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        #endregion

        #region Favorite

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddToFavourites(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { allowed = false });
            }

            if (!id.HasValue)
            {
                return Json(new { allowed = true, success = false, message = "Invalid request." });
            }

            Guid? userId = GetLoggedInUserId();
            if (!userId.HasValue)
            {
                return Json(new { allowed = true, success = false, message = "Invalid request." });
            }

            ServiceResult postRateResult = await _PostService.AddToFavouritesAsync(userId.Value, id.Value);
            if (postRateResult == null)
            {
                return Json(new { allowed = true, success = false, message = "An error occured. Please try again." });
            }

            return Json(new { allowed = true, success = postRateResult.Success });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveFromFavourites(int? id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { allowed = false });
            }

            if (!id.HasValue)
            {
                return Json(new { allowed = true, success = false, message = "Invalid request." });
            }

            Guid? userId = GetLoggedInUserId();
            if (!userId.HasValue)
            {
                return Json(new { allowed = true, success = false, message = "Invalid request." });
            }

            ServiceResult postRateResult = await _PostService.RemoveFromFavouritesAsync(userId.Value, id.Value);
            if (postRateResult == null)
            {
                return Json(new { allowed = true, success = false, message = "An error occured. Please try again." });
            }

            return Json(new { allowed = true, success = postRateResult.Success });
        }

        #endregion

        #region Rate Methods

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RatePost(int? id, bool like = false)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { allowed = false });
            }

            if (!id.HasValue)
            {
                return Json(new { allowed = true, success = false, message = "Invalid request." });
            }

            User user = await GetLoggedInUserAsync();
            if (user == null)
            {
                return Json(new { allowed = false });
            }

            ServiceResult postRateResult = await _PostService.RatePostAsync(user, id.Value, like);
            if (postRateResult == null)
            {
                return Json(new { allowed = true, success = false, message = "An error occured. Please try again." });
            }

            return Json(new { allowed = true, success = postRateResult.Success });
        }

        #endregion
    }
}