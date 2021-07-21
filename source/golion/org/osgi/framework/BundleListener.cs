using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface BundleListener
    {
        void bundleChanged(BundleEvent e);
    }
}
