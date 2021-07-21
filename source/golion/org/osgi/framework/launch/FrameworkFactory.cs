using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework.launch
{
    public interface FrameworkFactory
    {
        Framework newFramework(IDictionary<String, String> configuration);
    }
}
