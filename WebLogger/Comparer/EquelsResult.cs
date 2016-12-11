using System;

namespace WebLogger.Comparer
{
    public class EquelsResult
    {
        public EquelsResult(string leftName, string leftValue, string rightName, string rigtValue, string condition,
            bool result)
        {
            LeftName = leftName;
            LeftValue = leftValue;
            RightName = rightName;
            RightValue = rigtValue;
            Condition = condition;
            Result = result;
        }

        public string LeftName { get; set; }
        public string LeftValue { get; set; }
        public string RightName { get; set; }
        public string RightValue { get; set; }
        public string Condition { get; set; }
        public bool Result { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} '{1}' {5}{2} {3} '{4}'", LeftName, LeftValue, Condition, RightName, RightValue,
                Result ? "" : "!");
        }
    }
}