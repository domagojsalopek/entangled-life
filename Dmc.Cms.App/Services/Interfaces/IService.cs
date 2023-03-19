using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dmc.Cms.App.Services
{
    public interface IService
    {
    }

    public interface IService<T> : IService where T : class
    {
        Task<IEnumerable<T>> GetPagedAsync(int page, int perPage);

        Task<int> CountAsync();

        Task<T> GetByIdAsync(int id);
    }
}