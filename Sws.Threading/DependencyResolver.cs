using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.ThreadSafeProxyFactoryGenerics;
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
            private readonly DynamicThreadSafeProxyFactoryInvoker _dynamicThreadSafeProxyFactoryInvoker;
            private readonly Func<object, ILock> _defaultLockFactory;

            public IThreadSafeProxyFactory DefaultThreadSafeProxyFactory { get { return _defaultThreadSafeProxyFactory; } }
            public MethodInfoExtractor MethodInfoExtractor { get { return _methodInfoExtractor; } }
            public DynamicThreadSafeProxyFactoryInvoker DynamicThreadSafeProxyFactoryInvoker { get { return _dynamicThreadSafeProxyFactoryInvoker; } }
            public Func<object, ILock> DefaultLockFactory { get { return _defaultLockFactory; } }

            public ThreadSafeProxyBuilderDependencies(IThreadSafeProxyFactory defaultThreadSafeProxyFactory, MethodInfoExtractor methodInfoExtractor, DynamicThreadSafeProxyFactoryInvoker dynamicThreadSafeProxyFactoryInvoker, Func<object, ILock> defaultLockFactory)
            {
                if (defaultThreadSafeProxyFactory == null)
                {
                    throw new ArgumentNullException("defaultThreadSafeProxyFactory");
                }

                if (methodInfoExtractor == null)
                {
                    throw new ArgumentNullException("methodInfoExtractor");
                }

                if (dynamicThreadSafeProxyFactoryInvoker == null)
                {
                    throw new ArgumentNullException("dynamicThreadSafeProxyFactoryInvoker");
                }

                if (defaultLockFactory == null)
                {
                    throw new ArgumentNullException("defaultLockFactory");
                }

                _defaultThreadSafeProxyFactory = defaultThreadSafeProxyFactory;
                _methodInfoExtractor = methodInfoExtractor;
                _dynamicThreadSafeProxyFactoryInvoker = dynamicThreadSafeProxyFactoryInvoker;
                _defaultLockFactory = defaultLockFactory;
            }

        }

        public static ThreadSafeProxyBuilderDependencies GetThreadSafeProxyBuilderDependencies()
        {
            return new ThreadSafeProxyBuilderDependencies(
                GetDefaultThreadSafeProxyFactory(),
                GetMethodInfoExtractor(),
                GetDynamicThreadSafeProxyFactoryInvoker(),
                GetDefaultLockFactory()
            );
        }

        private static readonly ProxyGenerator CastleProxyGenerator = new ProxyGenerator();

        private static IThreadSafeProxyFactory GetDefaultThreadSafeProxyFactory()
        {
            return new ThreadSafeProxyFactory(
                new ThreadSafeInterceptorWithLockControllerFactory(
                    () => new SafeFailingLockController(new UnsafeFailingLockController(null))
                ),
                new CompositeProxyGenerator(
                    new InterfaceProxyGenerator(CastleProxyGenerator),
                    new ClassProxyGenerator(CastleProxyGenerator)
                ));
        }

        private static MethodInfoExtractor GetMethodInfoExtractor()
        {
            return new MethodInfoExtractor();
        }

        private static readonly ITypedFactoryCallProvider TypedFactoryCallProvider
            = new CachedTypedFactoryCallProvider(new ReflectionBasedTypedFactoryCallProvider());

        private static DynamicThreadSafeProxyFactoryInvoker GetDynamicThreadSafeProxyFactoryInvoker()
        {
            return new DynamicThreadSafeProxyFactoryInvoker(TypedFactoryCallProvider);
        }

        private static Func<object, ILock> GetDefaultLockFactory()
        {
            return lockingObject => new SafeFailingMonitorLock(lockingObject);
        }

    }
}
