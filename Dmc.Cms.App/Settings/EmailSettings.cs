using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.App
{
    public class EmailSettings
    {
        public string SmtpHost
        {
            get;
            internal set;
        }

        public string Username
        {
            get;
            internal set;
        }

        public string Password
        {
            get;
            internal set;
        }

        public string SendFromEmail
        {
            get;
            internal set;
        }

        public string SendFromName
        {
            get;
            internal set;
        }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(SmtpHost))
            {
                throw new ArgumentNullException(nameof(SmtpHost));
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                throw new ArgumentNullException(nameof(Username));
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new ArgumentNullException(nameof(Password));
            }

            if (string.IsNullOrWhiteSpace(SendFromEmail))
            {
                throw new ArgumentNullException(nameof(SendFromEmail));
            }
        }
    }
}
