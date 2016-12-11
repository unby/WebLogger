using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
            var path = member.ToString().Split('.');
            var subobj = obj;
            MemberInfo mInfo = null;
            var args = GetReverseArguments(member);
            int argsNumber = 0;
            for (int i = 1; i < path.Length; i++)
            {
                mInfo = subobj.GetType().GetField(path[i]);
                if (mInfo == null)
                    mInfo = subobj.GetType().GetProperty(path[i]);
                if (mInfo == null)
                {
                    var mName = path[i].Substring(0, path[i].IndexOf('('));
                    Type t = subobj.GetType();
                    mInfo = t.GetMethods().First(p => p.Name == mName);
                }

                if (mInfo != null)
                {
                    var propertyInfo = mInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        // subExpression = ((MemberExpression) subExpression).Expression;
                        subobj = propertyInfo.GetValue(subobj);
                        continue;
                    }
                    var fieldInfo = mInfo as FieldInfo;
                    if (fieldInfo != null)
                    {
                        subobj = fieldInfo.GetValue(subobj);
                        //  subExpression = ((MemberExpression)subExpression).Expression;
                        continue;
                    }
                    var methodInfo = mInfo as MethodInfo;
                    if (methodInfo != null)
                    {
                        subobj = args[argsNumber].Item1.Invoke(subobj, args[argsNumber].Item2);
                        argsNumber++;
                        continue;
                    }
                }
            }
            var l = subobj.ToString();
            return l;
        }

        List<Tuple<MethodInfo, object[]>> GetReverseArguments(Expression member)
        {
            List<Tuple<MethodInfo, object[]>> result = new List<Tuple<MethodInfo, object[]>>();
            if (member is MemberExpression)
                return result;
            MethodCallExpression n = (MethodCallExpression) member;
            while (n.NodeType == ExpressionType.Call)
            {

                result.Add(new Tuple<MethodInfo, object[]>(n.Method,
                    n.Arguments.Select(c =>
                        c is ConstantExpression
                            ? ((ConstantExpression) c).Value
                            : null).ToArray()));
                if (n.Object.NodeType != ExpressionType.Call)
                    break;
                n = (MethodCallExpression) n.Object;
            }
            result.Reverse();
            return result;
        }

        string GetMemberName(Expression member)
        {
            if (member is MemberExpression)
                return ((MemberExpression) member).Member.Name;
            Expression n = member;
            while (n.NodeType != ExpressionType.MemberAccess)
            {

                n = ((MethodCallExpression) n).Object;
                if (n is MemberExpression)
                {
                    return ((MemberExpression) n).Member.Name;
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
            Expression propLeft = null;
            Expression propRight = null;
            if (expression is LambdaExpression)
            {
                var body = ((LambdaExpression) expression).Body;
                Func<TLeft, TRight, bool> compiledExpression = _сondition.Compile();

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
                        propLeft = bodyC.Object == null ? bodyC.Arguments[0] : bodyC.Object;
                        propRight = bodyC.Object == null ? bodyC.Arguments[1] : bodyC.Arguments[0];
                        return new EquelsResult(GetMemberName(propLeft), GetValue(propLeft, l), GetMemberName(propRight),
                            GetValue(propRight, r), bodyC.Method.Name, compiledExpression(l, r));
                    default:
                        throw new ArgumentException("Expression not found " + body.ToString());
                }
            }
            else if (expression is MethodCallExpression)
            {


            }

            throw new ArgumentException("Expression not found " + expression.ToString());
        }
    }
}
