using AutoBogus;
using BH.oM.Diffing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace BH.Tests.Diffing
{
    public class PerformanceTests
    {
        [Test]
        public void ObjectDifferences_StaticProperties_100Color()
        {
            var colors = AutoFaker.Generate<System.Drawing.Color>(100);

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(colors, colors);

            objectDifferences.Differences.Count.Should().Be(0, "the number of propertyDifferences should be 0");
        }


        [Test]
        public void ObjectDifferences_StaticProperties_1KPlanes()
        {
            var planes = AutoFaker.Generate<BH.oM.Geometry.Plane>(1000);

            ObjectDifferences objectDifferences = BH.Engine.Diffing.Query.ObjectDifferences(planes, planes);

            objectDifferences.Differences.Count.Should().Be(0, "the number of propertyDifferences should be 0");
        }
    }
}
