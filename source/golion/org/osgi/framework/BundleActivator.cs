using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface BundleActivator
    {
        void start(BundleContext context);
        void stop(BundleContext context);
    }
}
