using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace golion
{
    class ServiceReferenceImpl : ServiceReference
    {
        private BundleContextImpl bundleContext;
        private String[] clazzes;
        private IDictionary<string, object> properties;
        private Object service;

        public ServiceReferenceImpl(BundleContextImpl bundleContext, String[] clazzes, IDictionary<string, object> properties, Object service)
        {
            this.bundleContext = bundleContext;
            this.clazzes = clazzes;
            this.properties = properties;
            this.service = service;
        }

        public Bundle getBundle()
        {
            return bundleContext.getBundle();
        }

        public object getProperty(string key)
        {
            return properties[key];
        }

        public string[] getPropertyKeys()
        {
            String[] propertyKeys = new String[properties.Count];
            properties.Keys.CopyTo(propertyKeys, 0);
            return propertyKeys;
        }

        internal void setProperties(IDictionary<string, object> properties)
        {
            this.properties = properties;
        }

        internal Object getService()
        {
            return service;
        }

        public Bundle[] getUsingBundles()
        {
            return bundleContext.getUsingBundles(this);
        }

        public bool isAssignableTo(Bundle bundle, string className)
        {
            if (bundleContext.getBundle().CompareTo(bundle) != 0) return false;
            foreach (String clazz in clazzes)
            {
                if (clazz.Equals(className)) return true;
            }
            return false;
        }

        public int CompareTo(object obj)
        {
            return GetHashCode().CompareTo(obj.GetHashCode());
        }
    }
}
