using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.osgi.framework
{
    public interface BundleContext : BundleReference
    {
        String getProperty(String key);
        Bundle installBundle(String location, Stream input);
        Bundle installBundle(String location);
        Bundle getBundle(long id);
        Bundle[] getBundles();
        void addServiceListener(ServiceListener listener, String filter);
        void addServiceListener(ServiceListener listener);
        void removeServiceListener(ServiceListener listener);
        void addBundleListener(BundleListener listener);
        void removeBundleListener(BundleListener listener);
        void addFrameworkListener(FrameworkListener listener);
        void removeFrameworkListener(FrameworkListener listener);
        ServiceRegistration registerService(String[] clazzes, Object service,
            IDictionary<String, Object> properties);
        ServiceRegistration registerService(String clazz, Object service,
            IDictionary<String, Object> properties);
        ServiceReference[] getServiceReferences(String clazz, String filter);
        ServiceReference[] getAllServiceReferences(String clazz, String filter);
        ServiceReference getServiceReference(String clazz);
        Object getService(ServiceReference reference);
        Boolean ungetService(ServiceReference reference);
        FileInfo getDataFile(String filename);
        Filter createFilter(String filter);
        Bundle getBundle(String location);
    }
}
