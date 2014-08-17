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

        /// <summary>
        /// Creates a thread-safe proxy of the supplied object, similar to adding "synchronized" to all members in Java.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>

        public static TProxy ThreadSafeProxy<TProxy>(this TProxy obj) where TProxy : class
        {
            return ConfigureThreadSafeProxy(obj).Build();
        }

        /// <summary>
        /// Creates a thread-safe proxy builder which you can use to configure a thread-safe proxy for the object.
        /// Call Build() when finished.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>

        public static ThreadSafeProxyBuilder<TProxy> ConfigureThreadSafeProxy<TProxy>(this TProxy obj) where TProxy : class
        {
            return new ThreadSafeProxyBuilder<TProxy>(obj);
        }

    }
}
