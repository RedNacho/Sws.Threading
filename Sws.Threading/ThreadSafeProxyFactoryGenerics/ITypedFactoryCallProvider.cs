using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{
    /// <summary>
    /// Supplies TypedFactoryCall objects for proxy types.
    /// </summary>
    internal interface ITypedFactoryCallProvider
    {
        /// <summary>
        /// This should always return an instance of TypedFactoryCall with the class generic type parameter set to
        /// proxyType.
        /// </summary>
        /// <param name="proxyType"></param>
        /// <returns></returns>
        TypedFactoryCall GetTypedFactoryCall(Type proxyType);
    }
}
