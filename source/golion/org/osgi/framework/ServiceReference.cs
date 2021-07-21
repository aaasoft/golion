using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface ServiceReference : IComparable
    {
        Bundle getBundle();
        Object getProperty(String key);
        String[] getPropertyKeys();
        Bundle[] getUsingBundles();
        Boolean isAssignableTo(Bundle bundle, String className);
    }
}
