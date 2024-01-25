using AutoBogus;
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
using BH.Engine.Diffing.Tests;
using BH.Engine.Base;
using BH.Engine.Serialiser;
using FluentAssertions.Execution;

namespace BH.Tests.Diffing
{
    public class GeometryHash
    {
        List<object> m_previousHashes = new();

        [SetUp]
        public void PreviousHashesSetup()
        {
            m_previousHashes = new();
        }

        [TestCase(1)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(double.NaN)]
        [TestCase(BH.oM.Geometry.Tolerance.Distance)]
        [TestCase(BH.oM.Geometry.Tolerance.MacroDistance)]
        [TestCase(BH.oM.Geometry.Tolerance.MicroDistance)]
        [TestCase(BH.oM.Geometry.Tolerance.Angle)]
        public void PointsOnAxesDifferentHash(double valueOnAxis)
        {
            // // Uncomment the following if behaviour on NaN should be to throw exception.
            //if (double.IsNaN(valueOnAxis))
            //{
            //    var act = () => (new BH.oM.Geometry.Point() { X = valueOnAxis, Y = 0, Z = 0 }).GeometryHash();
            //    act.Should().Throw<ArgumentException>();
            //    return;
            //}

            var px = new BH.oM.Geometry.Point() { X = valueOnAxis, Y = 0, Z = 0 };
            var py = new BH.oM.Geometry.Point() { X = 0, Y = valueOnAxis, Z = 0 };
            var pz = new BH.oM.Geometry.Point() { X = 0, Y = 0, Z = valueOnAxis };
            var pxy = new BH.oM.Geometry.Point() { X = valueOnAxis, Y = valueOnAxis, Z = 0 };
            var pyz = new BH.oM.Geometry.Point() { X = 0, Y = valueOnAxis, Z = valueOnAxis };
            var pxz = new BH.oM.Geometry.Point() { X = valueOnAxis, Y = 0, Z = valueOnAxis };
            var pxyz = new BH.oM.Geometry.Point() { X = valueOnAxis, Y = valueOnAxis, Z = valueOnAxis };

            var hx = px.GeometryHash();
            var hy = py.GeometryHash();
            var hz = pz.GeometryHash();
            var hxy = pxy.GeometryHash();
            var hyz = pyz.GeometryHash();
            var hxz = pxz.GeometryHash();
            var hxyz = pxyz.GeometryHash();

            HashSet<string> allHashes = new() { hx, hy, hz, hxy, hyz, hxz, hxyz };

            Console.WriteLine("hx:   " + hx);
            Console.WriteLine("hy:   " + hy);
            Console.WriteLine("hz:   " + hz);
            Console.WriteLine("hxy:  " + hxy);
            Console.WriteLine("hyz:  " + hyz);
            Console.WriteLine("hxz:  " + hxz);
            Console.WriteLine("hxyz: " + hxyz);

            using (new AssertionScope())
            {
                allHashes.Count.Should().Be(7);

                hy.Should().NotBe(hx);
                hz.Should().NotBe(hy);

                hxy.Should().NotBe(hx);
                hxy.Should().NotBe(hy);
                hxy.Should().NotBe(hz);

                hyz.Should().NotBe(hx);
                hyz.Should().NotBe(hy);
                hyz.Should().NotBe(hz);

                hxz.Should().NotBe(hx);
                hxz.Should().NotBe(hy);
                hxz.Should().NotBe(hz);

                hxy.Should().NotBe(hyz);
                hyz.Should().NotBe(hxy);
                hxz.Should().NotBe(hxy);

                hxyz.Should().NotBe(hx);
                hxyz.Should().NotBe(hy);
                hxyz.Should().NotBe(hz);
                hxyz.Should().NotBe(hxy);
                hxyz.Should().NotBe(hyz);
                hxyz.Should().NotBe(hxz);
            };
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

            hash.Should().NotBeNull();
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

            hash.Should().NotBeNull();
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
        public void Vector(double x, double y, double z)
        {
            var v = new BH.oM.Geometry.Vector() { X = x, Y = y, Z = z };

            var hash = v.GeometryHash();

            if ((v.X.HasValue() && v.X != 0) || (v.Y.HasValue() && v.Y != 0) || (v.Z.HasValue() && v.Z != 0))
            {
                hash.Should().NotBeNull();
            }

            m_previousHashes.Should().NotContain(hash);
            m_previousHashes.Add(hash);

            Console.WriteLine(hash);

            // Also verify that the hash for Vector is different that the hash for a point with the same defining coordinate
            var p = new BH.oM.Geometry.Point() { X = x, Y = y, Z = z };
            hash.Should().NotBe(p.GeometryHash());
        }


        [TestCase(1000)]
        [Repeat(10)] // Because this test relies on random data, it needs to be repeated multiple times
        public void Order_RandomPoints(int pointCount, int? shiftCount = null)
        {
            List<Point> points = AutoFaker.Generate<Point>(pointCount);

            Polyline polyline = new Polyline() { ControlPoints = points };

            var hash1 = polyline.GeometryHash();

            List<Point> shiftedPoints = points.Rotate(shiftCount ?? pointCount / 2).ToList(); // rotate the list of points.
            shiftedPoints.Should().Contain(points); // check that both lists still contain the same elements.
            shiftedPoints.Should().NotContainInOrder(points); // check that the two lists have different ordering.

            Polyline polyline2 = new Polyline() { ControlPoints = shiftedPoints };
            var hash2 = polyline2.GeometryHash();

            // Check that the two hashes are different.
            hash1.Should().NotBe(hash2, $"the two list of points have the same hash, but they contain points in a different order (rotation of {shiftCount ?? pointCount / 2} places):\n\t{polyline.ToJson()}\n\t{polyline2.ToJson()}");
        }

        [TestCase]
        public void Order_PointsOnAxes(int shiftCount = 1)
        {
            List<Point> points = new List<Point>() { new Point() { X = 1, Y = 0, Z = 0 }, new Point() { X = 0, Y = 1, Z = 0 }, new Point() { X = 0, Y = 0, Z = 1 } };

            Polyline polyline = new Polyline() { ControlPoints = points };

            var hash1 = polyline.GeometryHash();
            Console.WriteLine(hash1);

            List<Point> shiftedPoints = points.Rotate(shiftCount).ToList(); // rotate the list of points.
            shiftedPoints.Should().Contain(points); // check that both lists still contain the same elements.
            shiftedPoints.Should().NotContainInOrder(points); // check that the two lists have different ordering.

            Polyline polyline2 = new Polyline() { ControlPoints = shiftedPoints };
            var hash2 = polyline2.GeometryHash();
            Console.WriteLine(hash2);

            // Check that the two hashes are different.
            hash1.Should().NotBe(hash2);
        }
    }
}