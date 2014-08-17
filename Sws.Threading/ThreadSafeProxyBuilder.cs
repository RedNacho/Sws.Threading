using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Sws.Threading.Reflection;

namespace Sws.Threading
{
    public class ThreadSafeProxyBuilder<TProxy> where TProxy : class
    {

        private readonly TProxy _subject;

        private Func<object, ILock> _lockFactory;

        private readonly ThreadSafeProxyFactory _threadSafeProxyFactory;

        private readonly MethodInfoExtractor _methodInfoExtractor;

        private bool _includedMethodInfosSpecified = false;

        private List<MethodInfo> _includedMethodInfos = new List<MethodInfo>();

        private bool _excludedMethodInfosSpecified = false;

        private List<MethodInfo> _excludedMethodInfos = new List<MethodInfo>();

        private object _lockingObject = null;

        public ThreadSafeProxyBuilder(TProxy subject)
        {
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }

            _subject = subject;

            var dependencies = DependencyResolver.GetThreadSafeProxyBuilderDependencies();

            _threadSafeProxyFactory = dependencies.ThreadSafeProxyFactory;
            _methodInfoExtractor = dependencies.MethodInfoExtractor;
            _lockFactory = dependencies.DefaultLockFactory;
        }

        public TProxy Subject
        {
            get { return _subject; }
        }
        
        public ThreadSafeProxyBuilder<TProxy> ForMember(Expression<Action<TProxy>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        public ThreadSafeProxyBuilder<TProxy> ForMember<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        public ThreadSafeProxyBuilder<TProxy> ForMembers(params MemberInfo[] memberInfos)
        {
            if (memberInfos == null)
            {
                throw new ArgumentNullException("memberInfos");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberInfos));
        }

        public ThreadSafeProxyBuilder<TProxy> NotForMember(Expression<Action<TProxy>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return NotForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        public ThreadSafeProxyBuilder<TProxy> NotForMember<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return NotForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        public ThreadSafeProxyBuilder<TProxy> NotForMembers(params MemberInfo[] memberInfos)
        {
            if (memberInfos == null)
            {
                throw new ArgumentNullException("memberInfos");
            }

            return NotForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberInfos));
        }

        private ThreadSafeProxyBuilder<TProxy> ForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _includedMethodInfosSpecified = true;

            _includedMethodInfos.AddRange(methodInfos);

            return this;
        }

        private ThreadSafeProxyBuilder<TProxy> NotForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _excludedMethodInfosSpecified = true;

            _excludedMethodInfos.AddRange(methodInfos);

            return this;
        }

        public ThreadSafeProxyBuilder<TProxy> WithLockingObject(object lockingObject)
        {
            if (lockingObject == null)
            {
                throw new ArgumentNullException("lockingObject");
            }

            _lockingObject = lockingObject;

            return this;
        }

        public ThreadSafeProxyBuilder<TProxy> WithLockFactory(Func<object, ILock> lockFactory)
        {
            if (lockFactory == null)
            {
                throw new ArgumentNullException("lockFactory");
            }

            _lockFactory = lockFactory;

            return this;
        }

        public TProxy Build()
        {
            Predicate<MethodInfo> methodInfoIncluder = methodInfo => true;
            Predicate<MethodInfo> methodInfoExcluder = methodInfo => false;

            if (_includedMethodInfosSpecified)
            {
                methodInfoIncluder = methodInfo => _includedMethodInfos.Contains(methodInfo);
            }

            if (_excludedMethodInfosSpecified)
            {
                methodInfoExcluder = methodInfo => _excludedMethodInfos.Contains(methodInfo);
            }

            return _threadSafeProxyFactory.CreateProxy(_subject, methodInfo => methodInfoIncluder(methodInfo) && (!methodInfoExcluder(methodInfo)), _lockFactory(_lockingObject ?? new object()));
        }

    }
}
