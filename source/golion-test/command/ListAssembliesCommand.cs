using golion;
using org.osgi.framework;
using org.osgi.framework.launch;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace test.command
{
    class ListAssembliesCommand : ICommand
    {
        private Framework framework;
        public ListAssembliesCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "la";
        }

        public string GetHelpText()
        {
            return "列出当前AppDomain加载的程序集";
        }

        public string GetDetailHelpText()
        {
            return "la 列出当前AppDomain加载的程序集";
        }

        public string ExecuteCommand(string commandLine)
        {
            String bundleIdStr = commandLine.Substring(GetCommandName().Length).Trim();
            if (String.IsNullOrEmpty(bundleIdStr))
            {
                StringBuilder sb = new StringBuilder();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Assembly assembly = assemblies[i];
                    AssemblyName assemblyName = assembly.GetName();
                    sb.Append(String.Format("[{0}]: {1}, Version:{2}", i, assemblyName.Name, assemblyName.Version));
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            else
            {
                Int64 bundleId = Int64.Parse(bundleIdStr);
                Bundle bundle = framework.getBundleContext().getBundle(bundleId);
                if (bundle == null) return String.Format("未找到ID为[{0}]的插件", bundleId);
                BundleImpl bundleImpl = (BundleImpl)bundle;
                return bundleImpl.GetAssemblies();
            }
        }
    }
}
