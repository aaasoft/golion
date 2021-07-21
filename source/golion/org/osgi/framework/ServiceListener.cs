using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface ServiceListener
    {
        void serviceChanged(ServiceEvent serviceEvent);
    }
}
