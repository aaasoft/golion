using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using org.osgi.framework;
using org.osgi.framework.launch;
using test.utils;

namespace test.command
{
    public class ListBundleCommand : ICommand
    {
        private Framework framework;

        public ListBundleCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "lb";
        }

        public string GetDetailHelpText()
        {
            return "列出所有已安装插件的ID，状态和名称信息\r\n\r\nlb";
        }

        public string ExecuteCommand(string commandLine)
        {
            Bundle[] bundles = framework.getBundleContext().getBundles();
            StringBuilder sb = new StringBuilder();
            sb.Append("ID".PadLeft(5));
            sb.Append("|");
            sb.Append("State".PadRight(12));
            sb.Append("|");
            sb.Append("Name");
            sb.AppendLine();
            foreach (Bundle bundle in bundles)
            {
                sb.Append(bundle.getBundleId().ToString().PadLeft(5));
                sb.Append("|");
                sb.Append(BundleUtils.GetBundleStateString(bundle.getState()).PadRight(12));
                sb.Append("|");
                sb.Append(String.Format("{0} ({1}) \"{2}\"", bundle.getSymbolicName(), bundle.getVersion(), bundle.getHeaders()["Name"]));
                sb.AppendLine();
            }
            return sb.ToString();
        }


        public string GetHelpText()
        {
            return "列出所有插件及状态";
        }
    }
}
