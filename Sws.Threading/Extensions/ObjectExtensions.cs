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
        /// Creates a thread-safe proxy of the supplied object, similar to adding "synchronized" to all members in Java.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="proxyType"></param>
        /// <returns></returns>

        public static object ThreadSafeProxy(this object obj, Type proxyType)
        {
            return ConfigureThreadSafeProxy(obj, proxyType).Build();
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

        /// <summary>
        /// Creates a thread-safe proxy builder which you can use to configure a thread-safe proxy for the object.
        /// Call Build() when finished.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="proxyType"></param>
        /// <returns></returns>

        public static ThreadSafeProxyBuilder<object> ConfigureThreadSafeProxy(this object obj, Type proxyType)
        {
            return new ThreadSafeProxyBuilder<object>(obj, proxyType);
        }

    }
}
