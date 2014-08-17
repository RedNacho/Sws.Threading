using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using Sws.Threading.ProxyGeneration;
using Sws.Threading.Reflection;

namespace Sws.Threading
{
    internal static class DependencyResolver // TODO: Chuck in DI container if compositional code starts to get out of hand.
    {
        internal class ThreadSafeProxyBuilderDependencies
        {
            private readonly ThreadSafeProxyFactory _threadSafeProxyFactory;
            private readonly MethodInfoExtractor _methodInfoExtractor;
            private readonly Func<object, ILock> _defaultLockFactory;

            public ThreadSafeProxyFactory ThreadSafeProxyFactory { get { return _threadSafeProxyFactory; } }
            public MethodInfoExtractor MethodInfoExtractor { get { return _methodInfoExtractor; } }
            public Func<object, ILock> DefaultLockFactory { get { return _defaultLockFactory; } }

            public ThreadSafeProxyBuilderDependencies(ThreadSafeProxyFactory threadSafeProxyFactory, MethodInfoExtractor methodInfoExtractor, Func<object, ILock> defaultLockFactory)
            {
                if (threadSafeProxyFactory == null)
                {
                    throw new ArgumentNullException("threadSafeProxyFactory");
                }

                if (methodInfoExtractor == null)
                {
                    throw new ArgumentNullException("methodInfoExtractor");
                }

                if (defaultLockFactory == null)
                {
                    throw new ArgumentNullException("defaultLockFactory");
                }

                _threadSafeProxyFactory = threadSafeProxyFactory;
                _methodInfoExtractor = methodInfoExtractor;
                _defaultLockFactory = defaultLockFactory;
            }

        }

        public static ThreadSafeProxyBuilderDependencies GetThreadSafeProxyBuilderDependencies()
        {
            return new ThreadSafeProxyBuilderDependencies(
                GetThreadSafeProxyFactory(),
                GetMethodInfoExtractor(),
                GetDefaultLockFactory()
            );
        }

        private static ThreadSafeProxyFactory GetThreadSafeProxyFactory()
        {
            var castleProxyGenerator = new ProxyGenerator();

            return new ThreadSafeProxyFactory(new CompositeProxyGenerator(
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
            return lockingObject => new MonitorLock(lockingObject);
        }

    }
}
