using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.Interception;
using Sws.Threading.ProxyGeneration;

namespace Sws.Threading
{
    public static class StandardImplementations
    {

        private static readonly ProxyGenerator CastleProxyGenerator = new ProxyGenerator();

        public static IThreadSafeProxyFactory CreateThreadSafeProxyFactory()
        {
            return CreateThreadSafeProxyFactory(
                CreateThreadSafeInterceptorFactory(),
                CreateProxyGenerator());
        }

        public static IThreadSafeProxyFactory CreateThreadSafeProxyFactory(
            IThreadSafeInterceptorFactory threadSafeInterceptorFactory, IProxyGenerator proxyGenerator)
        {
            return new ThreadSafeProxyFactory(
                threadSafeInterceptorFactory,
                proxyGenerator);
        }

        public static IThreadSafeInterceptorFactory CreateThreadSafeInterceptorFactory()
        {
            return CreateThreadSafeInterceptorFactory(CreateLockControllerFactory());
        }

        public static IThreadSafeInterceptorFactory CreateThreadSafeInterceptorFactory(
            Func<ILockController> lockControllerFactory)
        {
            return new ThreadSafeInterceptorWithLockControllerFactory(
                       lockControllerFactory);
        }

        public static IProxyGenerator CreateProxyGenerator()
        {
            return new CompositeProxyGenerator(
                    new InterfaceProxyGenerator(CastleProxyGenerator),
                    new ClassProxyGenerator(CastleProxyGenerator));
        }

        public static Func<ILockController> CreateLockControllerFactory()
        {
            return CreateLockController;
        }

        public static ILockController CreateLockController()
        {
            return new SafeFailingLockController(new UnsafeFailingLockController(null));
        }

        public static Func<object, ILock> CreateLockFactory()
        {
            return CreateSafeFailingLockFactory();
        }

        public static Func<object, ISafeFailingLock> CreateSafeFailingLockFactory()
        {
            return lockingObject => new SafeFailingMonitorLock(lockingObject);
        }

    }
}
