using org.osgi.framework.launch;
using System;
using System.Collections.Generic;
using System.Text;

namespace golion.launch
{
    public class FrameworkFactoryImpl : FrameworkFactory
    {
        public Framework newFramework(IDictionary<string, string> configuration)
        {
            return new FrameworkImpl(configuration);
        }
    }
}
