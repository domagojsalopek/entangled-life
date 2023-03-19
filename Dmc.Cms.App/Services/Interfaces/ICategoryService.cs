using Dmc.Cms.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Cms.App.Services
{
    public interface ICategoryService : ICrudService<Category>, IContentService<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<IEnumerable<Category>> GetCategoriesForIdsAsync(int[] categoryIds);
    }
}
