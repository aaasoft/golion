using org.osgi.framework.launch;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using org.osgi.framework;
using test.command;
using System.IO;

namespace test
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.xml"));

            IDictionary<String, ICommand> commandDict = new Dictionary<String, ICommand>();
            IList<ICommand> commandList = new List<ICommand>();


            IDictionary<String, String> golionConfigDict = new Dictionary<String, String>();
            String golionConfigFileName = Path.Combine(Environment.CurrentDirectory, "golion.properties");
            if (File.Exists(golionConfigFileName))
            {
                //读取properties文件
                String[] lines = File.ReadAllLines(golionConfigFileName);
                foreach (String line2 in lines)
                {
                    String line = line2.Trim();
                    if (String.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                    int equalIndex = line.IndexOf("=");
                    if (equalIndex <= 0) continue;
                    String key = line.Substring(0, equalIndex);
                    String value = line.Substring(equalIndex + 1);
                    golionConfigDict.Add(key, value);
                }
            }
            else
            {
                log.Warn("未找到golion配置文件，使用默认的配置！");
            }

            FrameworkFactory factory = new golion.launch.FrameworkFactoryImpl();
            Framework framework = factory.newFramework(golionConfigDict);

            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            framework.getBundleContext().addBundleListener(new ConsoleBundleListener());


            log.Info("golion正在初始化...");
            framework.init();
            log.Info("golion正在启动...");
            try
            {
                framework.start();
                log.Info("golion启动成功！");
            }
            catch (Exception ex)
            {
                log.Error("golion启动失败！"+ex);
                Console.ReadLine();
                return;
            }

            commandList.Add(new HelpCommand(commandDict));
            commandList.Add(new ListBundleCommand(framework));
            commandList.Add(new StartBundleCommand(framework));
            commandList.Add(new StopBundleCommand(framework));
            commandList.Add(new InstallBundleCommand(framework));
            commandList.Add(new UninstallBundleCommand(framework));
            commandList.Add(new UpdateBundleCommand(framework));
            commandList.Add(new ExitCommand(framework));
            commandList.Add(new UpdateBundleTestCommand(framework));
            commandList.Add(new ListAssembliesCommand(framework));

            foreach (ICommand command in commandList)
            {
                commandDict.Add(command.GetCommandName(), command);
            }
            Console.WriteLine("------------------------------");
            Console.WriteLine("欢迎使用，输入help以查看帮助");
            Console.WriteLine("------------------------------");
            Console.WriteLine();
            while (true)
            {
                Console.Write("golion>");
                String commandLine = Console.ReadLine().Trim();
                if (String.IsNullOrEmpty(commandLine)) continue;

                String commandName = commandLine.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (commandDict.ContainsKey(commandName))
                {
                    String result = null;
                    try
                    {
                        result = commandDict[commandName].ExecuteCommand(commandLine);
                        Console.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("命令在执行过程中出现异常，信息：{0}", ex.Message), ex);
                    }
                }
                else
                {
                    log.Error(String.Format("未知命令：{0}", commandName));
                }
            }
        }

        class ConsoleBundleListener : BundleListener
        {
            public void bundleChanged(BundleEvent e)
            {
                Type bundleEventType = typeof(BundleEvent);
                String stateString = e.getType().ToString();
                foreach (FieldInfo fi in bundleEventType.GetFields())
                {
                    if (!fi.IsPublic || !fi.IsStatic || !fi.IsLiteral || !fi.FieldType.Equals(typeof(Int32))) continue;
                    Int32 fieldValue = (Int32)fi.GetValue(null);
                    if (e.getType() == fieldValue)
                    {
                        stateString = fi.Name;
                        break;
                    }
                }
                log.Debug(String.Format("{0}状态已改变为[{1}]", e.getBundle().ToString(), stateString));
            }
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            log.Debug(String.Format("程序集[{0}]已加载到当前应用程序域", args.LoadedAssembly));
        }
    }
}
