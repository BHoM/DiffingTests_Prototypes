using BH.Engine.Base;
using BH.oM.Base;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.Engine.Diffing
{
    public partial class Query
    {
        // Test parameters:
        // seenAsDifferent, number1, number2, tolerance
        [TestCase(true, 1.5, 2, double.MinValue)]
        [TestCase(false, 1.5, 2, 2)]
        [TestCase(false, 1.5, 2, 0.5)]
        [TestCase(true, 1.5, 2, 0.49)]
        [TestCase(false, 1.5, 2, 0.51)]
        public void NumericalDifferenceInclusion_Tolerance(bool seenAsDifferent, double number1, double number2, double tolerance)
        {
            var comparisonConfig = new ComparisonConfig()
            {
                NumericTolerance = tolerance,
            };

            BH.Engine.Diffing.Query.NumericalDifferenceInclusion(number1, number2, comparisonConfig: comparisonConfig).ShouldBe(seenAsDifferent);
        }

        // Test parameters:
        // seenAsDifferent, number1, number2, significantFigures
        [TestCase(false, 1.5, 2, 0)]
        [TestCase(false, 1.5, 2, 1)]
        [TestCase(true, 1.5, 2, 2)]
        public void NumericalDifferenceInclusion_SignificantFigures(bool seenAsDifferent, double number1, double number2, int significantFigures)
        {
            var comparisonConfig = new ComparisonConfig()
            {
                SignificantFigures = significantFigures,
            };

            BH.Engine.Diffing.Query.NumericalDifferenceInclusion(number1, number2, comparisonConfig: comparisonConfig).ShouldBe(seenAsDifferent);
        }
    }
}
