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
        private readonly Predicate<MethodInfo> _methodIncluder;

        public ThreadSafeInterceptor(ILock theLock, Predicate<MethodInfo> methodIncluder)
        {
            if (theLock == null)
            {
                throw new ArgumentNullException("theLock");
            }

            if (methodIncluder == null)
            {
                throw new ArgumentNullException("methodIncluder");
            }

            _lock = theLock;
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
                    _lock.Enter();
                    lockEntered = true;
                }

                invocation.Proceed();
            }
            finally
            {
                if (lockEntered)
                {
                    _lock.Exit();
                }
            }
        }

    }

}
