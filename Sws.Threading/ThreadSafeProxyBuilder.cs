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
        
        /// <summary>
        /// Specifies that the TProxy member described by the expression will be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForMember(Expression<Action<TProxy>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy member described by the expression will be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForMember<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy members listed will be made thread-safe.
        /// </summary>
        /// <param name="memberInfos"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForMembers(params MemberInfo[] memberInfos)
        {
            if (memberInfos == null)
            {
                throw new ArgumentNullException("memberInfos");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberInfos));
        }

        /// <summary>
        /// Specifies that proxy members which match the memberSelector predicate will be made thread-safe.
        /// </summary>
        /// <param name="memberSelector"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForMembers(Predicate<MemberInfo> memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException("memberSelector");
            }

            return ForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberSelector));
        }

        /// <summary>
        /// Specifies that the TProxy member described by the expression will not be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ExceptForMember(Expression<Action<TProxy>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ExceptForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy member described by the expression will not be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
         
        public ThreadSafeProxyBuilder<TProxy> ExceptForMember<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ExceptForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy members listed will not be made thread-safe.
        /// </summary>
        /// <param name="memberInfos"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ExceptForMembers(params MemberInfo[] memberInfos)
        {
            if (memberInfos == null)
            {
                throw new ArgumentNullException("memberInfos");
            }

            return ExceptForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberInfos));
        }

        /// <summary>
        /// Specifies that proxy members which match the memberSelector predicate will not be made thread-safe.
        /// </summary>
        /// <param name="memberSelector"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ExceptForMembers(Predicate<MemberInfo> memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException("memberSelector");
            }

            return ExceptForMethods(_methodInfoExtractor.ExtractMethods<TProxy>(memberSelector));
        }

        private ThreadSafeProxyBuilder<TProxy> ForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _includedMethodInfosSpecified = true;

            _includedMethodInfos.AddRange(methodInfos);

            return this;
        }

        private ThreadSafeProxyBuilder<TProxy> ExceptForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _excludedMethodInfosSpecified = true;

            _excludedMethodInfos.AddRange(methodInfos);

            return this;
        }

        /// <summary>
        /// Specifies an object on which to lock.  This will be passed to the lock factory when the proxy is built.
        /// </summary>
        /// <param name="lockingObject"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> WithLockingObject(object lockingObject)
        {
            if (lockingObject == null)
            {
                throw new ArgumentNullException("lockingObject");
            }

            _lockingObject = lockingObject;

            return this;
        }

        /// <summary>
        /// Specifies a lock factory to use when creating thread locks.
        /// </summary>
        /// <param name="lockFactory"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> WithLockFactory(Func<object, ILock> lockFactory)
        {
            if (lockFactory == null)
            {
                throw new ArgumentNullException("lockFactory");
            }

            _lockFactory = lockFactory;

            return this;
        }

        /// <summary>
        /// Builds the proxy.  If no explicit ForMember(s) calls were made, all members will be thread-safe, except any explicitly excluded through
        /// NotForMember(s) calls.  If no WithLockFactory call was made, a default lock factory which uses System.Threading.Monitor (equivalent to the
        /// lock keyword) will be used.  If no WithLockingObject call was made, a dedicated locking object will be newed up with each call.
        /// </summary>
        /// <returns></returns>

        public TProxy Build()
        {
            Predicate<MethodInfo> methodInfoIncluder = methodInfo => true;
            Predicate<MethodInfo> methodInfoExcluder = methodInfo => false;

            if (_includedMethodInfosSpecified)
            {
                var includedMethodInfos = _includedMethodInfos.ToArray();
                methodInfoIncluder = methodInfo => includedMethodInfos.Contains(methodInfo);
            }

            if (_excludedMethodInfosSpecified)
            {
                var excludedMethodInfos = _excludedMethodInfos.ToArray();
                methodInfoExcluder = methodInfo => excludedMethodInfos.Contains(methodInfo);
            }

            return _threadSafeProxyFactory.CreateProxy(_subject, methodInfo => methodInfoIncluder(methodInfo) && (!methodInfoExcluder(methodInfo)), _lockFactory(_lockingObject ?? new object()));
        }

    }
}
