using Dmc.Cms.App;
using Dmc.Cms.App.Identity;
using Dmc.Cms.App.Services;
using Dmc.Cms.Model;
using Dmc.Cms.Repository;
using Dmc.Identity;
using Entangled.Life.Web.Attributes;
using Entangled.Life.Web.Mappers;
using Entangled.Life.Web.ViewModels;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web.Controllers
{
    [Authorize]
    [NoCache]
    public class AccountController : ControllerBase
    {
        #region Fields

        private readonly IPostService _PostService;

        #endregion

        #region Constructors

        public AccountController(IPostService postService, ApplicationUserManager manager) : base(manager)
        {
            _PostService = postService ?? throw new ArgumentNullException(nameof(postService));
        }

        #endregion

        #region Account Details

        public async Task<ActionResult> Index()
        {
            Guid? userId = GetLoggedInUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            User loggedInUser = await UserManager.GetUserWithAllDetailsAsync(userId.Value);
            if (loggedInUser == null)
            {
                return RedirectToAction(nameof(Login));
            }

            int[] favouteIds = loggedInUser.FavouritePosts.Select(o => o.Id).ToArray();
            int[] ratingIds = loggedInUser.Ratings.Select(o => o.PostId).ToArray();
            IEnumerable<Post> allPostsForAllIds = await _PostService.GetAllByPostIdsAsync(favouteIds.Concat(ratingIds).Distinct().ToArray());

            var viewModel = new AccountDetailsViewModel();
            TransferInfoToAccountDetailsViewModel(loggedInUser, allPostsForAllIds, viewModel, favouteIds, ratingIds);

            return View(viewModel);
        }

        //TODO: with mapper
        private void TransferInfoToAccountDetailsViewModel(User loggedInUser, IEnumerable<Post> allPostsForAllIds, AccountDetailsViewModel viewModel, int[] favouteIds, int[] ratingIds)
        {
            // basic things ... 
            viewModel.Email = loggedInUser.Email;
            viewModel.FirstName = loggedInUser.FirstName;
            viewModel.LastName = loggedInUser.LastName;
            viewModel.NickName = loggedInUser.NickName;
            viewModel.MemberSince = loggedInUser.Created;

            // Crete lists from all ...
            IEnumerable<Post> favourites = allPostsForAllIds.Where(o => favouteIds.Contains(o.Id));
            IEnumerable<Post> ratedPosts = allPostsForAllIds.Where(o => ratingIds.Contains(o.Id));

            viewModel.FavouritePosts = PreparePostsViewModelList(viewModel, favourites, loggedInUser.Ratings, favourites); // for favourite every will be, for rated, every will have ... 
            viewModel.RatedPosts = PreparePostsViewModelList(viewModel, ratedPosts, loggedInUser.Ratings, favourites);
        }

        private List<PostViewModel> PreparePostsViewModelList(AccountDetailsViewModel viewModel, IEnumerable<Post> posts, IEnumerable<Rating> ratings, IEnumerable<Post> favourites)
        {
            List<PostViewModel> favouritePostViewModel = new List<PostViewModel>();
            foreach (var item in posts)
            {
                PostViewModel postViewModel = new PostViewModel();

                PostMapper.TransferToViewModel(item, postViewModel);
                AppendRatingAndFavouriteInfo(postViewModel, ratings, favourites);

                favouritePostViewModel.Add(postViewModel);
            }
            return favouritePostViewModel;
        }

        private void AppendRatingAndFavouriteInfo(PostViewModel item, IEnumerable<Rating> ratingsForCurrentUserForThesePosts, IEnumerable<Post> favouritePosts)
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

        #region Delete Account

        public async Task<ActionResult> Delete()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await GetLoggedInUserAsync();
            
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var viewModel = new AccountDeleteViewModel
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MemberSince = user.Created,
                NickName = user.NickName,
                Token = UserManager.CreateDeleteAccountCode(user)
            };

            AddMessageToViewData(MessageType.Warning, @"You are about to delete your account. This action CANNOT be undone.");
            AddMessageToViewData(MessageType.Info, @"You will have to create a completely new account and verify your E-mail again.");

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(AccountDeleteViewModel model)
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            var user = await GetLoggedInUserAsync();

            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await UserManager.DeleteUserAsync(user, model.Token);

            if (!result.Succeeded)
            {
                // regenerate code
                model.Token = UserManager.CreateDeleteAccountCode(user);

                // add error messages
                AddErrorMessagesToViewData(result.Errors);

                // retry
                return View(model);
            }

            IdentitySignout();
            AddMessageToTempData(MessageType.Success, "Your account has been removed successfully. We are sad to see you go. Please remember that you can always create a new account.");
            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new UserLoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLoginViewModel model, string ReturnUrl)
        {
            if (Request.IsAuthenticated)
            {
                IdentitySignout();
                AddMessageToTempData(MessageType.Warning, "Session has expired. Please login again.");
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            var user = await UserManager.FindUserByUserNameAsync(model.Email);

            if (user == null) // should not happen
            {
                AddMessageToViewData(MessageType.Error, "Incorrect E-mail or Password. Please Try again.");
                return View(model);
            }

            if (!user.EmailConfirmed)
            {
                SendEmailConfirmationCode(user);

                AddMessageToViewData(MessageType.Warning, "You need to verify your E-mail address before you can log in.");
                AddMessageToViewData(MessageType.Info, "We have sent another confirmation E-mail to you, please follow the link to activate your account.");

                return View(model);
            }

            // TODO: Helper method to verify using user. that way we don't have to go to DB twice
            var result = await UserManager.VerifyUserCredentialsAsync(model.Email, model.Password);

            if (!result.Succeeded)
            {
                AddErrorMessagesToViewData(result.Errors);
                return View(model);
            }

            var identity = await UserManager.CreateIdentityAsync(user, IdentityAuthenticationTypes.ApplicationCookie);
            IdentitySignin(identity, model.RememberMe);

            return RedirectToLocal(ReturnUrl ?? "/");
        }

        #endregion

        #region Forgot Password

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            if (Request.IsAuthenticated) // should not come here if already logged in
            {
                IdentitySignout();
                return RedirectToAction(nameof(ForgotPassword));
            }
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            var user = await UserManager.FindUserByEmailAsync(model.Email);

            if (user == null || !user.EmailConfirmed || UserManager.IsLockedOut(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            SendEmailResetCode(user);

            // If we got this far, something failed, redisplay form
            return View(nameof(ForgotPasswordConfirmation));
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            if (Request.IsAuthenticated)
            {
                IdentitySignout();
                AddMessageToTempData(MessageType.Warning, "Session has expired. Please login again.");
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        #endregion

        #region Reset Password

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(Guid? userId, string code)
        {
            if (!userId.HasValue || string.IsNullOrWhiteSpace(code))
            {
                AddErrorMessagesToTempData(new List<string> { "An error occured.", "Please verify that you clicked the link in the E-mail you received.", "You can also copy/paste the reset link in your browser Address bar." });
                return Error();
            }

            if (!(await UserManager.IsResetPasswordCodeCorrectAsync(userId.Value, code)))
            {
                AddErrorMessagesToTempData(new List<string> { "An error occured.", "Please verify that you clicked the link in the E-mail you received.", "You can also copy/paste the reset link in your browser Address bar." });
                return Error();
            }

            if (Request.IsAuthenticated)
            {
                IdentitySignout();
            }

            ResetPasswordViewModel viewModel = new ResetPasswordViewModel
            {
                Code = code
            };

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (Request.IsAuthenticated)
            {
                IdentitySignout();
                AddMessageToTempData(MessageType.Warning, "Session has expired. Please login again.");
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            var user = await UserManager.FindUserByEmailAsync(model.Email);

            if (user == null || !user.EmailConfirmed)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (result.Succeeded)
            {
                LoggedInUserInfo userInfo = new LoggedInUserInfo();
                userInfo.FromUser(user);
                IdentitySignin(userInfo, false);
                AddMessageToTempData(MessageType.Success, "Your password has been changed. Please keep your password safe.");

                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            AddErrorMessagesToTempData(result.Errors);
            return View(model);
        }

        #endregion

        #region Change Password

        public ActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            Guid? userId = GetLoggedInUserId();

            if (!userId.HasValue)
            {
                return Error();
            }

            var result = await UserManager.ChangePasswordAsync(userId.Value, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                AddErrorMessagesToViewData(result.Errors);
                return View(model);
            }

            IdentitySignout();
            AddMessageToTempData(MessageType.Success, "You have changed your password. Please Login using your new password.");
            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new UserRegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserRegisterViewModel model)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                ModelState.AddModelError("", "Validation Has failed. Please correct the errors and try again.");
                return View(model);
            }

            User user = new User
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await UserManager.CreateUserAsync(user, model.Password);

            if (result.Succeeded)
            {
                SendEmailConfirmationCode(user);
                return RedirectToAction(nameof(RegisterSuccess));
            }

            AddToModelStateMessagesFromMembershipResult(result);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterSuccess()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        #endregion

        #region Confirm E-mail

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(Guid? userId, string code)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!userId.HasValue || string.IsNullOrWhiteSpace(code))
            {
                AddErrorMessagesToTempData(new List<string> { "An error occured.", "Please verify that you clicked the link in the E-mail you received." });
                return RedirectToAction(nameof(Login));
            }

            var result = await UserManager.VerifyEmailAsync(userId.Value, code);

            if (result.Succeeded)
            {
                AddMessageToTempData(MessageType.Success, "Thank you for activating your account. You may now login.");
                return RedirectToAction(nameof(Login));
            }

            AddErrorMessagesToViewData(result.Errors);
            return View("Error");
        }

        #endregion

        #region Logoff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LogOff()
        {
            IdentitySignout();
            return Json(new { success = true });
        }

        #endregion

        #region Child Actions

        [AllowAnonymous]
        [ChildActionOnly]
        public PartialViewResult HeaderLogin()
        {
            return PartialView("Partials/_HeaderLoginPartial", new UserLoginViewModel());
        }

        #endregion

        #region Private Methods

        private bool SendEmailResetCode(User user)
        {
            string token = UserManager.CreatePasswordResetCode(user);
            string callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.UniqueId, code = token }, protocol: Request.Url.Scheme);
            ConfirmEmailViewModel viewModel = new ConfirmEmailViewModel
            {
                Link = callbackUrl
            };

            try
            {
                string html = this.RenderViewToString("~/Views/Email/ResetPassword.cshtml", viewModel);
                EmailClient client = CreateEmailClient();
                client.Send(user.Email, "Reset your password", html);

                return true;
            }
            catch (Exception) //TODO: Log
            {
                return false;
            }
        }

        private bool SendEmailConfirmationCode(User user)
        {
            string code = UserManager.CreateEmailVerificationCode(user);
            string callbackUrl = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.UniqueId, code = code }, protocol: Request.Url.Scheme);
            ConfirmEmailViewModel viewModel = new ConfirmEmailViewModel
            {
                Link = callbackUrl
            };

            try
            {
                string html = this.RenderViewToString("~/Views/Email/ConfirmEmail.cshtml", viewModel);
                EmailClient client = CreateEmailClient();
                client.Send(user.Email, "Confirm your account", html);

                return true;
            }
            catch (Exception) //TODO: Log
            {
                return false;
            }
        }

        #endregion

        #region Helpers

        // copied from identity ... 
        private const string XsrfKey = "XsrfId";

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}