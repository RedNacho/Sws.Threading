using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.ProxyGeneration
{

    public class CompositeProxyGenerator : IProxyGenerator
    {

        private readonly IProxyGenerator[] _components;

        public CompositeProxyGenerator(params IProxyGenerator[] components)
        {
            _components = components;
        }

        public bool CanProxy<TProxy>() where TProxy : class
        {
            return _components.Any(component => component.CanProxy<TProxy>());
        }

        public TProxy Generate<TProxy>(TProxy target, IInterceptor interceptor) where TProxy : class
        {
            return _components.Aggregate((TProxy)null,
                (accumulate, component) => accumulate == null && component.CanProxy<TProxy>()
                    ? component.Generate(target, interceptor)
                    : accumulate);
        }
    }

}
