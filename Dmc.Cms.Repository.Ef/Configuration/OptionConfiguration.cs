using Dmc.Cms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.Repository.Ef.Configuration
{
    internal class OptionConfiguration : DatabaseEntityConfiguration<Option>
    {
        #region Constructor

        internal OptionConfiguration()
        {
            ConfigureProperties();
            ConfigureRelationships();
        }

        #endregion

        #region Private Methods

        private void ConfigureProperties()
        {
            Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(255);

            Property(o => o.Value)
                .IsRequired()
                .HasMaxLength(255);
        }

        private void ConfigureRelationships()
        {
        }

        #endregion
    }
}
