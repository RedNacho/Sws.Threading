using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sws.Threading.Reflection
{
    internal class MethodInfoExtractor
    {

        public IEnumerable<MethodInfo> ExtractMethods(Type declaringType, Expression memberExpression)
        {
            return ExtractMethods(declaringType, ExtractMembers(declaringType, memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractSetters(Type declaringType, Expression memberExpression)
        {
            return ExtractSetters(declaringType, ExtractMembers(declaringType, memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractGetters(Type declaringType, Expression memberExpression)
        {
            return ExtractGetters(declaringType, ExtractMembers(declaringType, memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractMethods(Type declaringType, Predicate<MemberInfo> memberSelector)
        {
            return ExtractMethods(
                declaringType,
                GetMemberInfosForDeclaringType(declaringType, memberSelector).ToArray());
        }

        private IEnumerable<MethodInfo> ExtractGetters(Type declaringType, MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType(declaringType, GetGetters(GetProperties(memberInfos)));
        }

        private IEnumerable<MethodInfo> ExtractSetters(Type declaringType, MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType(declaringType, GetSetters(GetProperties(memberInfos)));
        }

        public IEnumerable<MethodInfo> ExtractMethods(Type declaringType, MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType(declaringType, GetMethods(memberInfos)
                .Union(GetGettersAndSetters(GetProperties(memberInfos)))
                .Distinct());
        }

        private IEnumerable<MethodInfo> FilterMethodsForDeclaringType(Type declaringType, IEnumerable<MethodInfo> methodInfos)
        {
            return GetMethodInfosForDeclaringType(declaringType, methodInfo => methodInfos.Contains(methodInfo));
        }

        private IEnumerable<MethodInfo> GetGettersAndSetters(IEnumerable<PropertyInfo> propertyInfos)
        {
            return GetGetters(propertyInfos).Union(GetSetters(propertyInfos));
        }

        private IEnumerable<MethodInfo> GetGetters(IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(propertyInfo => propertyInfo.GetGetMethod())
                .Where(propertyGetMethod => propertyGetMethod != null);
        }

        private IEnumerable<MethodInfo> GetSetters(IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(propertyInfo => propertyInfo.GetSetMethod())
                .Where(propertySetMethod => propertySetMethod != null);
        }

        private IEnumerable<PropertyInfo> GetProperties(IEnumerable<MemberInfo> memberInfos)
        {
            return memberInfos.Select(memberInfo => memberInfo as PropertyInfo).Where(propertyInfo => propertyInfo != null);
        }

        private IEnumerable<MethodInfo> GetMethods(IEnumerable<MemberInfo> memberInfos)
        {
            return memberInfos.Select(memberInfo => memberInfo as MethodInfo).Where(methodInfo => methodInfo != null);
        }

        private IEnumerable<MemberInfo> ExtractMembers(Type declaringType, Expression expression)
        {
            var visitor = new MemberInfoExpressionVisitor();

            visitor.Visit(expression);

            return GetMemberInfosForDeclaringType(declaringType, memberInfo => visitor.GetVisitedMemberInfos().Contains(memberInfo));
        }

        private class MemberInfoExpressionVisitor : ExpressionVisitor
        {

            private List<MemberInfo> _memberInfos = new List<MemberInfo>();

            protected override Expression VisitMember(MemberExpression node)
            {
                _memberInfos.Add(node.Member);
                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                _memberInfos.Add(node.Method);
                return base.VisitMethodCall(node);
            }

            public MemberInfo[] GetVisitedMemberInfos()
            {
                return _memberInfos.Distinct().ToArray();
            }

        }

        private static IEnumerable<MemberInfo> GetMemberInfosForDeclaringType(Type type, Predicate<MemberInfo> filter)
        {
            return GetMemberInfosForDeclaringType<MemberInfo>(type,
                t => t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), filter);
        }

        private static IEnumerable<MethodInfo> GetMethodInfosForDeclaringType(Type type, Predicate<MemberInfo> filter)
        {
            return GetMemberInfosForDeclaringType<MethodInfo>(type,
                t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), filter);
        }

        private static IEnumerable<TMemberInfo> GetMemberInfosForDeclaringType<TMemberInfo>(Type type,
            Func<Type, TMemberInfo[]> memberExtractor, Predicate<TMemberInfo> filter)
        {
            var visited = new List<Type>();

            var waiting = new Queue<Type>();

            waiting.Enqueue(type);

            while (waiting.Any())
            {
                var nextType = waiting.Dequeue();

                visited.Add(nextType);

                foreach (var memberInfo in memberExtractor(nextType).Where(memberInfo => filter(memberInfo)))
                {
                    yield return memberInfo;
                }

                if (nextType.BaseType != null)
                {
                    if (!visited.Contains(nextType.BaseType))
                    {
                        waiting.Enqueue(nextType.BaseType);
                    }
                }

                foreach (var implementedInterface in nextType.GetInterfaces())
                {
                    if (!visited.Contains(implementedInterface))
                    {
                        waiting.Enqueue(implementedInterface);
                    }
                }

            }
        }

    }
}
