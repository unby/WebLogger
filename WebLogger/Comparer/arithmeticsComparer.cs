using System;
using System.Linq.Expressions;

namespace WebLogger.Comparer
{
    public class ArithmeticsComparer<TLeft, TRight> : Comparer<TLeft, TRight>
    {
        public ArithmeticsComparer(Expression<Func<TLeft, TRight, bool>> сondition) : base(сondition)
        {

        }

        protected override EquelsResult Compare(TLeft l, TRight r, Expression<Func<TLeft, TRight, bool>> condition)
        {
            Expression expression = condition;
            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                var body = lambdaExpression.Body;
                Func<TLeft, TRight, bool> compiledExpression = condition.Compile();

                switch (body.NodeType)
                {
                    case ExpressionType.NotEqual:
                        throw new ArgumentException("Not correct type condition " + body.NodeType);
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Equal:
                        var bodyE = ((BinaryExpression)body);
                        return new EquelsResult(GetMemberName(bodyE.Left), GetValue(bodyE.Left, l),
                            GetMemberName(bodyE.Right), GetValue(bodyE.Right, r), body.ToString(),
                            compiledExpression(l, r));
                    case ExpressionType.Call:
                        var bodyC = ((MethodCallExpression)body);
                        var propLeft = bodyC.Object ?? bodyC.Arguments[0];
                        var propRight = bodyC.Object == null ? bodyC.Arguments[1] : bodyC.Arguments[0];
                        var t = propRight.GetType();
                        return new EquelsResult(GetMemberName(propLeft), GetValue(propLeft, l), GetMultiMemberName(propRight),
                            GetMultiValue(propRight, r), body.ToString(), compiledExpression(l, r));
                    default:
                        throw new ArgumentException("Expression not found " + body.ToString());
                }
            }
            throw new ArgumentException("Expression not found " + expression.ToString());
        }

        private string GetMultiValue(Expression exp, TRight v)
        {
            string res = "";
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Divide:
                case ExpressionType.Multiply:
                    var bodyA = ((BinaryExpression)exp);
                    res = res + Environment.NewLine +
                          GetMultiValue(bodyA.Left, v);
                    res = res + Environment.NewLine +
                          GetMultiValue(bodyA.Right, v);
                    return res;
                case ExpressionType.Call:
                    var bodyC = ((MethodCallExpression) exp);
                    res = res + Environment.NewLine +
                          GetMultiValue(bodyC.Object ?? bodyC.Arguments[0], v);
                    res = res + Environment.NewLine +
                          GetMultiValue(bodyC.Object == null ? bodyC.Arguments[1] : bodyC.Arguments[0], v);
                    return res;
                case ExpressionType.MemberAccess:
                   return GetValue(exp, v);
                default:
                    return res;
            }
        }

        private string GetMultiMemberName(Expression exp)
        {
            string res = "";
            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Divide:
                case ExpressionType.Multiply:
                    var bodyA = ((BinaryExpression)exp);
                    res = res + Environment.NewLine +
                          GetMultiMemberName(bodyA.Left);
                    res = res + Environment.NewLine +
                          GetMultiMemberName(bodyA.Right);
                    return res;
                case ExpressionType.Call:
                    var bodyC = ((MethodCallExpression)exp);
                    res = res + Environment.NewLine +
                          GetMultiMemberName(bodyC.Object ?? bodyC.Arguments[0]);
                    res = res + Environment.NewLine +
                          GetMultiMemberName(bodyC.Object == null ? bodyC.Arguments[1] : bodyC.Arguments[0]);
                    return res;
                case ExpressionType.MemberAccess:
                    return GetMemberName(exp);
                default:
                    return res;
            }
        }
    }
}