using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public class FrameworkEvent : EventArgs
    {
        public const int STARTED = 0x00000001;
        public const int ERROR = 0x00000002;
        public const int PACKAGES_REFRESHED = 0x00000004;
        public const int STARTLEVEL_CHANGED = 0x00000008;
        public const int WARNING = 0x00000010;
        public const int INFO = 0x00000020;
        public const int STOPPED = 0x00000040;
        public const int STOPPED_UPDATE = 0x00000080;
        public const int STOPPED_BOOTCLASSPATH_MODIFIED = 0x00000100;
        public const int WAIT_TIMEDOUT = 0x00000200;

        private int type;
        private Bundle bundle;
        private Exception exception;

        public FrameworkEvent(int type, Bundle bundle, Exception exception)
        {
            this.type = type;
            this.bundle = bundle;
            this.exception = exception;
        }

        public Bundle getBundle()
        {
            return bundle;
        }
        public Exception getThrowable()
        {
            return exception;
        }
        public int getType()
        {
            return type;
        }
    }
}
