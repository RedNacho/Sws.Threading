using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.Interception
{
    [Obsolete("ThreadSafeInterceptorFactory has been deprecated; please use ThreadSafeInterceptorWithLockControllerFactory instead.")]
    public class ThreadSafeInterceptorFactory : IThreadSafeInterceptorFactory
    {
        public IInterceptor CreateInterceptor(ILock theLock, Predicate<MethodInfo> methodIncluder)
        {
            return new ThreadSafeInterceptor(theLock, methodIncluder);
        }
    }
}
