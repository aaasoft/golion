using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using org.osgi.framework;
using org.osgi.framework.launch;

namespace test.command
{
    public class UpdateBundleCommand : ICommand
    {
        private Framework framework;
        public UpdateBundleCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "update";
        }

        public string GetHelpText()
        {
            return "更新插件";
        }

        public string GetDetailHelpText()
        {
            return "更新插件\r\n\r\n"
            + "update [插件编号]                更新插件编号为[插件编号]的插件\r\n"
            + "update [插件编号] [更新文件路径] 从[更新文件路径]更新插件编号为[插件编号]的插件\r\n";
        }

        public string ExecuteCommand(string commandLine)
        {
            String args = commandLine.Substring(GetCommandName().Length).Trim();

            Int64 bundleId = 0;
            Stream inputStream = null;

            if (args.Contains(" "))
            {
                Int32 spaceIndex = args.IndexOf(" ");
                bundleId = Int64.Parse(args.Substring(0, spaceIndex));
                String location = args.Substring(spaceIndex).Trim();
                if (File.Exists(location))
                {
                    inputStream = File.Open(location, FileMode.Open);
                }
                else
                {
                    throw new ArgumentException(String.Format("无法找到路径[{0}]对应的文件！", location));
                }
            }
            else
            {
                bundleId = Int64.Parse(args);
            }

            Bundle bundle = null;
            try
            {
                bundle = framework.getBundleContext().getBundle(bundleId);
                if (bundle == null) return String.Format("未找到ID为[{0}]的插件", bundleId);
                bundle.update(inputStream);
            }
            catch (Exception ex)
            {
                return String.Format("更新插件出错，原因：{0}", ex.Message);
            }
            finally
            {
                if (inputStream != null) inputStream.Close();
            }
            return String.Format("插件[{0} ({1})]已更新.", bundle.getSymbolicName(), bundle.getVersion());
        }
    }
}
