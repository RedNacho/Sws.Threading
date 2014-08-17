using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.ProxyGeneration
{
    internal abstract class CastleProxyGenerator : IProxyGenerator
    {
        private readonly ProxyGenerator _proxyGenerator;

        protected CastleProxyGenerator(ProxyGenerator proxyGenerator)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException("proxyGenerator");
            }

            _proxyGenerator = proxyGenerator;
        }

        public abstract bool CanProxy<TProxy>() where TProxy : class;

        public abstract TProxy InvokeCastleProxyGenerator<TProxy>(ProxyGenerator proxyGenerator, TProxy target, IInterceptor interceptor) where TProxy : class;

        public TProxy Generate<TProxy>(TProxy target, IInterceptor interceptor) where TProxy : class
        {
            return InvokeCastleProxyGenerator(_proxyGenerator, target, interceptor);
        }

    }    
}
