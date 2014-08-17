using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.ProxyGeneration
{
    internal class InterfaceProxyGenerator : CastleProxyGenerator
    {
        public InterfaceProxyGenerator(ProxyGenerator proxyGenerator) : base(proxyGenerator) { }

        public override bool CanProxy<TProxy>()
        {
            return typeof(TProxy).IsInterface;
        }

        public override TProxy InvokeCastleProxyGenerator<TProxy>(ProxyGenerator proxyGenerator, TProxy target, IInterceptor interceptor)
        {
            return proxyGenerator.CreateInterfaceProxyWithTarget<TProxy>(target, interceptor);
        }
    }
}
