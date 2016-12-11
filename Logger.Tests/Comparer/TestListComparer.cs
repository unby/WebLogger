using System.Collections.Generic;
using NUnit.Framework;
using WebLogger.Comparer;

namespace Logger.Tests.Comparer
{
    [TestFixture]
    public class TestListComparer
    {
        [Test]
        public void TestMethod()
        {
            List<TestType> l = new List<TestType>()
            {
                new TestType() {LastName = "ss2", Name = "NN2", AmountFlat = 426.5},
                new TestType() {LastName = "gg4", Name = "tr3", AmountFlat = 456.5}
            };
            List<AmountPerson> r = new List<AmountPerson>()
            {
                new AmountPerson() {LastName = "ss2", Name = "NN2", Account = new Account() {Amount = 426.5}},
                new AmountPerson() {LastName = "gg4", Name = "tr3", Account = new Account() {Amount = 456.5}}
            };

            var t = new ListComparer<TestType, AmountPerson>(l, r);
            t.AddCondition((x, z) => x.Name.Trim('%').ToLower().ToUpper() != z.Name.ToUpper());
            //    t.AddCondition((x, z) => x.Name.Equals(z.Name, StringComparison.OrdinalIgnoreCase));
            //    t.AddCondition((x, z) => x.AmountFlat.EquelsDouble(z.Account.Amount));
            Assert.IsTrue(t.Check());
        }
    }
}