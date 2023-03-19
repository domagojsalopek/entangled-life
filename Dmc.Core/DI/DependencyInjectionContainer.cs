using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Core.DI
{
    // caching needs to be much better as we should cache the constructor infos we resolved etc
    // this was made for experimantationa nd testing, not to really use ... 
    public class DependencyInjectionContainer 
    {
        #region Private Fields

        private readonly IList<DependencyInjectionRegisteredObject> _RegisteredObjects = new List<DependencyInjectionRegisteredObject>();

        #endregion

        #region Public Methods

        public void Register<TTypeToResolve, TConcrete>()
        {
            _RegisteredObjects.Add(new DependencyInjectionRegisteredObject(typeof(TTypeToResolve), typeof(TConcrete)));
        }

        public object Resolve(Type typeToResolve, Guid requestId) // we count that this is called once per request for controller constructor.
        {
            string key = requestId.ToString();
            DIPerRequestFactory factory = GetOrCreateFactory(key);
            return factory.Resolve(typeToResolve);
        }

        private DIPerRequestFactory GetOrCreateFactory(string key)
        {
            const int cacheDurationInSeconds = 15; // even this is too much, maybe we shouldn't use cache, but private property ... 
            DIPerRequestFactory factory;
            if (System.Runtime.Caching.MemoryCache.Default.Contains(key))
            {
                factory = System.Runtime.Caching.MemoryCache.Default[key] as DIPerRequestFactory;
            }
            else
            {
                factory = new DIPerRequestFactory(new List<DependencyInjectionRegisteredObject>(_RegisteredObjects));
                System.Runtime.Caching.MemoryCache.Default.Add(key, factory, DateTime.Now.AddSeconds(cacheDurationInSeconds));
            }

            return factory;
        }

        #endregion
    }
}
