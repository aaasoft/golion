using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.osgi.framework
{
    public class Bundle_Const
    {
        public const int UNINSTALLED = 0x00000001;
        public const int INSTALLED = 0x00000002;
        public const int RESOLVED = 0x00000004;
        public const int STARTING = 0x00000008;
        public const int STOPPING = 0x00000010;
        public const int ACTIVE = 0x00000020;
    }

    public class Bundle_Const_Other
    {
        public const int START_TRANSIENT = 0x00000001;
        public const int START_ACTIVATION_POLICY = 0x00000002;
        public const int STOP_TRANSIENT = 0x00000001;
        public const int SIGNERS_ALL = 1;
        public const int SIGNERS_TRUSTED = 2;
    }

    public interface Bundle : IComparable<Bundle>
    {
        T adapt<T>(Type type);
        IEnumerator<Stream> findEntries(String path, String filePattern, Boolean recurse);
        BundleContext getBundleContext();
        long getBundleId();
        String getDataFile(String filename);
        Stream getEntry(String path);
        IEnumerator<String> getEntryPaths(String path);
        IDictionary<String, String> getHeaders();
        IDictionary<String, String> getHeaders(String locale);
        long getLastModified();
        String getLocation();
        ServiceReference[] getRegisteredServices();
        Stream getResource(String name);
        IEnumerator<Stream> getResources(String name);
        ServiceReference[] getServicesInUse();
        int getState();
        String getSymbolicName();
        Version getVersion();
        Type loadClass(String name);
        void start();
        void start(int options);
        void stop();
        void stop(int options);
        void uninstall();
        void update();
        void update(Stream input);
    }
}
