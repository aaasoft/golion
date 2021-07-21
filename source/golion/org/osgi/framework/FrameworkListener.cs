using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface FrameworkListener
    {
        void frameworkEvent(FrameworkEvent frameworkEvent);
    }
}
