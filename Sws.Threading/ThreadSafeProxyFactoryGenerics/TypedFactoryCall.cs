using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sws.Threading.ThreadSafeProxyFactoryGenerics
{
    internal abstract class TypedFactoryCall
    {
        public abstract object Invoke(IThreadSafeProxyFactory threadSafeProxyFactory, object obj, Predicate<MethodInfo> methodIncluder, ILock theLock);
    }
}
