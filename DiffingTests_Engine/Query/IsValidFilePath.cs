using BH.oM.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.Diffing.Tests
{
    public static partial class Query
    {
        public static bool IsValidFilePath(this string filePath, bool allowRelativePaths = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                string fullPath = Path.GetFullPath(filePath);

                if (allowRelativePaths)
                    return Path.IsPathRooted(filePath);
                else
                {
                    string root = Path.GetPathRoot(filePath);
                    return string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                }
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError("Invalid File path.");
                return false;
            }
        }
    }
}
