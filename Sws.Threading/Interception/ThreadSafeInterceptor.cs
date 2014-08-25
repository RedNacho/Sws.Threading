using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Castle.DynamicProxy;

namespace Sws.Threading.Interception
{

    public class ThreadSafeInterceptor : IInterceptor
    {

        private readonly ILock _lock;
        private readonly ILockController _lockController;
        private readonly Predicate<MethodInfo> _methodIncluder;

        [Obsolete("This constructor has been deprecated, please use the overload which accepts an ILockController.")]
        public ThreadSafeInterceptor(ILock theLock, Predicate<MethodInfo> methodIncluder)
            : this(theLock, methodIncluder, new SafeFailingLockController(new UnsafeFailingLockController(null)))
        {
        }

        public ThreadSafeInterceptor(ILock theLock, Predicate<MethodInfo> methodIncluder, ILockController lockController)
        {
            if (theLock == null)
            {
                throw new ArgumentNullException("theLock");
            }

            if (methodIncluder == null)
            {
                throw new ArgumentNullException("methodIncluder");
            }

            if (lockController == null)
            {
                throw new ArgumentNullException("lockController");
            }

            _lock = theLock;
            _lockController = lockController;
            _methodIncluder = methodIncluder;
        }
        
        public void Intercept(IInvocation invocation)
        {
            bool lockEntered = false;

            try
            {
                var enterLock = _methodIncluder(invocation.Method);

                if (enterLock)
                {
                    _lockController.Enter(_lock, ref lockEntered);

                    if (!lockEntered)
                    {
                        throw new LockFailureException(ExceptionMessages.LockFailure);
                    }
                }

                invocation.Proceed();
            }
            finally
            {
                if (lockEntered)
                {
                    _lockController.Exit(_lock);
                }
            }
        }

    }

}
