using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logger.Tests
{
    public class EquelsResult
    {
        public EquelsResult(string leftName, string leftValue, string rightName, string rigtValue, string condition,
            bool result)
        {
            LeftName = leftName;
            LeftValue = leftValue;
            RightName = rightName;
            RigtValue = rigtValue;
            Condition = condition;
            Result = result;
            Console.WriteLine(ToString());
        }

        public string LeftName { get; set; }
        public string LeftValue { get; set; }
        public string RightName { get; set; }
        public string RigtValue { get; set; }
        public string Condition { get; set; }
        public bool Result { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} '{1}' {5}{2} {3} '{4}'", LeftName, LeftValue, Condition, RightName, RigtValue,
                Result ? "" : "!");
        }
    }

    public class Comparer<TLeft, TRight>
    {
        public Comparer(Expression<Func<TLeft, TRight, bool>> сondition)
        {
            _сondition = сondition;
        }

        readonly Expression<Func<TLeft, TRight, bool>> _сondition;
        private string _getValue(Expression expression,object obj)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object,object>>(objectMember);

            var getter = getterLambda.Compile()(obj);

            return getter.ToString();
        }
        protected virtual string GetValue(Expression member, object obj)
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
                    Type t= subobj.GetType();
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
                if(n.Object.NodeType!=ExpressionType.Call)
                    break;
                n = (MethodCallExpression)n.Object;
            }
            result.Reverse();
            return result;
        }
        string GetMemberName(Expression member)
        {
            List<Tuple<MethodInfo, object[]>> result = new List<Tuple<MethodInfo, object[]>>();
            if (member is MemberExpression)
                return ((MemberExpression)member).Member.Name;
            Expression n =member;
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
            Expression expression = _сondition;
            MemberExpression propLeft = null;
            MemberExpression propRight = null;
            if (expression is LambdaExpression)
            {
                var body = ((LambdaExpression)expression).Body;
               // Console.WriteLine(body.ToString());
                Func<TLeft, TRight, bool> compiledExpression = _сondition.Compile();
                
                switch (body.NodeType)
                {
                    case ExpressionType.Equal:
                        var bodyE = ((BinaryExpression)body);
                        //var propLeftValue = GetValue(bodyE.Left, l);
                   //     propLeft = ((BinaryExpression)body).Left;
                    //    propRight = (MemberExpression) ((BinaryExpression)body).Right;
                        return new EquelsResult(GetMemberName(bodyE.Left), GetValue(bodyE.Left, l),
                            GetMemberName(bodyE.Right), GetValue(bodyE.Right, r), GetMemberName(bodyE.Right), compiledExpression(l, r));
                    case ExpressionType.Call:
                        var bodyC = ((MethodCallExpression)body);
                        propLeft = (MemberExpression)bodyC.Arguments[0];
                        propRight = (MemberExpression)bodyC.Arguments[1];
                        return new EquelsResult(propLeft.Member.Name, GetValue(propLeft, l), propRight.Member.Name, GetValue(propRight, r), bodyC.Method.Name, compiledExpression(l, r));
                    default:
                       throw  new ArgumentException("Expression not found " +body.ToString());
                }
            }
            else if (expression is MethodCallExpression)
            {
              
               
            }

            throw new ArgumentException("Expression not found " + expression.ToString());
        }
    }

    public class ListComparer<TLeft, TRight>  
    {
        private IEnumerable<TLeft> Lefts;
        private IEnumerable<TRight> Rights;
        List<Comparer<TLeft, TRight>> Сonditions=new List<Comparer<TLeft, TRight>>();
        public ListComparer(IEnumerable<TLeft> left, IEnumerable<TRight> right)
        {
            Lefts = left;
            Rights = right;
        }

        public bool Check()
        {
            if(Lefts.Count()!=Rights.Count())
                throw new IndexOutOfRangeException("Списки неравны");
            if(!Lefts.Any())
                throw new IndexOutOfRangeException("Списки пусты");
            var tempL = Lefts.ToList();
            var tempR = Rights.ToList();
            foreach (var lItem in Lefts)
            {
                foreach (var rItem in Rights)
                {
                    if (!Сonditions.All(a => a.Compare(lItem, rItem).Result))
                        continue;
                    tempL.Remove(lItem);
                    tempR.Remove(rItem);
                }
            }
            if (!tempL.Any() && !tempR.Any())
                return true;

            return false;
        }

        public void AddCondition(Expression<Func<TLeft, TRight, bool>> condition)
        {
            Сonditions.Add(new Comparer<TLeft, TRight>(condition));
        }

    }

    public class PersonFlat
    {
        public string Name { get; set; }
        public string LastName;
        public double AmountFlat { get; set; }
        public int AccountNumber;
    }
    public class Account
    {
        public double Amount;
        public int AccountNumber { get; set; }
    }
    public class AmounPerson
    {
        public string Name;
        public string LastName { get; set; }
        public Account Account { get;set; }
    }
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

    [TestFixture]
    public class TestListComparer
    {
        [Test]
        public void TestMethod()
        {
            List<PersonFlat> l = new List<PersonFlat>()
            {
                new PersonFlat() {LastName = "ss2", Name = "NN2", AmountFlat = 426.5},
                new PersonFlat() {LastName = "gg4", Name = "tr3", AmountFlat = 456.5}
            };
            List<AmounPerson> r = new List<AmounPerson>()
            {
                new AmounPerson() {LastName = "ss2", Name = "NN2", Account = new Account() {Amount = 426.5}},
                new AmounPerson() {LastName = "gg4", Name = "tr3", Account = new Account() {Amount = 456.5}}
            };

            var t = new ListComparer<PersonFlat, AmounPerson>(l, r);
            t.AddCondition((x, z) => x.Name.Trim('%').ToLower().ToUpper() == z.Name.ToUpper());
             t.AddCondition((x, z) => x.AmountFlat.EquelsDouble(z.Account.Amount));
            Assert.IsTrue(t.Check());
        }
    }
}
