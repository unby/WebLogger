using System;
using System.Linq.Expressions;

namespace WebLogger.Comparer
{
    public static class Exten
    {
        public static string GetExpressionString<T>(Expression<Func<T, bool>> exp)
            where T : class
        {
            return exp.Body.ToString();
        }
        public static bool EquelsDouble(this double left, double right)
        {
            return Math.Abs(left - right) < 0.001;
        }
    }
}