using Dmc.Cms.App.Services;
using Dmc.Cms.Model;
using Dmc.Core.DI;
using Dmc.Identity;
using Dmc.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Entangled.Life.Web
{
    public class CmsControllerFactory : DefaultControllerFactory
    {
        #region Private Fields

        private readonly DependencyInjectionContainer _Container;
        private static readonly Type[] _SupportedTypes = new Type[] { typeof(IService), typeof(IIdentityUnitOfWork<User>), typeof(IUnitOfWork) };

        #endregion

        #region Constructors

        //TODO !!! CACHE!!!!!

        public CmsControllerFactory(DependencyInjectionContainer container)
        {
            _Container = container ?? throw new ArgumentNullException(nameof(container));
        }

        #endregion

        #region Overrides

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null) // can this happen?
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            var constructors = controllerType.GetConstructors(); //TODO: better, resolve all ... ?

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();

                if (!parameters.All(o => (_SupportedTypes.Any(s => s.IsAssignableFrom(o.ParameterType)))))
                {
                    continue;
                }

                Guid requestId = Guid.NewGuid();
                List<object> resolved = new List<object>();

                foreach (var item in parameters)
                {
                    resolved.Add(_Container.Resolve(item.ParameterType, requestId)); // for each resolve during request id must be the same
                }

                return (IController)constructor.Invoke(resolved.ToArray());
            }

            return base.GetControllerInstance(requestContext, controllerType);
        }

        #endregion
    }
}