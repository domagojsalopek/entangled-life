﻿using Dmc.Cms.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.Repository.Ef.Configuration
{
    internal class PageConfiguration : DatabaseEntityConfiguration<Page>
    {
        #region Constructor

        internal PageConfiguration() 
        {
            ConfigureProperties();
            ConfigureRelationships();
        }

        #endregion

        #region Private Methods

        private void ConfigureProperties()
        {
            Ignore(o => o.CanBeDisplayed);

            Property(o => o.Title)
                .IsRequired()
                .HasMaxLength(255);

            Property(o => o.Slug)
                .IsRequired()
                .HasMaxLength(255);

            Property(o => o.Content)
                .HasColumnType("ntext")
                .IsRequired();

            Property(o => o.Description)
                .IsOptional()
                .HasMaxLength(4000);

            Property(o => o.Order)
                .IsRequired();            
        }

        private void ConfigureRelationships()
        {
            HasOptional(o => o.Author) // We must change this to required when membership is done
                .WithMany()
                .HasForeignKey(o => o.AuthorId)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.PreviewImage)
                .WithMany()
                .HasForeignKey(o => o.PreviewImageId)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.DetailImage)
                .WithMany()
                .HasForeignKey(o => o.DetailImageId)
                .WillCascadeOnDelete(false);

            HasMany(a => a.Gallery).WithMany().Map(
                at => at.MapLeftKey("PageId")
                        .MapRightKey("ImageId")
                        .ToTable("PageImages"));
        }

        #endregion
    }
}
