using System;
using System.Collections.Generic;
using System.Text;
using org.osgi.framework;
using org.osgi.framework.launch;
using test.utils;

namespace test.command
{
    class StartBundleCommand : ICommand
    {
        private Framework framework;
        public StartBundleCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "start";
        }

        public string GetHelpText()
        {
            return "启动插件";
        }

        public string GetDetailHelpText()
        {
            return "启动插件\r\n\r\rn"
                + "start [插件ID]  启动插件ID为[插件ID]的插件";
        }

        public string ExecuteCommand(string commandLine)
        {
            String bundleIdStr = commandLine.Substring(GetCommandName().Length).Trim();
            Int64 bundleId = Int64.Parse(bundleIdStr);
            Bundle bundle = framework.getBundleContext().getBundle(bundleId);
            if (bundle == null) return String.Format("未找到ID为[{0}]的插件", bundleId);
            bundle.start();
            return String.Format("启动插件[{0} ({1})]完成，当前状态为:{2}", bundle.getSymbolicName(), bundle.getVersion(), BundleUtils.GetBundleStateString(bundle.getState()));
        }
    }
}
