using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using HtmlGenerator;

namespace WebLogger.Comparer
{
    public class ListComparer<TLeft, TRight>  
    {
        private IEnumerable<TLeft> Lefts;
        private IEnumerable<TRight> Rights;
        readonly List<Comparer<TLeft, TRight>> Сonditions=new List<Comparer<TLeft, TRight>>();
        public ListComparer(IEnumerable<TLeft> left, IEnumerable<TRight> right)
        {
            Lefts = left;
            Rights = right;
        }

        public bool Check()
        {
            if (Lefts.Count() != Rights.Count())
                throw new IndexOutOfRangeException("Списки неравны");
            if (!Lefts.Any())
                throw new IndexOutOfRangeException("Списки пусты");
            var tempL = Lefts.ToList();
            var tempR = Rights.ToList();
            Dictionary<TLeft, Dictionary<TRight, List<EquelsResult>>> result =
                new Dictionary<TLeft, Dictionary<TRight, List<EquelsResult>>>();
            foreach (var lItem in Lefts)
            {
                Dictionary<TRight, List<EquelsResult>> itemResult = new Dictionary<TRight, List<EquelsResult>>();
                foreach (var rItem in Rights)
                {
                    List<EquelsResult> equelsResults = new List<EquelsResult>();
                    foreach (var condition in Сonditions)
                    {
                        equelsResults.Add(condition.Compare(lItem, rItem));
                    }
                    itemResult.Add(rItem, equelsResults.OrderBy(o => o.LeftName).ToList());
                }
                result.Add(lItem, itemResult);
            }
            Print(result);
          
            return result.Values.All(a => a.Values.Any(v => v.All(r => r.Result)));
        }

        private void Print(Dictionary<TLeft, Dictionary<TRight, List<EquelsResult>>> result)
        {
            var d = new HtmlDocument();
            var n = result.Select((r, e) => new {Index = e, r.Value,
                condition =r.Value.First().Value.Select(s => new {s.LeftName, s.LeftValue, s.Condition}).ToList(),
                AllCheck = r.Value.Any(a=>a.Value.All(aa=>aa.Result)) });
            var table = Tag.Table;
            d.Head.AddChild(Tag.Style.WithInnerText("table, th, td {border: 1px solid black;}"+ "tr.good {background-color:green;margin:0;border:0;padding:0;}"+ "tr.bad {background-color:gray;margin:0;border:0;padding:0;}"));
            foreach (var left in n)
            {
                var trLeft = Tag.Tr;
                if (left.AllCheck)
                    trLeft.WithClass("good");
                else
                    trLeft.WithClass("bad");
                trLeft.AddChild(Tag.Td.WithInnerText(left.Index.ToString()));
                trLeft.AddChild(Tag.Td);
                foreach (var c in left.condition)
                {
                    trLeft.AddChild(Tag.Td.WithInnerText(c.LeftValue));
                }
                table.AddChild(trLeft);
                foreach (var right in left.Value.Select((r, e) => new { Index = e,
                                condition = r.Value.Select(s => new {s.RightValue, s.Result}).ToList(),
                                AllCheck = r.Value.All(a => a.Result),
                                AnyCheck = r.Value.Any(a => a.Result) }))
                {
                    var trRight = Tag.Tr;
                    trRight.AddChild(Tag.Td);
                    trRight.AddChild(Tag.Td.WithInnerText(right.Index.ToString()));
                    foreach (var c in right.condition)
                    {
                        trRight.AddChild(Tag.Td.WithInnerText(c.RightValue + " " + c.Result));
                    }
                    if(right.AllCheck)
                        trRight.WithClass("good");
                    else 
                        trRight.WithClass("bad");
                    table.AddChild(trRight);
                }
            }


            d.Body.AddChild(table);
            var k = d.Serialize();
        }

        public void AddCustomComparer(Comparer<TLeft, TRight> condition)
        {
            Сonditions.Add(condition);
        }


        public void AddCondition(Expression<Func<TLeft, TRight, bool>> condition)
        {
            Сonditions.Add(new Comparer<TLeft, TRight>(condition));
        }
    }
}