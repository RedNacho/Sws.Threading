using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.ProxyGeneration;

namespace Sws.Threading
{
    public class ThreadSafeProxyFactory : IThreadSafeProxyFactory
    {

        private readonly IProxyGenerator _proxyGenerator;

        public ThreadSafeProxyFactory(IProxyGenerator proxyGenerator)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException("proxyGenerator");
            }

            _proxyGenerator = proxyGenerator;
        }

        public IProxyGenerator ProxyGenerator
        {
            get { return _proxyGenerator; }
        }

        public TProxy CreateProxy<TProxy>(TProxy obj, Predicate<MethodInfo> methodIncluder, ILock theLock) where TProxy : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (methodIncluder == null)
            {
                throw new ArgumentNullException("methodIncluder");
            }

            if (theLock == null)
            {
                throw new ArgumentNullException("theLock");
            }

            if (!_proxyGenerator.CanProxy<TProxy>())
            {
                throw new ArgumentException(string.Format("Proxy generator does not support this type of object: {0}", typeof(TProxy).FullName));
            }

            var interceptor = new ThreadSafeInterceptor(theLock, methodIncluder);

            return _proxyGenerator.Generate(obj, interceptor);
        }
        
        private class ThreadSafeInterceptor : IInterceptor
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
}
