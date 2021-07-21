using System;
using System.Collections.Generic;
using System.Text;

namespace golion.utils
{
    public class BundleUtils
    {
        public static Boolean IsAssemblyBelongsFCL(String assemblyName)
        {
            return assemblyName.StartsWith("System.")
                || assemblyName.Equals("mscorlib");
        }
    }
}
