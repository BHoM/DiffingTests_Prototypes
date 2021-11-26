using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Adapters.Revit;
using BH.Engine.Base;
using BH.Engine.Diffing.Tests;
using BH.Engine.Adapters.Revit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Linq;
using BH.oM.Adapters.Revit.Parameters;

namespace BH.Tests.Diffing.Revit
{
    public static class Utilities
    {
        public static void SetRevitParameter(this IBHoMObject bhomobject, RevitParameter revitParameter)
        {
            bhomobject = bhomobject.SetRevitParameter(revitParameter.Name, revitParameter.Value);
        }

        [Description("Adds a RevitIdentifier Fragment on the object. " +
            "The RevitIdentifier `PersistentId` and the `ElementId` property get populated with a progressive number starting from 0. E.g. 0,1,2,...")]
        public static void SetProgressiveRevitIdentifier<T>(this List<T> bhomobjects) where T : IBHoMObject
        {
            int i = 0;
            bhomobjects.ForEach(obj =>
            {
                obj.Fragments.AddOrReplace(new RevitIdentifiers(i.ToString(), i));
                i++;
            });
        }
    }
}
