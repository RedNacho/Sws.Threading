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

        public IEnumerable<MethodInfo> ExtractMethods<TDeclaring>(Predicate<MemberInfo> memberSelector)
        {
            return ExtractMethods<TDeclaring>(
                typeof(TDeclaring).GetMembers().Where(memberInfo => memberSelector(memberInfo)).ToArray());
        }

        public IEnumerable<MethodInfo> ExtractMethods<TDeclaring>(MemberInfo[] memberInfos)
        {
            return GetMethods(memberInfos)
                .Union(GetGettersAndSetters(GetProperties(memberInfos)))
                .Where(methodInfo => methodInfo.DeclaringType == typeof(TDeclaring))
                .Distinct();
        }

        public IEnumerable<MethodInfo> GetGettersAndSetters(IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Select(propertyInfo => new[] { propertyInfo.GetGetMethod(), propertyInfo.GetSetMethod() })
                .SelectMany(propertyGetSetMethods => propertyGetSetMethods)
                .Where(propertyGetSetMethod => propertyGetSetMethod != null);
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
