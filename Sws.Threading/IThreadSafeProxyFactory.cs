using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sws.Threading
{
    public interface IThreadSafeProxyFactory
    {
        TProxy CreateProxy<TProxy>(TProxy obj, Predicate<MethodInfo> methodIncluder, ILock theLock) where TProxy : class;
    }
}
