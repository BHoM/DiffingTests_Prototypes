using BH.oM;
using BH.oM.Base;
using BH.oM.Dimensional;
using BH.oM.Geometry;
using BH.oM.Structure.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Diffing.Test
{
    public class TestObject : BHoMObject
    {
        public IElement0D Location { get; set; }
    }

    public class TestLocation : BHoMObject, IElement0D
    {
        public Point Position { get; set; }
    }
}
