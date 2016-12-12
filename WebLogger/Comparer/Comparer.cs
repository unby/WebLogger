using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WebLogger.Comparer
{
    public class Comparer<TLeft, TRight>
    {
        public Comparer(Expression<Func<TLeft, TRight, bool>> сondition)
        {
            _сondition = сondition;
        }

        readonly Expression<Func<TLeft, TRight, bool>> _сondition;

        protected string GetValue(Expression member, object obj)
        {
            var path =Regex.Replace(member.ToString(), @"([^)]+?)","").Split('.');
            var subobj = obj;
            var args = GetReverseArguments(member);
            int argsNumber = 0;
            for (int i = 1; i < path.Length; i++)
            {
                MemberInfo mInfo = subobj.GetType().GetField(path[i]) ??
                                   (MemberInfo) subobj.GetType().GetProperty(path[i]);
                if (mInfo == null)
                {
                    mInfo = subobj.GetType().GetMethods().First(p => p.Name == path[i]);
                }

                if (mInfo != null)
                {
                    var propertyInfo = mInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        subobj = propertyInfo.GetValue(subobj);
                        continue;
                    }
                    var fieldInfo = mInfo as FieldInfo;
                    if (fieldInfo != null)
                    {
                        subobj = fieldInfo.GetValue(subobj);
                        continue;
                    }
                    subobj = args[argsNumber].Item1.Invoke(subobj, args[argsNumber].Item2);
                    argsNumber++;
                    continue;
                }
            }
            if(subobj == null)
                return string.Empty;;
            return subobj.ToString();
        }

        protected List<Tuple<MethodInfo, object[]>> GetReverseArguments(Expression member)
        {
            List<Tuple<MethodInfo, object[]>> result = new List<Tuple<MethodInfo, object[]>>();
            if (member is MemberExpression)
                return result;
            MethodCallExpression n = (MethodCallExpression) member;
            while (n != null && n.NodeType == ExpressionType.Call)
            {

                result.Add(new Tuple<MethodInfo, object[]>(n.Method,
                    n.Arguments.Select(c =>
                        (c as ConstantExpression)?.Value).ToArray()));
                if (n.Object != null && n.Object.NodeType != ExpressionType.Call)
                    break;
                n = (MethodCallExpression) n.Object;
            }
            result.Reverse();
            return result;
        }

        protected string GetMemberName(Expression member)
        {
            var expression = member as MemberExpression;
            if (expression != null)
                return expression.Member.Name;
            Expression n = member;
            while (n != null && n.NodeType != ExpressionType.MemberAccess)
            {
                if (n is MethodCallExpression)
                {
                    n = ((MethodCallExpression) n).Object;
                    var memberExpression = n as MemberExpression;
                    if (memberExpression != null)
                    {
                        return memberExpression.Member.Name;
                    }
                }
                else
                {
                   return string.Empty;
                }
            }
            return string.Empty;
        }

        public EquelsResult Compare(TLeft l, TRight r)
        {
            return Compare(l, r, _сondition);
        }

        protected virtual EquelsResult Compare(TLeft l, TRight r, Expression<Func<TLeft, TRight, bool>> condition)
        {
            Expression expression = condition;
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                var body = lambdaExpression.Body;
                var compiledExpression = _сondition.Compile();

                switch (body.NodeType)
                {
                    case ExpressionType.NotEqual:
                        throw new ArgumentException("Not correct type condition " + body.NodeType.ToString());
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Equal:
                        var bodyE = ((BinaryExpression) body);
                        return new EquelsResult(GetMemberName(bodyE.Left), GetValue(bodyE.Left, l),
                            GetMemberName(bodyE.Right), GetValue(bodyE.Right, r), body.NodeType.ToString(),
                            compiledExpression(l, r));
                    case ExpressionType.Call:
                        var bodyC = ((MethodCallExpression) body);
                        var propLeft = bodyC.Object ?? bodyC.Arguments[0];
                        var propRight = bodyC.Object == null ? bodyC.Arguments[1] : bodyC.Arguments[0];
                        return new EquelsResult(GetMemberName(propLeft), GetValue(propLeft, l), GetMemberName(propRight),
                            GetValue(propRight, r), bodyC.Method.Name, compiledExpression(l, r));
                    default:
                        throw new ArgumentException("Expression not found " + body);
                }
            }
            throw new ArgumentException("Expression not found " + expression);
        }
    }
}
