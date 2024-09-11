using AutoBogus;
using BH.oM.Diffing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using BH.oM.Base;
using BH.Engine.Base;

namespace BH.Tests.Diffing
{
    public class PerformanceHash : BH.oM.Test.NUnit.NUnitTest
    {
        [Test]
        public void StaticProperties_100Color()
        {
            CustomObject co = new CustomObject();

            var data = AutoFaker.Generate<System.Drawing.Color>(100);
            co.CustomData["data"] = data;

            string hash = co.Hash();
        }


        //[Test]
        //public void StaticProperties_1KPlanes()
        //{
        //    CustomObject co = new CustomObject();

        //    var data = AutoFaker.Generate<BH.oM.Geometry.Plane>(1000);
        //    co.CustomData["data"] = data;

        //    string hash = co.Hash();
        //}
    }
}
