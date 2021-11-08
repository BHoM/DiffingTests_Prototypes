using BH.oM;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class TestIdFragment : IFragment
    {
        public object Id { get; set; }
    }

    public class RandomNumberFragment : IFragment
    {
        public object RandomNumber { get; set; } = new Random().Next();
    }
}
