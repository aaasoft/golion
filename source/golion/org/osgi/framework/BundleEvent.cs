using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public class BundleEvent : EventArgs
    {
        public const int INSTALLED = 0x00000001;
        public const int STARTED = 0x00000002;
        public const int STOPPED = 0x00000004;
        public const int UPDATED = 0x00000008;
        public const int UNINSTALLED = 0x00000010;
        public const int RESOLVED = 0x00000020;
        public const int UNRESOLVED = 0x00000040;
        public const int STARTING = 0x00000080;
        public const int STOPPING = 0x00000100;
        public const int LAZY_ACTIVATION = 0x00000200;

        private Bundle bundle;
        private Bundle origin;
        private int type;

        public BundleEvent(int type, Bundle bundle, Bundle origin)
        {
            this.bundle = bundle;
            this.type = type;
            this.origin = origin;
        }
        public BundleEvent(int type, Bundle bundle)
            : this(type, bundle, bundle)
        {
        }

        public Bundle getBundle()
        {
            return bundle;
        }
        public Bundle getOrigin()
        {
            return origin;
        }
        public int getType()
        {
            return type;
        }
    }
}
