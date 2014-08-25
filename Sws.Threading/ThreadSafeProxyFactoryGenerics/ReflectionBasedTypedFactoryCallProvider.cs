using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{
    internal class ReflectionBasedTypedFactoryCallProvider : ITypedFactoryCallProvider
    {
        private static Type GenericTypedFactoryCallType = typeof(TypedFactoryCall<>);

        public TypedFactoryCall GetTypedThreadSafeProxyFactory(Type proxyType)
        {
            var typedFactoryCallType = GenericTypedFactoryCallType.MakeGenericType(proxyType);
            return Activator.CreateInstance(typedFactoryCallType) as TypedFactoryCall;
        }

    }
}
