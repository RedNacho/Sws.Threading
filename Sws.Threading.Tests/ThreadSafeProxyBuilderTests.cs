using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        public void ThreadSafeProxyNotInLockWhenInterfaceMemberSpecifiedInExceptForMemberLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForMember(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberNotSpecifiedInExceptForMemberLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForMembers().Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberSpecifiedInLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

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

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

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

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberSpecifiedByForMembersPredicate()
        {

            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForMembers(memberInfo => memberInfo.Name == "SomeProperty").Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockWhenInterfaceMemberSpecifiedByExceptForMembersPredicate()
        {

            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForMembers(memberInfo => memberInfo.Name == "SomeProperty").Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockWhenInterfaceMemberNotSpecifiedByForMembersPredicate()
        {

            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForMembers(memberInfo => memberInfo.Name == "SomeOtherProperty").Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockWhenInterfaceMemberNotSpecifiedByExceptForMembersPredicate()
        {

            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForMembers(memberInfo => memberInfo.Name == "SomeOtherProperty").Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyRetainsCorrectBehaviourIfBuilderModifiedSubsequently()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy1 = proxyBuilder.ForMembers(memberInfo => memberInfo.Name == "SomeOtherProperty").Build();

            var proxy2 = proxyBuilder.ForMembers(memberInfo => memberInfo.Name == "SomeProperty").Build();

            var propertyValue = proxy1.SomeProperty;

            var lockEntryCountDuringCallback1 = lockEntryCountDuringCallback;

            propertyValue = proxy2.SomeProperty;

            var lockEntryCountDuringCallback2 = lockEntryCountDuringCallback;

            lockEntryCountDuringCallback1.Should().Be(0);

            lockEntryCountDuringCallback2.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockOnReadWhenInterfaceMemberSpecifiedInForSetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForSetter(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockOnReadWhenInterfaceMemberSpecifiedInForGetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForGetter(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockOnWriteWhenInterfaceMemberSpecifiedInForGetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupSet(test => test.SomeProperty = It.IsAny<bool>()).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForGetter(test => test.SomeProperty).Build();

            proxy.SomeProperty = true;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockOnWriteWhenInterfaceMemberSpecifiedInForSetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupSet(test => test.SomeProperty = It.IsAny<bool>()).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForSetter(test => test.SomeProperty).Build();

            proxy.SomeProperty = true;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockOnReadWhenInterfaceMemberSpecifiedInExceptForSetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForSetter(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockOnReadWhenInterfaceMemberSpecifiedInExceptForGetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupGet(test => test.SomeProperty).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForGetter(test => test.SomeProperty).Build();

            var propertyValue = proxy.SomeProperty;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyInLockOnWriteWhenInterfaceMemberSpecifiedInExceptForGetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupSet(test => test.SomeProperty = It.IsAny<bool>()).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForGetter(test => test.SomeProperty).Build();

            proxy.SomeProperty = true;

            lockEntryCountDuringCallback.Should().Be(1);
        }

        [TestMethod]
        public void ThreadSafeProxyNotInLockOnWriteWhenInterfaceMemberSpecifiedInExceptForSetterLambdaExpression()
        {
            var testMock = new Mock<ITest>();

            int? lockEntryCountDuringCallback = null;

            int lockEntryCount = 0;

            testMock.SetupSet(test => test.SomeProperty = It.IsAny<bool>()).Callback(() => lockEntryCountDuringCallback = lockEntryCount);

            Func<object, ILock> lockFactory = obj =>
            {
                var lockMock = new Mock<ILock>();

                lockMock.Setup(lck => lck.Enter()).Callback(() => lockEntryCount++);
                lockMock.Setup(lck => lck.Exit()).Callback(() => lockEntryCount--);

                return lockMock.Object;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Except().ForSetter(test => test.SomeProperty).Build();

            proxy.SomeProperty = true;

            lockEntryCountDuringCallback.Should().Be(0);
        }

        [TestMethod]
        public void ThreadSafeProxyCreatedByAlternativeThreadSafeProxyFactoryIfSpecified()
        {
            var threadSafeProxyFactoryMock = new Mock<IThreadSafeProxyFactory>();

            var testMock = new Mock<ITest>();

            var proxyMock = new Mock<ITest>();

            var someProperty = typeof(ITest).GetProperties().Where(propertyInfo => propertyInfo.Name == "SomeProperty").Single();

            var somePropertySetter = someProperty.GetSetMethod();
            var somePropertyGetter = someProperty.GetGetMethod();

            var generatedLocks = new List<ILock>();

            threadSafeProxyFactoryMock.Setup(threadSafeProxyFactory => threadSafeProxyFactory.CreateProxy<ITest>(
                    testMock.Object,
                    It.Is<Predicate<MethodInfo>>(predicate => predicate(somePropertySetter) && (!predicate(somePropertyGetter))),
                    It.Is<ILock>(theLock => generatedLocks.Contains(theLock)))
                ).Returns(proxyMock.Object);

            Func<object, ILock> lockFactory = obj =>
            {
                var theLock = Mock.Of<ILock>();
                generatedLocks.Add(theLock);
                return theLock;
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.ForMembers(somePropertySetter).WithThreadSafeProxyFactory(threadSafeProxyFactoryMock.Object).Build();

            proxy.Should().Be(proxyMock.Object);
        }

        private class SafeFailingLockMock : ISafeFailingLock
        {

            private readonly bool _setLockTaken;
            private readonly Action _enterLock;
            private readonly Action _exitLock;

            public SafeFailingLockMock(bool setLockTaken, Action enterLock, Action exitLock)
            {
                _setLockTaken = setLockTaken;
                _enterLock = enterLock;
                _exitLock = exitLock;
            }

            public void Enter(ref bool lockTaken)
            {
                lockTaken = _setLockTaken;
                _enterLock();
            }

            public void Enter()
            {
                _enterLock();
            }

            public void Exit()
            {
                _exitLock();
            }

        }

        [TestMethod]
        public void ThreadSafeProxyThrowsExceptionIfSafeFailingLockDoesNotSetLockTaken()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            Func<object, ILock> lockFactory = obj =>
            {
                return new SafeFailingLockMock(false, () => { }, () => { });
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(p => p.SomeAction(testParameter)).ShouldThrow<LockFailureException>();
        }

        [TestMethod]
        public void ThreadSafeProxyDoesNotUnlockIfSafeFailingLockDoesNotSetLockTaken()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            bool triedToUnlock = false;

            Func<object, ILock> lockFactory = obj =>
            {
                return new SafeFailingLockMock(false, () => { }, () => triedToUnlock = true);
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(p => p.SomeAction(testParameter)).ShouldThrow<LockFailureException>();

            triedToUnlock.Should().BeFalse();
        }

        [TestMethod]
        public void ThreadSafeProxyUnlocksIfSafeFailingLockSetsLockTaken()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            bool triedToUnlock = false;

            Func<object, ILock> lockFactory = obj =>
            {
                return new SafeFailingLockMock(true, () => { }, () => triedToUnlock = true);
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(p => p.SomeAction(testParameter)).ShouldNotThrow<LockFailureException>();

            triedToUnlock.Should().BeTrue();
        }

        [TestMethod]
        public void ThreadSafeProxyUnlocksIfSafeFailingLockSetsLockTakenBeforeThrowingException()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            bool triedToUnlock = false;

            Func<object, ILock> lockFactory = obj =>
            {
                return new SafeFailingLockMock(true, () => { throw new Exception("Problem!"); }, () => triedToUnlock = true);
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(p => p.SomeAction(testParameter)).ShouldThrow<Exception>();

            triedToUnlock.Should().BeTrue();
        }

        [TestMethod]
        public void ThreadSafeProxyDoesNotUnlockIfSafeFailingLockDoesNotSetLockTakenBeforeThrowingException()
        {
            const int testParameter = 12345;

            var testMock = new Mock<ITest>();

            bool triedToUnlock = false;

            Func<object, ILock> lockFactory = obj =>
            {
                return new SafeFailingLockMock(false, () => { throw new Exception("Problem!"); }, () => triedToUnlock = true);
            };

            var proxyBuilder = CreateThreadSafeProxyBuilder(testMock.Object, lockFactory);

            var proxy = proxyBuilder.Build();

            proxy.Invoking(p => p.SomeAction(testParameter)).ShouldThrow<Exception>();

            triedToUnlock.Should().BeFalse();
        }
    }
}
