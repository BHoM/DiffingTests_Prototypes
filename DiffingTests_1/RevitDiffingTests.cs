using BH.Engine.Base;
using BH.oM.Base;
using BH.oM.Diffing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BH.Tests.Diffing
{
    public  class RevitDiffingTests // not really implemented yet. Problem with assembly loading.
    {
        public  void RevitDiffing_basic()
        {
            // Generate random objects
            int numElements = 10;
            //Type revitType = typeof(BH.oM.Adapters.Revit.Elements.ModelInstance);
            //List<IBHoMObject> pastObjects = Utils.GenerateRandomObjects(revitType, numElements);
            //List<IBHoMObject> followingObjs = Utils.GenerateRandomObjects(revitType, numElements);

            //// Add RevitIds on them
            //int i = 0;
            //pastObjects.ForEach(o => { i++; o.AddFragment(new BH.oM.Adapters.Revit.Parameters.RevitIdentifiers(i.ToString(), i)); });
            //followingObjs.ForEach(o => { i++; o.AddFragment(new BH.oM.Adapters.Revit.Parameters.RevitIdentifiers(i.ToString(), i)); });

            //Diff diff = BH.Engine.Adapters.Revit.Compute.RevitDiffing(pastObjects, followingObjs, null);
        }
    }
}
