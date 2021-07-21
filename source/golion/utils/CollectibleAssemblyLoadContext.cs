using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace golion.utils
{
    public class CollectibleAssemblyLoadContext : AssemblyLoadContext
    {
        public CollectibleAssemblyLoadContext() : base(true) { }
        public CollectibleAssemblyLoadContext(string name) : base(name, true) { }
    }
}
