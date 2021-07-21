using System;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public class ServiceEvent : EventArgs
    {
        public const int REGISTERED = 0x00000001;
        public const int MODIFIED = 0x00000002;
        public const int UNREGISTERING = 0x00000004;
        public const int MODIFIED_ENDMATCH = 0x00000008;

        private int type;
        private ServiceReference reference;

        public ServiceEvent(int type, ServiceReference reference)
        {
            this.type = type;
            this.reference = reference;
        }

        public ServiceReference getServiceReference()
        {
            return reference;
        }
        public int getType()
        {
            return type;
        }
    }
}
