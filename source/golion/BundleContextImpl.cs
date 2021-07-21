using golion.launch;
using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace golion
{
    class BundleContextImpl : BundleContext
    {
        private FrameworkImpl framework;
        private Bundle bundle;
        private IList<ServiceListener> serviceListenerList = new List<ServiceListener>();
        private IList<BundleListener> bundleListenerList = new List<BundleListener>();
        private IList<FrameworkListener> frameworkListenerList = new List<FrameworkListener>();
        private IList<ServiceReference> serviceReferenceList = new List<ServiceReference>();
        private IList<ServiceReference> bundleUsingServiceReferenceList = new List<ServiceReference>();

        public BundleContextImpl(FrameworkImpl framework, Bundle bundle)
        {
            this.framework = framework;
            this.bundle = bundle;
        }

        public string getProperty(string key)
        {
            throw new NotImplementedException();
        }

        public Bundle installBundle(string location, System.IO.Stream input)
        {
            return framework.installBundle(location, input);
        }

        public Bundle installBundle(string location)
        {
            return installBundle(location, null);
        }

        public Bundle getBundle(long id)
        {
            return framework.getBundle(id);
        }

        public Bundle[] getBundles()
        {
            return framework.getBundles();
        }

        public void addServiceListener(ServiceListener listener, string filter)
        {
            framework.addServiceListener(listener, filter);
            serviceListenerList.Add(listener);
        }

        public void addServiceListener(ServiceListener listener)
        {
            addServiceListener(listener, null);
        }

        public void removeServiceListener(ServiceListener listener)
        {
            serviceListenerList.Remove(listener);
            framework.removeServiceListener(listener);
        }

        public void addBundleListener(BundleListener listener)
        {
            framework.addBundleListener(listener);
            bundleListenerList.Add(listener);
        }

        public void removeBundleListener(BundleListener listener)
        {
            bundleListenerList.Remove(listener);
            framework.removeBundleListener(listener);
        }

        public void addFrameworkListener(FrameworkListener listener)
        {
            framework.addFrameworkListener(listener);
            frameworkListenerList.Add(listener);
        }

        public void removeFrameworkListener(FrameworkListener listener)
        {
            frameworkListenerList.Remove(listener);
            framework.removeFrameworkListener(listener);
        }

        public ServiceRegistration registerService(string[] clazzes, object service, IDictionary<string, object> properties)
        {
            ServiceRegistration sr = framework.registerService(this, clazzes, service, properties);
            serviceReferenceList.Add(sr.getReference());
            return sr;
        }

        public ServiceRegistration registerService(string clazz, object service, IDictionary<string, object> properties)
        {
            return registerService(new String[] { clazz }, service, properties);
        }

        internal void unregisterService(ServiceReferenceImpl serviceReferenceImpl)
        {
            if (!serviceReferenceList.Contains(serviceReferenceImpl)) return;
            framework.unregisterService(serviceReferenceImpl);
            serviceReferenceList.Remove(serviceReferenceImpl);
        }

        public ServiceReference[] getServiceReferences(string clazz, string filter)
        {
            ServiceReference[] serviceReferences = new ServiceReference[serviceReferenceList.Count];
            serviceReferenceList.CopyTo(serviceReferences, 0);
            return serviceReferences;
        }

        public ServiceReference[] getAllServiceReferences(string clazz, string filter)
        {
            throw new NotImplementedException();
        }

        public ServiceReference getServiceReference(string clazz)
        {
            ServiceReference sr = framework.getServiceReference(clazz);
            if (sr != null && !bundleUsingServiceReferenceList.Contains(sr)) bundleUsingServiceReferenceList.Add(sr);
            return sr;
        }

        public object getService(ServiceReference reference)
        {
            return framework.getService(reference, getBundle());
        }

        internal Bundle[] getUsingBundles(ServiceReferenceImpl serviceReferenceImpl)
        {
            return framework.getUsingBundles(serviceReferenceImpl);
        }

        public bool ungetService(ServiceReference reference)
        {
            if (reference != null && bundleUsingServiceReferenceList.Contains(reference)) bundleUsingServiceReferenceList.Remove(reference);
            return framework.ungetService(reference, getBundle());
        }

        public System.IO.FileInfo getDataFile(string filename)
        {
            throw new NotImplementedException();
        }

        public Filter createFilter(string filter)
        {
            throw new NotImplementedException();
        }

        public Bundle getBundle(string location)
        {
            throw new NotImplementedException();
        }

        public Bundle getBundle()
        {
            return bundle;
        }

        internal void stop()
        {
            //移除服务监听器
            foreach (ServiceListener listener in serviceListenerList)
            {
                framework.removeServiceListener(listener);
            }
            serviceListenerList.Clear();
            //移除Bundle监听器
            foreach (BundleListener listener in bundleListenerList)
            {
                framework.removeBundleListener(listener);
            }
            bundleListenerList.Clear();
            //移除Framework监听器
            foreach (FrameworkListener listener in frameworkListenerList)
            {
                framework.removeFrameworkListener(listener);
            }
            frameworkListenerList.Clear();
            //移除已注册的服务
            foreach (ServiceReference reference in serviceReferenceList)
            {
                framework.unregisterService(reference);
            }
            serviceReferenceList.Clear();
            //取消使用正在使用的服务
            ServiceReference[] bundleUsingServiceReferences = new ServiceReference[bundleUsingServiceReferenceList.Count];
            bundleUsingServiceReferenceList.CopyTo(bundleUsingServiceReferences, 0);
            foreach (ServiceReference reference in bundleUsingServiceReferences)
            {
                ungetService(reference);
            }
            bundleUsingServiceReferenceList.Clear();
        }

        internal ServiceReference[] getRegisteredServices()
        {
            ServiceReference[] serviceReferences = new ServiceReference[serviceReferenceList.Count];
            serviceReferenceList.CopyTo(serviceReferences, 0);
            return serviceReferences;
        }

        internal ServiceReference[] getServicesInUse()
        {
            ServiceReference[] references = new ServiceReference[bundleUsingServiceReferenceList.Count];
            bundleUsingServiceReferenceList.CopyTo(references, 0);
            return references;
        }
    }
}
