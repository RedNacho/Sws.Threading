using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{
    internal class TypedFactoryCall<TProxy> : TypedFactoryCall where TProxy : class
    {
        public override object Invoke(IThreadSafeProxyFactory threadSafeProxyFactory, object obj, Predicate<MethodInfo> methodIncluder, ILock theLock)
        {
            return threadSafeProxyFactory.CreateProxy<TProxy>(obj as TProxy, methodIncluder, theLock);
        }
    }
}
