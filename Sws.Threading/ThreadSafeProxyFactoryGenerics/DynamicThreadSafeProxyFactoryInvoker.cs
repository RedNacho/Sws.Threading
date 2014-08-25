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

        public object CreateProxy(IThreadSafeProxyFactory threadSafeProxyFactory, object obj, Type proxyType, Predicate<MethodInfo> methodIncluder, ILock theLock)
        {
            var typedFactoryCall
                = _typedFactoryCallProvider.GetTypedFactoryCall(proxyType);

            return typedFactoryCall.Invoke(threadSafeProxyFactory, obj, methodIncluder, theLock);
        }

    }

}
