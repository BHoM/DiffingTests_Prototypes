using BH.oM.Base;
using System;

namespace BH.oM.Diffing.Test
{
    public class RandomNumberFragment : IFragment
    {
        public object RandomNumber { get; set; } = new Random().Next();
    }
}
