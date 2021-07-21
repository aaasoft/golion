using System;
using System.Collections.Generic;
using System.Text;
using org.osgi.framework;

namespace golion.calc.aaasoft
{
    public class Activator : BundleActivator
    {
        private ServiceRegistration calcServiceRegistration;

        public void start(BundleContext context)
        {
            CalcService calcService = new CalcServiceImpl();
            calcServiceRegistration = context.registerService(typeof(CalcService).FullName, calcService, null);
        }

        public void stop(BundleContext context)
        {
            calcServiceRegistration.unregister();
        }
    }
}
