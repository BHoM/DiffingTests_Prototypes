using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Diffing
{
    public partial class Query
    {
        [Test]
        [TestCase(1050.67, 1, 1000)]
        [TestCase(1050.67, 2, 1100)]
        [TestCase(1050.67, 3, 1050)]
        [TestCase(1050.67, 4, 1051)]
        [TestCase(1050.67, 5, 1050.7)]
        [TestCase(123456.123, 7, 123456.1)]
        [TestCase(123456.123, 1, 100000)]
        [TestCase(0.0000000000000000000123456789, 5, 1.2346E-20)]
        [TestCase(0.0000000000000000000123456789, 99, 1.23456789E-20)]
        public void SignificantFigures(double number, int significantFigures, double expected)
        {
            double result = BH.Engine.Base.Query.RoundToSignificantFigures(number, significantFigures);
            Assert.IsTrue(result == expected, $"Returned value was {result} instead of {expected}.");
        }
    }
}
