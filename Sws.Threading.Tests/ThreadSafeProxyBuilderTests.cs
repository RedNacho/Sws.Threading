using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using Sws.Threading;

namespace Sws.Threading.Tests
{
    [TestClass]
    public class ThreadSafeProxyBuilderTests
    {

        public interface ITest
        {
            int SomeAction(int parameter);
            bool SomeProperty { get; set; }
        }

        public abstract class Test
        {
            public abstract int SomeAction(int parameter);
        }

        private Func<object, ILock> CreateLockFactory()
        {
            return obj => Mock.Of<ILock>();
        }

        private ThreadSafeProxyBuilder<TProxy> CreateThreadSafeProxyBuilder<TProxy>(TProxy subject, Func<object, ILock> lockFactory) where TProxy : class
        {
            return new ThreadSafeProxyBuilder<TProxy>(subject).WithLockFactory(lockFactory);
        }

        [TestMethod]
        public void ThreadSafeProxyWrapsInterfaceMethod()
        {
            const int testParameter = 12345;
            const int testResponse = 67890;

            var testMock = new Mock<ITest>();

            var someActionCount = 0;

            testMock.Setup(test => test.SomeAction(testParameter)).Callback(() => someActionCount++).Returns(testResponse);

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, CreateLockFactory());

            var proxy = proxyBuilder.Build();

            var response = proxy.SomeAction(testParameter);

            someActionCount.Should().Be(1);
            response.Should().Be(testResponse);
        }

        [TestMethod]
        public void ThreadSafeProxyWrapsClassMethod()
        {
            const int testParameter = 12345;
            
            var testMock = new Mock<Test>();

            var someActionCount = 0;

            testMock.Setup(test => test.SomeAction(testParameter)).Callback(() => someActionCount++);

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, CreateLockFactory());

            var proxy = proxyBuilder.Build();

            proxy.SomeAction(testParameter);

            someActionCount.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMethodCalled()
        {
            const int testParameter = 12345;
            
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeAction(testParameter)).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.SomeAction(testParameter);

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyUsesSuppliedLockingObjectToBuildLock()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeAction(testParameter)).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            var lockingObject = new object();

            Func<object, ILock> lockFactory = obj =>
            {
                if (obj != lockingObject)
                {
                    return null;
                }

                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.WithLockingObject(lockingObject).Build();

            proxy.SomeAction(testParameter);

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyOutOfLockWhenInterfaceMethodExits()
        {
            const int testParameter = 12345;
            
            var testMock = new Mock<ITest>();

            int lockEntryCount = 0;

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.SomeAction(testParameter);

            lockEntryCount.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyOutOfLockWhenInterfaceMethodThrowsException()
        {
            const int testParameter = 12345;
            
            var testMock = new Mock<ITest>();

            testMock.Setup(test => test.SomeAction(testParameter)).Throws(
                new Exception()
            );

            int lockEntryCount = 0;

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(test => test.SomeAction(testParameter)).ShouldThrow<Exception>();

            lockEntryCount.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockWhenInterfaceMemberSpecifiedInNotForMemberLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.NotForMember(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberNotSpecifiedInNotForMemberLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.NotForMembers().Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberSpecifiedInLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForMember(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockWhenInterfaceMemberNotSpecifiedInLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.Setup(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };


            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForMembers().Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

    }
}
