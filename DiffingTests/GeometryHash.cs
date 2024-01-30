/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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
    public class GeometryHash : BH.oM.Test.NUnit.NUnitTest
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
            hash.Should().NotBeNull();

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

        /// <summary>
        /// Tests whether the GeometryHashes for the same mesh are different when including/not including a type of Point in the TypeExceptions.
        /// </summary>
        /// <param name="t"></param>
        [TestCase(typeof(Mesh))]
        [TestCase(typeof(Mesh3D))]
        public void MeshTopology(Type t)
        {
            IGeometry mesh1 = (IGeometry)BH.Engine.Base.Create.RandomObject(t);

            var mesh1Hash = mesh1.GeometryHash();
            mesh1Hash.Should().NotBeNull();

            Console.WriteLine(mesh1Hash);

            ComparisonConfig cc = new() { TypeExceptions = new() { typeof(BH.oM.Geometry.Point) } };
            var meshHashWithPointException = mesh1.GeometryHash(cc);

            mesh1Hash.Should().NotBe(meshHashWithPointException);

            // Also make sure that the Geometry Hash for another completely different mesh
            // is equal to the GeometryHash for the first mesh 
            // when both Point and Face are specified in the TypeExceptions.
            IGeometry mesh2 = (IGeometry)BH.Engine.Base.Create.RandomObject(t);
            ComparisonConfig cc2 = new() { TypeExceptions = new() { typeof(BH.oM.Geometry.Point), typeof(BH.oM.Geometry.Face) } };
            var mesh1HashWithPointAndFaceExceptions = mesh1.GeometryHash(cc2);
            var mesh2HashWithPointAndFaceExceptions = mesh2.GeometryHash(cc2);

            mesh1HashWithPointAndFaceExceptions.Should().Be(mesh2HashWithPointAndFaceExceptions);
        }
    }
}
