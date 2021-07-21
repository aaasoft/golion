using golion;
using org.osgi.framework;
using org.osgi.framework.launch;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return "列出全部或者指定Bundle加载的程序集";
        }

        public string GetDetailHelpText()
        {
            return @"la 列出当前应用程序域全部加载的程序集
la [bundleId] 列出指定Bundle加载的程序集，当bundleId为0时，显示全部程序集加载上下文加载的程序集";
        }

        public string ExecuteCommand(string commandLine)
        {
            String bundleIdStr = commandLine.Substring(GetCommandName().Length).Trim();
            if (String.IsNullOrEmpty(bundleIdStr))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("AppDomain.CurrentDomain.GetAssemblies()");
                sb.AppendLine("-----------------------");
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Assembly assembly = assemblies[i];
                    AssemblyName assemblyName = assembly.GetName();
                    sb.Append(String.Format("  [{0}]: {1}, Version:{2}", i, assemblyName.Name, assemblyName.Version));
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            else
            {
                Int64 bundleId = Int64.Parse(bundleIdStr);
                Bundle bundle = framework.getBundleContext().getBundle(bundleId);
                if (bundle == null) return String.Format("未找到ID为[{0}]的插件", bundleId);
                if (bundle is BundleImpl)
                {
                    BundleImpl bundleImpl = (BundleImpl)bundle;
                    return bundleImpl.GetAssemblies();
                }
                else if (bundle is golion.launch.FrameworkImpl)
                {
                    StringBuilder sb = new StringBuilder();
                    var list = AssemblyLoadContext.All.ToList();
                    list.Remove(AssemblyLoadContext.Default);
                    list.Insert(0, AssemblyLoadContext.Default);
                    foreach (var context in list)
                    {
                        sb.AppendLine();
                        sb.AppendLine(context.Name);
                        sb.AppendLine("-----------------------");
                        Assembly[] assemblies = context.Assemblies.ToArray();
                        for (int i = 0; i < assemblies.Length; i++)
                        {
                            Assembly assembly = assemblies[i];
                            AssemblyName assemblyName = assembly.GetName();
                            sb.Append(String.Format("  [{0}]: {1}, Version:{2}", i, assemblyName.Name, assemblyName.Version));
                            sb.AppendLine();
                        }
                    }
                    return sb.ToString();
                }
                return null;
            }
        }
    }
}
