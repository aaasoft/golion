using System;
using System.Collections.Generic;
using System.Text;
using org.osgi.framework;
using org.osgi.framework.launch;

namespace test.command
{
    class InstallBundleCommand : ICommand
    {
        private Framework framework;
        public InstallBundleCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "install";
        }

        public string GetHelpText()
        {
            return "安装插件";
        }

        public string GetDetailHelpText()
        {
            return "安装插件\r\n\r\n"
                    + "install [路径]  从[路径]安装插件";
        }

        public string ExecuteCommand(string commandLine)
        {
            String location = commandLine.Substring(GetCommandName().Length).Trim();
            Bundle bundle = null;
            try
            {
                bundle = framework.getBundleContext().installBundle(location);
            }
            catch (Exception ex)
            {
                return String.Format("安装插件出错，原因：{0}", ex.Message);
            }
            return String.Format("插件[{0} ({1})]已安装.", bundle.getSymbolicName(), bundle.getVersion());
        }
    }
}
