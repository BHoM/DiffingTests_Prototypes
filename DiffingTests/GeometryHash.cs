using BH.Engine.Geometry;
using BH.oM.Base;
using BH.oM.Geometry;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace BH.Tests.Diffing
{
    public class GeometryHash
    {
        List<double> m_previousHashes = new();

        [SetUp]
        public void PreviousHashesSetup()
        {
            m_previousHashes = new();
        }

        [TestCase(1)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.NaN)]
        public void PointsOnAxesDifferentHash(double valueOnAxis)
        {
            var px = new BH.oM.Geometry.Point() { X = valueOnAxis, Y = 0, Z = 0 };
            var py = new BH.oM.Geometry.Point() { X = 0, Y = valueOnAxis, Z = 0 };
            var pz = new BH.oM.Geometry.Point() { X = 0, Y = 0, Z = valueOnAxis };

            var hx = px.GeometryHash();
            var hy = py.GeometryHash();
            var hz = pz.GeometryHash();

            Console.WriteLine(hx);
            Console.WriteLine(hy);
            Console.WriteLine(hz);

            hx.Should().NotBe(hy);
            hy.Should().NotBe(hz);
        }

        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(double.PositiveInfinity, 0, 0)]
        [TestCase(double.NegativeInfinity, 0, 0)]
        [TestCase(double.NaN, 0, 0)]
        public void Point(double x, double y, double z)
        {
            var p = new BH.oM.Geometry.Point() { X = x, Y = y, Z = z };

            var hash = p.GeometryHash();

            hash.HasValue().Should().BeTrue();
            m_previousHashes.Should().NotContain(hash);
            m_previousHashes.Add(hash);

            Console.WriteLine(hash);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.NaN)]
        public void Circle(double radius, Point centre = null!)
        {
            if (centre == null)
                centre = new();

            var c = new BH.oM.Geometry.Circle() { Radius = radius };

            var hash = c.GeometryHash();

            hash.HasValue().Should().BeTrue();
            m_previousHashes.Should().NotContain(hash);
            m_previousHashes.Add(hash);

            Console.WriteLine(hash);
        }

        [TestCase(0, 0, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(double.PositiveInfinity, 0, 0)]
        [TestCase(double.NegativeInfinity, 0, 0)]
        [TestCase(double.NaN, 0, 0)]
        public void Vector(double x, double y, double z)
        {
            var p = new BH.oM.Geometry.Vector() { X = x, Y = y, Z = z };

            var hash = p.GeometryHash();

            hash.HasValue().Should().BeTrue();
            m_previousHashes.Should().NotContain(hash);
            m_previousHashes.Add(hash);

            Console.WriteLine(hash);
        }
    }
}
