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

        public IEnumerable<MethodInfo> ExtractMethods<TDeclaring>(Expression memberExpression)
        {
            return ExtractMethods<TDeclaring>(ExtractMembers<TDeclaring>(memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractSetters<TDeclaring>(Expression memberExpression)
        {
            return ExtractSetters<TDeclaring>(ExtractMembers<TDeclaring>(memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractGetters<TDeclaring>(Expression memberExpression)
        {
            return ExtractGetters<TDeclaring>(ExtractMembers<TDeclaring>(memberExpression).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractMethods<TDeclaring>(Predicate<MemberInfo> memberSelector)
        {
            return ExtractMethods<TDeclaring>(
                typeof(TDeclaring)
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(memberInfo => memberSelector(memberInfo))
                .ToArray());
        }

        private IEnumerable<MethodInfo> ExtractGetters<TDeclaring>(MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType<TDeclaring>(GetGetters(GetProperties(memberInfos)));
        }

        private IEnumerable<MethodInfo> ExtractSetters<TDeclaring>(MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType<TDeclaring>(GetSetters(GetProperties(memberInfos)));
        }

        public IEnumerable<MethodInfo> ExtractMethods<TDeclaring>(MemberInfo[] memberInfos)
        {
            return FilterMethodsForDeclaringType<TDeclaring>(GetMethods(memberInfos)
                .Union(GetGettersAndSetters(GetProperties(memberInfos)))
                .Distinct());
        }

        private IEnumerable<MethodInfo> FilterMethodsForDeclaringType<TDeclaring>(IEnumerable<MethodInfo> methodInfos)
        {
            return methodInfos.Where(methodInfo => methodInfo.DeclaringType == typeof(TDeclaring));
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

        private IEnumerable<MemberInfo> ExtractMembers<TDeclaring>(Expression expression)
        {
            var visitor = new MemberInfoExpressionVisitor<TDeclaring>();

            visitor.Visit(expression);

            return visitor.MemberInfos;
        }

        private class MemberInfoExpressionVisitor<TDeclaring> : ExpressionVisitor
        {

            private List<MemberInfo> _memberInfos = new List<MemberInfo>();

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.DeclaringType == typeof(TDeclaring))
                {
                    _memberInfos.Add(node.Member);
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(TDeclaring))
                {
                    _memberInfos.Add(node.Method);
                }

                return base.VisitMethodCall(node);
            }

            public IEnumerable<MemberInfo> MemberInfos
            {
                get { return _memberInfos; }
            }

        }


    }
}
