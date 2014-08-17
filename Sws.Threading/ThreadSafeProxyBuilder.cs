using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Sws.Threading.Reflection;

namespace Sws.Threading
{

    public abstract class ForMembersThreadSafeProxyBuilderContextBase<TProxy> where TProxy : class
    {

        private readonly MethodInfoExtractor _methodInfoExtractor;

        internal ForMembersThreadSafeProxyBuilderContextBase(MethodInfoExtractor methodInfoExtractor)
        {
            if (methodInfoExtractor == null)
            {
                throw new ArgumentNullException("methodInfoExtractor");
            }

            _methodInfoExtractor = methodInfoExtractor;
        }

        /// <summary>
        /// Specifies that the TProxy member described by the expression be included in the current context.  In the standard context,
        /// this means it will be made thread-safe.  Immediately following an "Except()" call, it will explicitly not be made thread-safe.
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
        /// Specifies that the TProxy setter described by the expression be included in the current context.  In the standard context,
        /// this means it will be made thread-safe.  Immediately following an "Except()" call, it will explicitly not be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForSetter<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractSetters<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy getter described by the expression be included in the current context.  In the standard context,
        /// this means it will be made thread-safe.  Immediately following an "Except()" call, it will explicitly not be made thread-safe.
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>

        public ThreadSafeProxyBuilder<TProxy> ForGetter<TReturn>(Expression<Func<TProxy, TReturn>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }

            return ForMethods(_methodInfoExtractor.ExtractGetters<TProxy>(memberExpression));
        }

        /// <summary>
        /// Specifies that the TProxy member described by the expression be included in the current context.  In the standard context,
        /// this means it will be made thread-safe.  Immediately following an "Except()" call, it will explicitly not be made thread-safe.
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
        /// Specifies that the TProxy members listed will be included in the current context.  In the standard context,
        /// this means they will be made thread-safe.  Immediately following an "Except()" call, they will explicitly not be made thread-safe.
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
        /// Specifies that proxy members which match the memberSelector predicate be included in the current context.  In the standard context,
        /// this means they will be made thread-safe.  Immediately following an "Except()" call, they will explicitly not be made thread-safe.
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

        protected abstract ThreadSafeProxyBuilder<TProxy> ForMethods(IEnumerable<MethodInfo> methodInfos);
        
    }

    public class SingleUseForMembersThreadSafeProxyBuilderContext<TProxy> : ForMembersThreadSafeProxyBuilderContextBase<TProxy> where TProxy : class
    {

        private readonly Func<IEnumerable<MethodInfo>, ThreadSafeProxyBuilder<TProxy>> _forMethodApplier;

        internal SingleUseForMembersThreadSafeProxyBuilderContext(MethodInfoExtractor methodInfoExtractor, Func<IEnumerable<MethodInfo>, ThreadSafeProxyBuilder<TProxy>> forMethodApplier)
            : base(methodInfoExtractor)
        {
            if (forMethodApplier == null)
            {
                throw new ArgumentNullException("forMethodApplier");
            }

            _forMethodApplier = forMethodApplier;
        }

        protected override ThreadSafeProxyBuilder<TProxy> ForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            return _forMethodApplier(methodInfos);
        }

    }

    public class ThreadSafeProxyBuilder<TProxy> : ForMembersThreadSafeProxyBuilderContextBase<TProxy> where TProxy : class
    {

        private readonly TProxy _subject;

        private Func<object, ILock> _lockFactory;

        private readonly MethodInfoExtractor _methodInfoExtractor;

        private readonly ThreadSafeProxyFactory _threadSafeProxyFactory;

        private bool _includedMethodInfosSpecified = false;

        private List<MethodInfo> _includedMethodInfos = new List<MethodInfo>();

        private bool _excludedMethodInfosSpecified = false;

        private List<MethodInfo> _excludedMethodInfos = new List<MethodInfo>();

        private object _lockingObject = null;

        public ThreadSafeProxyBuilder(TProxy subject) : this(subject, DependencyResolver.GetThreadSafeProxyBuilderDependencies())
        {
        }

        internal ThreadSafeProxyBuilder(TProxy subject, DependencyResolver.ThreadSafeProxyBuilderDependencies dependencies) : base(dependencies.MethodInfoExtractor)
        {
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }

            _subject = subject;

            _threadSafeProxyFactory = dependencies.ThreadSafeProxyFactory;
            _methodInfoExtractor = dependencies.MethodInfoExtractor;
            _lockFactory = dependencies.DefaultLockFactory;
        }

        public TProxy Subject
        {
            get { return _subject; }
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

        private ThreadSafeProxyBuilder<TProxy> IncludeMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _includedMethodInfosSpecified = true;

            _includedMethodInfos.AddRange(methodInfos);

            return this;
        }

        private ThreadSafeProxyBuilder<TProxy> ExcludeMethods(IEnumerable<MethodInfo> methodInfos)
        {
            _excludedMethodInfosSpecified = true;

            _excludedMethodInfos.AddRange(methodInfos);

            return this;
        }

        protected override ThreadSafeProxyBuilder<TProxy> ForMethods(IEnumerable<MethodInfo> methodInfos)
        {
            return IncludeMethods(methodInfos);
        }

        /// <summary>
        /// Temporarily switches to an "Except" context.  The members specified with the method call immediately following this will not be thread-safed.
        /// </summary>
        /// <returns></returns>

        public SingleUseForMembersThreadSafeProxyBuilderContext<TProxy> Except()
        {
            return new SingleUseForMembersThreadSafeProxyBuilderContext<TProxy>(_methodInfoExtractor, ExcludeMethods);
        }

    }

}
