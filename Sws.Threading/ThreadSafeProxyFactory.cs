using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.Interception;
using Sws.Threading.ProxyGeneration;

namespace Sws.Threading
{
    public class ThreadSafeProxyFactory : IThreadSafeProxyFactory
    {

        private readonly IProxyGenerator _proxyGenerator;

        private readonly IThreadSafeInterceptorFactory _threadSafeInterceptorFactory;

        public ThreadSafeProxyFactory(IThreadSafeInterceptorFactory threadSafeInterceptorFactory, IProxyGenerator proxyGenerator)
        {
            if (proxyGenerator == null)
            {
                throw new ArgumentNullException("proxyGenerator");
            }

            if (threadSafeInterceptorFactory == null)
            {
                throw new ArgumentNullException("threadSafeInterceptorFactory");
            }

            _proxyGenerator = proxyGenerator;

            _threadSafeInterceptorFactory = threadSafeInterceptorFactory;
        }

        public IProxyGenerator ProxyGenerator
        {
            get { return _proxyGenerator; }
        }

        public IThreadSafeInterceptorFactory ThreadSafeInterceptorFactory
        {
            get { return _threadSafeInterceptorFactory; }
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
                throw new ArgumentException(string.Format(ExceptionMessages.ObjectNotSupportedByProxyGeneratorFormat, typeof(TProxy).FullName));
            }

            var interceptor = _threadSafeInterceptorFactory.CreateInterceptor(theLock, methodIncluder);

            return _proxyGenerator.Generate(obj, interceptor);
        }
        
    }
}
