using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.Interception;
using Sws.Threading.ProxyGeneration;
using Sws.Threading.Reflection;

namespace Sws.Threading
{
    internal static class DependencyResolver // TODO: Chuck in DI container if compositional code starts to get out of hand.
    {
        internal class ThreadSafeProxyBuilderDependencies
        {
            private readonly IThreadSafeProxyFactory _defaultThreadSafeProxyFactory;
            private readonly MethodInfoExtractor _methodInfoExtractor;
            private readonly Func<object, ILock> _defaultLockFactory;

            public IThreadSafeProxyFactory DefaultThreadSafeProxyFactory { get { return _defaultThreadSafeProxyFactory; } }
            public MethodInfoExtractor MethodInfoExtractor { get { return _methodInfoExtractor; } }
            public Func<object, ILock> DefaultLockFactory { get { return _defaultLockFactory; } }

            public ThreadSafeProxyBuilderDependencies(IThreadSafeProxyFactory defaultThreadSafeProxyFactory, MethodInfoExtractor methodInfoExtractor, Func<object, ILock> defaultLockFactory)
            {
                if (defaultThreadSafeProxyFactory == null)
                {
                    throw new ArgumentNullException("defaultThreadSafeProxyFactory");
                }

                if (methodInfoExtractor == null)
                {
                    throw new ArgumentNullException("methodInfoExtractor");
                }

                if (defaultLockFactory == null)
                {
                    throw new ArgumentNullException("defaultLockFactory");
                }

                _defaultThreadSafeProxyFactory = defaultThreadSafeProxyFactory;
                _methodInfoExtractor = methodInfoExtractor;
                _defaultLockFactory = defaultLockFactory;
            }

        }

        public static ThreadSafeProxyBuilderDependencies GetThreadSafeProxyBuilderDependencies()
        {
            return new ThreadSafeProxyBuilderDependencies(
                GetDefaultThreadSafeProxyFactory(),
                GetMethodInfoExtractor(),
                GetDefaultLockFactory()
            );
        }

        private static IThreadSafeProxyFactory GetDefaultThreadSafeProxyFactory()
        {
            var castleProxyGenerator = new ProxyGenerator();

            return new ThreadSafeProxyFactory(
                new ThreadSafeInterceptorWithLockControllerFactory(
                    () => new SafeFailingLockController(new UnsafeFailingLockController(null))
                ),
                new CompositeProxyGenerator(
                    new InterfaceProxyGenerator(castleProxyGenerator),
                    new ClassProxyGenerator(castleProxyGenerator)
                ));
        }

        private static MethodInfoExtractor GetMethodInfoExtractor()
        {
            return new MethodInfoExtractor();
        }

        private static Func<object, ILock> GetDefaultLockFactory()
        {
            return lockingObject => new SafeFailingMonitorLock(lockingObject);
        }

    }
}
