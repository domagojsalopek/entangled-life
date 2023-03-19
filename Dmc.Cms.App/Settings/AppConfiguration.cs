using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.App
{
    public sealed class AppConfiguration
    {
        #region Lazy

        private static readonly Lazy<AppConfiguration> _Lazy = new Lazy<AppConfiguration>(() => new AppConfiguration());

        #endregion

        #region Constants

        private const int _LoginDurationInMinutes = (60 * 24) * 7;

        #endregion

        #region Fields

        private EmailSettings _EmailSettings;
        private string _SiteName;
        private string _RecaptchaKey;
        private string _RecaptchaSecretKey;

        private int _DefaultItemsPerPage = 10;
        private int _DefaultFavoritePostsInSidebar = 5;
        private int _DefaultRecentPostsInSidebar = 3;
        private string _MainContactEmail;

        #endregion

        #region Properties

        public string MainContactEmail => _MainContactEmail;

        public EmailSettings EmailSettings => _EmailSettings;

        public int DefaultItemsPerPage => _DefaultItemsPerPage;

        public int LatestUserFavouritePostsInSideBar => _DefaultFavoritePostsInSidebar;

        public int RecentPostsInSideBar => _DefaultRecentPostsInSidebar;

        //TODO: a lot of these things should be read from site settings.
        public string SiteName => _SiteName;
        public string RecaptchaSiteKey => _RecaptchaKey;
        public string RecaptchaSecretKey => _RecaptchaSecretKey;

        #endregion

        #region Constructors

        private AppConfiguration()
        {
            Configure();
        }

        #endregion

        #region Singleton Instance

        public static AppConfiguration Instance
        {
            get
            {
                return _Lazy.Value;
            }
        }

        #endregion

        #region Methods

        public TimeSpan GetLoginDuration()
        {
            return TimeSpan.FromMinutes(_LoginDurationInMinutes);
        }

        #endregion

        #region Configuration Methods

        public void Configure()
        {
            CreateBasicSiteSettings();
            CreateMailSettings();
        }

        private void CreateBasicSiteSettings() // TODO: these settings we should read from DB configuration. let's do this in next version
        {
            _SiteName = ConfigurationManager.AppSettings["SiteName"];
            _RecaptchaKey = ConfigurationManager.AppSettings["RecaptchaSiteKey"];
            _RecaptchaSecretKey = ConfigurationManager.AppSettings["RecaptchaSecretKey"];
            _MainContactEmail = ConfigurationManager.AppSettings["MainContactEmail"];

            if (int.TryParse(ConfigurationManager.AppSettings["DefaultItemsPerPage"], out int itemsPerPage))
            {
                _DefaultItemsPerPage = itemsPerPage;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["LatestUserFavouritePostsInSideBar"], out int favouritePosts))
            {
                _DefaultFavoritePostsInSidebar = favouritePosts;
            }

            if (int.TryParse(ConfigurationManager.AppSettings["DefaultRecentPostsInSidebar"], out int recentPosts))
            {
                _DefaultRecentPostsInSidebar = recentPosts;
            }
        }

        private void CreateMailSettings()
        {
            _EmailSettings = new EmailSettings();
            // this all shuld be in configuration file which should be managed through admin.
            _EmailSettings.SmtpHost = ConfigurationManager.AppSettings["SMTPServer"];
            _EmailSettings.Username = ConfigurationManager.AppSettings["SMTPUsername"];
            _EmailSettings.Password = ConfigurationManager.AppSettings["SMTPPassword"];
            _EmailSettings.SendFromEmail = ConfigurationManager.AppSettings["SendFromEmail"];
            _EmailSettings.SendFromName = ConfigurationManager.AppSettings["SendFromDisplayName"];
            _EmailSettings.Validate();
        }

        #endregion
    }
}
