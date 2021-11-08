using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Utilities
{
    public static class LoggingUtils
    {
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Dump(this object obj)
        {
            logger.Log(NLog.LogLevel.Debug, obj.ToString());
            Console.WriteLine(obj.ToString());
        }
    }
}
