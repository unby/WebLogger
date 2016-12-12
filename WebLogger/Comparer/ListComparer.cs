using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HtmlGenerator;

namespace WebLogger.Comparer
{
    public class ListComparer<TLeft, TRight>  
    {
        private readonly IEnumerable<TLeft> _lefts;
        private readonly IEnumerable<TRight> _rights;
        readonly List<Comparer<TLeft, TRight>> _сonditions=new List<Comparer<TLeft, TRight>>();
        public ListComparer(IEnumerable<TLeft> left, IEnumerable<TRight> right)
        {
            _lefts = left;
            _rights = right;
        }

        public bool Check()
        {
            if (_lefts.Count() != _rights.Count())
                throw new IndexOutOfRangeException("Списки неравны");
            if (!_lefts.Any())
                throw new IndexOutOfRangeException("Списки пусты");
            var result = new Dictionary<TLeft, Dictionary<TRight, List<EquelsResult>>>();
            foreach (var lItem in _lefts)
            {
                Dictionary<TRight, List<EquelsResult>> itemResult = new Dictionary<TRight, List<EquelsResult>>();
                foreach (var rItem in _rights)
                {
                    List<EquelsResult> equelsResults = new List<EquelsResult>();
                    foreach (var condition in _сonditions)
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
                trLeft.WithClass(left.AllCheck ? "good" : "bad");
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
                    trRight.WithClass(right.AllCheck ? "good" : "bad");
                    table.AddChild(trRight);
                }
            }

            d.Body.AddChild(table);
            var k = d.Serialize();
        }

        public void AddCustomComparer(Comparer<TLeft, TRight> condition)
        {
            _сonditions.Add(condition);
        }

        public void AddCondition(Expression<Func<TLeft, TRight, bool>> condition)
        {
            _сonditions.Add(new Comparer<TLeft, TRight>(condition));
        }
    }
}