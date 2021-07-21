using System;
using System.Collections.Generic;
using System.Text;
using org.osgi.framework;
using org.osgi.framework.launch;

namespace test.command
{
    class UninstallBundleCommand : ICommand
    {
        private Framework framework;
        public UninstallBundleCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "uninstall";
        }

        public string GetHelpText()
        {
            return "卸载插件";
        }

        public string GetDetailHelpText()
        {
            return "卸载插件\r\n\r\n"
                + "uninstall [插件ID]  卸载插件ID为[插件ID]的插件";
        }

        public string ExecuteCommand(string commandLine)
        {
            String bundleIdStr = commandLine.Substring(GetCommandName().Length).Trim();
            Int64 bundleId = Int64.Parse(bundleIdStr);
            Bundle bundle = framework.getBundleContext().getBundle(bundleId);
            if (bundle == null) return String.Format("未找到ID为[{0}]的Bundle", bundleId);
            bundle.stop();
            bundle.uninstall();
            return String.Format("插件[{0} ({1})]已卸载.", bundle.getSymbolicName(), bundle.getVersion());
        }
    }
}
