using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.ViewModels
{
    public class AccountDetailsViewModel
    {
        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string NickName
        {
            get;
            set;
        }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
                {
                    return string.Format("{0} {1}", FirstName, LastName);
                }

                if (!string.IsNullOrWhiteSpace(NickName))
                {
                    return NickName;
                }

                return Email;
            }
        }

        public DateTimeOffset MemberSince
        {
            get;
            set;
        }

        public List<PostViewModel> FavouritePosts
        {
            get;
            set;
        }

        public List<PostViewModel> RatedPosts
        {
            get;
            set;
        }
    }
}