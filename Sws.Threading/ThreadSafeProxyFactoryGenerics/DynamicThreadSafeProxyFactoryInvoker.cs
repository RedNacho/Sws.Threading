using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{
    internal class DynamicThreadSafeProxyFactoryInvoker
    {
        private readonly ITypedFactoryCallProvider _typedFactoryCallProvider;

        public DynamicThreadSafeProxyFactoryInvoker(ITypedFactoryCallProvider typedFactoryCallProvider)
        {
            if (typedFactoryCallProvider == null)
            {
                throw new ArgumentNullException("typedFactoryCallProvider");
            }

            _typedFactoryCallProvider = typedFactoryCallProvider;
        }

        public TProxy CreateProxy<TProxy>(IThreadSafeProxyFactory threadSafeProxyFactory, TProxy obj, Type proxyType, Predicate<MethodInfo> methodIncluder, ILock theLock)
            where TProxy : class
        {
            if (!typeof(TProxy).IsAssignableFrom(proxyType))
            {
                throw new ArgumentException(ExceptionMessages.TProxyNotAssignableFromProxyType);
            }

            var typedFactoryCall
                = _typedFactoryCallProvider.GetTypedThreadSafeProxyFactory(proxyType);

            return typedFactoryCall.Invoke(threadSafeProxyFactory, obj, methodIncluder, theLock) as TProxy;
        }

    }

}
