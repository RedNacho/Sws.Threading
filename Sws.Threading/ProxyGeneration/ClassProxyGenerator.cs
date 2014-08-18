using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.ProxyGeneration
{

    public class ClassProxyGenerator : CastleProxyGenerator
    {
        public ClassProxyGenerator(ProxyGenerator proxyGenerator) : base(proxyGenerator) { }

        public override bool CanProxy<TProxy>()
        {
            return typeof(TProxy).IsClass;
        }

        protected override TProxy InvokeCastleProxyGenerator<TProxy>(ProxyGenerator proxyGenerator, TProxy target, IInterceptor interceptor)
        {
            return proxyGenerator.CreateClassProxyWithTarget<TProxy>(target, interceptor);
        }
    }

}
