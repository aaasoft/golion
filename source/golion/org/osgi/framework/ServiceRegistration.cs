using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface ServiceRegistration
    {
        ServiceReference getReference();
        void setProperties(IDictionary<String, Object> properties);
        void unregister();
    }
}
