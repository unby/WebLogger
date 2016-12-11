using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
                    foreach (var condition in Сonditions)
                    {
                        condition.Compare(lItem, rItem);
                    }
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