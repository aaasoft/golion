using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.osgi.framework.launch
{
    public interface Framework : Bundle
    {
        void init();
        FrameworkEvent waitForStop(long timeout);
    }
}
