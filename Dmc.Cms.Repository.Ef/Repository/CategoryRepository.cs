﻿using Dmc.Cms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Repository;

namespace Dmc.Cms.Repository.Ef
{
    internal class CategoryRepository : ContentRepositoryBase<Category>, ICategoryRepository
    {
        internal CategoryRepository(IRepository<Category> repository) : base(repository)
        {
        }

        public override async Task<Category> GetByIdAsync(int id)
        {
            return await Repository
                .Query()
                .Include(o => o.IntroImage)
                .Include(o => o.Children)
                .Filter(o => o.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await Repository
                .Query()
                //.Include(o => o.Children)
                //.Include(o => o.Children.Select(s => s.Children))
                .GetEntitiesAsync(); // but this will get ALL! so we can sort in memory!! we should do this!
        }

        public async Task<IEnumerable<Category>> GetCategoriesForIdsAsync(int[] categoryIds)
        {
            return await Repository
                .Query()
                //.Include(o => o.Children.Where(d => categoryIds.Contains(d.Id)))
                .Filter(o => categoryIds.Contains(o.Id))
                .GetEntitiesAsync();
        }

        public override async Task<Category> GetBySlugAsync(string slug)
        {
            return await Repository
                .Query()
                .Include(o => o.IntroImage)
                .Include(o => o.Children)
                .Filter(o => o.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();
        }
    }
}
