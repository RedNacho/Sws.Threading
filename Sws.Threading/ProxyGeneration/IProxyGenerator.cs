using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.ProxyGeneration
{

    public interface IProxyGenerator
    {
        bool CanProxy<TProxy>() where TProxy : class;
        TProxy Generate<TProxy>(TProxy target, IInterceptor interceptor) where TProxy : class;
    }

}
