using golion.launch;
using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace golion
{
    class ServiceRegistrationImpl : ServiceRegistration
    {
        private FrameworkImpl framework;
        private BundleContextImpl bundleContext;
        private ServiceReferenceImpl reference;

        public ServiceRegistrationImpl(FrameworkImpl framework, BundleContextImpl bundleContext, ServiceReferenceImpl reference)
        {
            this.framework = framework;
            this.bundleContext = bundleContext;
            this.reference = reference;
        }

        public ServiceReference getReference()
        {
            return reference;
        }

        public void setProperties(IDictionary<string, object> properties)
        {
            reference.setProperties(properties);
        }

        public void unregister()
        {
            bundleContext.unregisterService(reference);
        }
    }
}
