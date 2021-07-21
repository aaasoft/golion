using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using org.osgi.framework.launch;

namespace test.command
{
    class UpdateBundleTestCommand : ICommand
    {
        private Framework framework;
        public UpdateBundleTestCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "test";
        }

        public string GetHelpText()
        {
            return "测试Bundle的更新";
        }

        public string GetDetailHelpText()
        {
            return "测试Bundle的更新\r\n\r\n"
                    + "test [插件ID] [测试次数]";
        }

        public string ExecuteCommand(string commandLine)
        {
            Int64 preWorkingSet = Process.GetCurrentProcess().WorkingSet64;

            String args = commandLine.Substring(GetCommandName().Length).Trim();
            String[] argArray = args.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            Int64 bundleId = Int64.Parse(argArray[0]);
            Int64 testTimes = Int64.Parse(argArray[1]);

            for (int i = 0; i < testTimes; i++)
            {
                framework.getBundleContext().getBundle(bundleId).update();
            }
            Int64 afterWorkingSet = Process.GetCurrentProcess().WorkingSet64;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("        之前".PadRight(20) + "之后".PadRight(20));
            sb.AppendLine("内存    " + preWorkingSet.ToString("N").Replace(".00", "").PadRight(20) + afterWorkingSet.ToString("N").Replace(".00", "").PadRight(20));
            return sb.ToString();
        }
    }
}
