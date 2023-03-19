using Dmc.Cms.App;
using Dmc.Cms.App.Identity;
using Dmc.Cms.App.Services;
using Dmc.Cms.Model;
using Entangled.Life.Web.Attributes;
using Entangled.Life.Web.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Entangled.Life.Web.Controllers
{
    [NoCache]
    public class ContactController : ControllerBase
    {
        private readonly IContactQueryService _ContactQueryService;

        public ContactController(IContactQueryService contactQueryService, ApplicationUserManager manager) : base(manager)
        {
            _ContactQueryService = contactQueryService;
        }

        public ActionResult Index()
        {
            return View(new ContactQueryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ContactQueryViewModel model)
        {
            if (!ModelState.IsValid || !HoneyPotCheckValid(EnvironmentKeys.HoneyPotFieldName))
            {
                return View(model);
            }

            string captcha = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrWhiteSpace(captcha) || !await CaptchaSuccess(captcha))
            {
                AddMessageToViewData(MessageType.Error, "Please indicate that you are not a robot.");
                return View(model);
            }

            ContactQuery contactQuery = CreateFromViewModel(model);

            if (User.Identity.IsAuthenticated)
            {
                User user = await GetLoggedInUserAsync();

                if (user != null)
                {
                    contactQuery.UserId = user.Id;
                }
            }

            var saveResult = await _ContactQueryService.InsertAsync(contactQuery);

            if (!saveResult.Success)
            {
                AddMessageToViewData(Dmc.Cms.App.MessageType.Error, "An error occured. Please Try Again.");
                return View(model);
            }

            model.Id = contactQuery.Id; // need this to render ... 
            SendEmailWithQueryToSiteOwner(contactQuery, model);

            if (model.SendMeACopy)
            {
                //SendEmailWithQueryToCustomer(contactQuery, model);
            }
                                    
            return View("MessageSentSuccess", model);
        }

        private async Task<bool> CaptchaSuccess(string captcha)
        {
            HttpClient httpClient = new HttpClient();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", AppConfiguration.Instance.RecaptchaSecretKey),
                new KeyValuePair<string, string>("response", captcha),
                new KeyValuePair<string, string>("remoteip", Request.UserHostAddress)
            });

            try
            {
                var result = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", formContent);

                if (result == null || result.Content == null)
                {
                    return false;
                }

                string responseAsString = await result.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseAsString))
                {
                    return false;
                }

                dynamic googleResponse = JsonConvert.DeserializeObject(responseAsString);
                if (googleResponse == null)
                {
                    return false;
                }

                string successAsString = googleResponse.success;
                if (!bool.TryParse(successAsString, out bool success))
                {
                    return false;
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }        

        private bool SendEmailWithQueryToSiteOwner(ContactQuery contactQuery, ContactQueryViewModel model)
        {
            try
            { 
                string html = this.RenderViewToString("~/Views/Email/AdminContactQuery.cshtml", model);
                EmailClient client = CreateEmailClient();
                client.Send(AppConfiguration.Instance.MainContactEmail, "[Entangled Life] New Contact Query", html);

                return true;
            }
            catch (Exception) //TODO: Log
            {
                return false;
            }
        }

        private bool SendEmailWithQueryToCustomer(ContactQuery contactQuery, ContactQueryViewModel model)
        {
            try
            {
                string html = this.RenderViewToString("~/Views/Email/ContactQuery.cshtml", model);
                EmailClient client = CreateEmailClient();
                client.Send(contactQuery.Email, "[Entangled Life] Your Copy of Contact Query", html);

                return true;
            }
            catch (Exception) //TODO: Log
            {
                return false;
            }
        }

        private ContactQuery CreateFromViewModel(ContactQueryViewModel model)
        {
            return new ContactQuery
            {
                Email = model.Email,
                IP = Request.UserHostAddress,
                Message = model.Message,
                Name = model.Name,
                Subject = model.Subject,
            };
        }
    }
}