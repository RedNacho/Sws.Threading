using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sws.Threading.Extensions
{
    public static class ObjectExtensions
    {

        public static TProxy ThreadSafeProxy<TProxy>(this TProxy obj) where TProxy : class
        {
            return ConfigureThreadSafeProxy(obj).Build();
        }

        public static ThreadSafeProxyBuilder<TProxy> ConfigureThreadSafeProxy<TProxy>(this TProxy obj) where TProxy : class
        {
            return new ThreadSafeProxyBuilder<TProxy>(obj);
        }

    }
}
