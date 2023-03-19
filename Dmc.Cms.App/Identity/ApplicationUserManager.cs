using Dmc.Cms.App.Services;
using Dmc.Cms.Model;
using Dmc.Cms.Repository;
using Dmc.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.App.Identity
{
    public class ApplicationUserManager : UserManager<User>, IService
    {
        #region Private Fields

        private ICmsUnitOfWork _CmsUnitOfWork;

        #endregion

        #region Constructors

        public ApplicationUserManager(ApplicationUserStore userStore, ICmsUnitOfWork cmsUnitOfWork) 
            : base(userStore)
        {
            _CmsUnitOfWork = cmsUnitOfWork;
        }

        #endregion

        #region Helper Methods

        public async Task<User> GetUserWithAllDetailsAsync(Guid uniqueId)
        {
            ApplicationUserStore store = UserStore as ApplicationUserStore;
            return await store.GetWithAllDetailsAsync(uniqueId);
        }

        #endregion  

        #region Identity

        public override async Task<ClaimsIdentity> CreateIdentityAsync(User user, string authenticationType)
        {
            // will create NameIdentifier and roles
            var identity = await base.CreateIdentityAsync(user, authenticationType);

            LoggedInUserInfo appUserState = new LoggedInUserInfo();
            appUserState.FromUser(user);
            appUserState.ExpiresUtc = DateTimeOffset.UtcNow.Add(AppConfiguration.Instance.GetLoginDuration());

            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName));
            identity.AddClaim(new Claim(AppConstants.LoggedInUserInfoKey, appUserState.Serialize()));

            return identity;
        }

        #endregion

        #region Additional Overrides

        public override async Task<MembershipResult> DeleteUserAsync(User user, string token)
        {
            var userToDelete = await GetUserWithAllDetailsAsync(user.UniqueId);

            DeleteRatings(userToDelete);
            await DeleteQueries(userToDelete);

            return await base.DeleteUserAsync(userToDelete, token);
        }

        private async Task DeleteQueries(User userToDelete)
        {
            var queriesToChange = await _CmsUnitOfWork.ContactRepository.GetAllForUserAsync(userToDelete.Id);

            foreach (var item in queriesToChange)
            {
                //item.UserId = null; // deassociate
                _CmsUnitOfWork.ContactRepository.Delete(item);
            }

            // TODO: is this enough?
        }

        private void DeleteRatings(User userToDelete)
        {
            var ratingsToDelete = new List<Rating>();
            ratingsToDelete.AddRange(userToDelete.Ratings);

            foreach (var item in ratingsToDelete)
            {
                _CmsUnitOfWork.RatingRepository.Delete(item);
            }
        }

        #endregion
    }
}
