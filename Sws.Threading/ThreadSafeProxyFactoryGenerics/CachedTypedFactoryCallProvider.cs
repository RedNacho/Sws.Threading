using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{

    internal class CachedTypedFactoryCallProvider : ITypedFactoryCallProvider
    {
        private object _cachedTypedFactoryCallsLockingObject = new object();

        private readonly IDictionary<Type, TypedFactoryCall> _cachedTypedFactoryCalls
            = new Dictionary<Type, TypedFactoryCall>();

        private readonly ITypedFactoryCallProvider _typedFactoryCallProvider;

        public CachedTypedFactoryCallProvider(ITypedFactoryCallProvider typedFactoryCallProvider)
        {
            if (typedFactoryCallProvider == null)
            {
                throw new ArgumentNullException("typedFactoryCallProvider");
            }

            _typedFactoryCallProvider = typedFactoryCallProvider;
        }

        public TypedFactoryCall GetTypedThreadSafeProxyFactory(Type proxyType)
        {
            TypedFactoryCall typedFactoryCall;

            lock (_cachedTypedFactoryCallsLockingObject)
            {

                if (!_cachedTypedFactoryCalls.TryGetValue(proxyType, out typedFactoryCall))
                {
                    typedFactoryCall = _typedFactoryCallProvider.GetTypedThreadSafeProxyFactory(proxyType);
                    _cachedTypedFactoryCalls[proxyType] = typedFactoryCall;
                }

            }

            return typedFactoryCall;
        }

    }

}
