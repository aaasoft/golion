using System;
using System.Collections.Generic;
using System.Text;
using org.osgi.framework.launch;

namespace test.command
{
    class ExitCommand : ICommand
    {
        private Framework framework;
        public ExitCommand(Framework framework)
        {
            this.framework = framework;
        }

        public string GetCommandName()
        {
            return "exit";
        }

        public string GetHelpText()
        {
            return "退出程序";
        }

        public string GetDetailHelpText()
        {
            return "退出程序\r\n\r\nexit";
        }

        public string ExecuteCommand(string commandLine)
        {
            framework.stop();
            Environment.Exit(0);
            return "";
        }
    }
}
