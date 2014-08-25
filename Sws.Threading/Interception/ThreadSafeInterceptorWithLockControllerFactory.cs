using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;

namespace Sws.Threading.Interception
{
    public class ThreadSafeInterceptorWithLockControllerFactory : IThreadSafeInterceptorFactory
    {
        private readonly Func<ILockController> _lockControllerFactory;

        public ThreadSafeInterceptorWithLockControllerFactory(Func<ILockController> lockControllerFactory)
        {
            if (lockControllerFactory == null)
            {
                throw new ArgumentNullException("lockControllerFactory");
            }

            _lockControllerFactory = lockControllerFactory;
        }

        public Func<ILockController> LockControllerFactory
        {
            get { return _lockControllerFactory; }
        }

        public IInterceptor CreateInterceptor(ILock theLock, Predicate<MethodInfo> methodIncluder)
        {
            return new ThreadSafeInterceptor(theLock, methodIncluder, _lockControllerFactory());
        }
    }
}
