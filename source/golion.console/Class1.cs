using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Text;
using golion.calc;
using log4net;

namespace golion.console
{
    public class Class1 : BundleActivator
    {
        //private static ILog log = LogManager.GetLogger(typeof(Class1));

        public void start(BundleContext context)
        {
            //ServiceReference reference = context.getServiceReference(typeof(CalcService).FullName);
            //if (reference == null)
            //{
            //    log.Warn("未找到服务引用！！！");
            //    return;
            //}
            //int x = 4324;
            //int y = 7833;
            //CalcService calcService = context.getService(reference) as CalcService;
            //log.Info(String.Format("调用服务测试：{0} + {1} = {2}", x, y, calcService.Add(x, y)));
            //context.ungetService(reference);
        }

        public void stop(BundleContext context)
        {
            
        }
    }
}