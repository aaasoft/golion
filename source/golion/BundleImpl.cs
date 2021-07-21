using golion.launch;
using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using golion.utils;
using log4net;
using System.Runtime.Loader;
using System.Linq;

namespace golion
{
    public class BundleImpl : Bundle
    {
        private static ILog log = LogManager.GetLogger(typeof(BundleImpl));
        private static Encoding defaultEncoding = new UTF8Encoding(false);

        public const String BUNDLE_FILE_NAME = "bundle";
        public const String BUNDLE_LIBS_DIRECTORY_NAME = "libs";
        public const String BUNDLE_LOCATION_FILE_NAME = "location";
        public const String BUNDLE_MANIFEST_REQUIRED_BUNDLE_VERSION = "bundle-version";

        private String bundleDirectoryPath;
        //Bundle程序集文件路径
        private String bundleAssemblyFileName;
        //Bundle程序集全名
        private String bundleAssemblyFullName;
        private String bundleSymbolicName = "<未加载>";
        private Version bundleVersion;
        private BundleActivator bundleActivator;

        private long bundleId;
        private String location;
        private long lastModified;
        private String activatorClassName;
        private String requireBundles;
        private int state = Bundle_Const.INSTALLED;
        private FrameworkImpl framework;
        private BundleContextImpl bundleContext;

        private IDictionary<String, String> headerDictionary;

        private IDictionary<String, Assembly> refAssemblyDict = new Dictionary<String, Assembly>();


        //Bundle的主程序集
        private Assembly bundleAssembly;
        private IList<BundleImpl> requiredBundleList;
        private CollectibleAssemblyLoadContext bundleAssemblyLoadContext;

        /// <summary>
        /// 得到Bundle的主程序集文件路径
        /// </summary>
        /// <returns></returns>
        public string GetBundleAssemblyFileName()
        {
            return bundleAssemblyFileName;
        }

        public BundleImpl[] GetRequiredBundles()
        {
            BundleImpl[] requiredBundles = null;
            lock (requiredBundleList)
            {
                requiredBundles = new BundleImpl[requiredBundleList.Count];
                requiredBundleList.CopyTo(requiredBundles, 0);
            }
            return requiredBundles;
        }

        /// <summary>
        /// 增加代理程序集
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddBootDelegationAssembly(String assemblyName, Assembly assembly)
        {
            //if (allBundleRefAssemblyDict.ContainsKey(assemblyName)) return;
            //allBundleRefAssemblyDict.Add(assemblyName, assembly);
        }

        private void addRefAssembly(String assemblyName, Assembly assembly)
        {
            if (!refAssemblyDict.ContainsKey(assemblyName))
                refAssemblyDict.Add(assemblyName, assembly);
            AddBootDelegationAssembly(assemblyName, assembly);
        }

        private void removeAllRefAssembly()
        {
            refAssemblyDict.Clear();
        }

        /// <summary>
        /// 得到Bundle依赖库的目录名称
        /// </summary>
        /// <returns></returns>
        public String getBundleLibsDirectoryName()
        {
            return Path.Combine(bundleDirectoryPath, BUNDLE_LIBS_DIRECTORY_NAME);
        }

        public BundleImpl(FrameworkImpl framework, String bundleDirectoryPath)
        {
            this.framework = framework;
            this.bundleDirectoryPath = bundleDirectoryPath;
            this.bundleAssemblyFileName = Path.Combine(bundleDirectoryPath, BUNDLE_FILE_NAME);

            init();
            //初始化
            bundleContext = new BundleContextImpl(framework, this);
        }

        private void init()
        {
            //清除之前的所有程序集引用
            this.removeAllRefAssembly();

            //读取基础信息
            DirectoryInfo di = new DirectoryInfo(bundleDirectoryPath);
            bundleId = long.Parse(di.Name);
            //location = File.ReadAllText(Path.Combine(bundleDirectoryPath, BUNDLE_LOCATION_FILE_NAME), defaultEncoding);
            lastModified = File.GetLastWriteTime(bundleAssemblyFileName).Ticks;

            headerDictionary = new Dictionary<String, String>();
            using (var assemblyResolver = new AssemblyResolver())
            {
                assemblyResolver.Init((File.ReadAllBytes(bundleAssemblyFileName)));
                this.bundleAssemblyFullName = assemblyResolver.GetAssemblyFullName();
                this.bundleSymbolicName = assemblyResolver.GetAssemblyName();
                headerDictionary.Add("SymbolicName", bundleSymbolicName);
                this.bundleVersion = assemblyResolver.GetVersion();
                headerDictionary.Add("Version", bundleVersion.ToString());
                headerDictionary.Add("Name", assemblyResolver.GetAssemblyTitle());
                headerDictionary.Add("Vendor", assemblyResolver.GetVendor());
                headerDictionary.Add("Require-Bundle", assemblyResolver.GetAssemblyRequiredAssembly());
            }
            this.requireBundles = headerDictionary["Require-Bundle"];
        }

        public T adapt<T>(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<System.IO.Stream> findEntries(string path, string filePattern, bool recurse)
        {
            throw new NotImplementedException();
        }

        public BundleContext getBundleContext()
        {
            return this.bundleContext;
        }

        public long getBundleId()
        {
            return bundleId;
        }

        public String getDataFile(string filename)
        {
            //数据目录
            String bundleDataFolderName = Path.Combine(bundleDirectoryPath, "data");
            if (!Directory.Exists(bundleDataFolderName))
            {
                Directory.CreateDirectory(bundleDataFolderName);
            }
            if (String.IsNullOrEmpty(filename))
            {
                return bundleDataFolderName;
            }
            else
            {
                return Path.Combine(bundleDataFolderName, filename);
            }
        }

        public System.IO.Stream getEntry(string path)
        {
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }
            path = path.Replace("/", ".");
            return GetManifestResourceStream(path);
        }

        public IEnumerator<string> getEntryPaths(string path)
        {
            List<String> list = new List<string>();
            String[] resourceNames = GetManifestResourceNames();
            foreach (String resourceName in resourceNames)
            {
                if (String.IsNullOrEmpty(path) || resourceName.StartsWith(path))
                {
                    list.Add(resourceName);
                }
            }
            return list.GetEnumerator();
        }

        public IDictionary<string, string> getHeaders()
        {
            return getHeaders(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
        }

        public IDictionary<string, string> getHeaders(string locale)
        {
            //重新拷贝一份，防止外部修改
            return new Dictionary<string, string>(headerDictionary);
        }

        public long getLastModified()
        {
            return lastModified;
        }

        public string getLocation()
        {
            return location;
        }

        public ServiceReference[] getRegisteredServices()
        {
            return bundleContext.getRegisteredServices();
        }

        public System.IO.Stream getResource(string name)
        {
            return getEntry(name);
        }

        public IEnumerator<System.IO.Stream> getResources(string name)
        {
            IEnumerator<String> enumerator = getEntryPaths(name);
            List<Stream> list = new List<Stream>();
            while (enumerator.MoveNext())
            {
                list.Add(getResource(enumerator.Current));
            }
            return list.GetEnumerator();
        }

        public ServiceReference[] getServicesInUse()
        {
            return bundleContext.getServicesInUse();
        }

        public int getState()
        {
            return this.state;
        }

        public string getSymbolicName()
        {
            return this.bundleSymbolicName;
        }

        public Version getVersion()
        {
            return this.bundleVersion;
        }

        public Type loadClass(string name)
        {
            return null;
        }

        private void loadRequiredBundle()
        {
            //加载Required-Bundle中的所有程序集
            foreach (String tmpStr in this.requireBundles.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                String requireBundleString = tmpStr.Trim();
                if (String.IsNullOrEmpty(requireBundleString)) continue;
                String requireBundle = null;
                String requireBundleVersionString = null;

                String[] requireBundleStringArray = requireBundleString.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                requireBundle = requireBundleStringArray[0].Trim();
                IDictionary<String, String> otherDict = new Dictionary<String, String>();
                for (int i = 1; i < requireBundleStringArray.Length; i++)
                {
                    String requireBundleStringPart = requireBundleStringArray[i];
                    String[] requireBundleStringPartStringArray = requireBundleStringPart.Split(new Char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    otherDict.Add(requireBundleStringPartStringArray[0].Trim(), requireBundleStringPartStringArray[1].Trim());
                }
                //得到需要的版本信息
                if (otherDict.ContainsKey(BUNDLE_MANIFEST_REQUIRED_BUNDLE_VERSION))
                {
                    requireBundleVersionString = otherDict[BUNDLE_MANIFEST_REQUIRED_BUNDLE_VERSION];
                    requireBundleVersionString = requireBundleVersionString.Replace("\"", "");
                }

                BundleImpl matchBundle = null;
                List<BundleImpl> matchBundleList = new List<BundleImpl>();
                foreach (Bundle bundle in this.getBundleContext().getBundles())
                {
                    if (bundle.getBundleId() == 0 || bundle.getBundleId() == this.getBundleId()) continue;
                    BundleImpl tmpBundle = bundle as BundleImpl;

                    if (requireBundle.Equals(tmpBundle.getSymbolicName()))
                    {
                        if (String.IsNullOrEmpty(requireBundleVersionString))
                        {
                            matchBundle = tmpBundle;
                            break;
                        }
                        else
                        {
                            matchBundleList.Add(tmpBundle);
                        }
                    }
                }
                if (matchBundle == null && !String.IsNullOrEmpty(requireBundleVersionString))
                {
                    Version requireBundleVersion = new Version(requireBundleVersionString);

                    matchBundleList.Sort(new Comparison<BundleImpl>(delegate (BundleImpl x, BundleImpl y)
                    {
                        return x.getVersion().CompareTo(y.getVersion());
                    }));
                    foreach (BundleImpl tmpBundle in matchBundleList)
                    {
                        if (tmpBundle.getVersion().CompareTo(requireBundleVersion) >= 0)
                        {
                            matchBundle = tmpBundle;
                            break;
                        }
                    }
                }
                if (matchBundle == null)
                    continue;
                //如果此Bundle没有解析，则解析此Bundle
                if (matchBundle.getState() != Bundle_Const.RESOLVED
                    && matchBundle.getState() != Bundle_Const.ACTIVE)
                {
                    matchBundle.resolve();
                }
                requiredBundleList.Add(matchBundle);
            }
        }

        private BundleImpl getBundleFromRequiredBundles(String bundleName)
        {
            foreach (BundleImpl bundle in requiredBundleList)
            {
                if (bundle.getSymbolicName().Equals(bundleName))
                    return bundle;
            }
            return null;
        }

        //加载所需的所有程序集
        private void loadAssemblys()
        {
            using (var fs = File.OpenRead(bundleAssemblyFileName))
                bundleAssembly = bundleAssemblyLoadContext.LoadFromStream(fs);
        }

        public void resolve()
        {
            requiredBundleList = new List<BundleImpl>();
            loadRequiredBundle();

            this.activatorClassName = null;
            unloadAssemblyLoadContext();
            bundleAssemblyLoadContext = new CollectibleAssemblyLoadContext($"Bundle[{getSymbolicName()}] AppDomain");
            bundleAssemblyLoadContext.Resolving += BundleAssemblyLoadContext_Resolving;
            loadAssemblys();
            this.activatorClassName = FindBundleActivatorClassName();            
            if (this.activatorClassName != null)
                headerDictionary.Add("Activator", activatorClassName);

            //解析Bundle完成
            this.state = Bundle_Const.RESOLVED;
            framework.fireBundleEvent(new BundleEvent(BundleEvent.RESOLVED, this));
        }

        private Assembly BundleAssemblyLoadContext_Resolving(AssemblyLoadContext sender, AssemblyName e)
        {
            String assemblyFullName = e.FullName;

            //Console.WriteLine(String.Format("插件[{0}]正试图加载程序集[{1}].", bundle.getSymbolicName(), assemblyFullName));
            //如果是加载Bundle的主程序集
            if (bundleAssembly.FullName.Equals(assemblyFullName)) return bundleAssembly;
            //如果是依赖的Bundle的主程序集
            foreach (BundleImpl requredBundle in GetRequiredBundles())
            {
                String requredBundleAssemblyFullName = requredBundle.GetBundleAssemblyFullName();
                if (assemblyFullName.Equals(requredBundleAssemblyFullName))
                {
                    return requredBundle.bundleAssembly;
                }
            }

            //尝试从插件引用程序集目录下加载
            String[] assemblyNameParts = assemblyFullName.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            String assemblyName = assemblyNameParts[0];
            String assemblyFileName = assemblyNameParts[0] + ".dll";

            String bundleLibsDirectoryName = getBundleLibsDirectoryName();
            String assemblyFullFileName = Path.Combine(bundleLibsDirectoryName, assemblyFileName);
            if (File.Exists(assemblyFullFileName))
            {
                Assembly assembly = null;
                using (var fs = File.OpenRead(assemblyFullFileName))
                    assembly = sender.LoadFromStream(fs);
                return assembly;
            }
            //Console.WriteLine(String.Format("解析插件[{0}]时未能解析程序集[{1}]", bundle.getSymbolicName(), assemblyFullName));
            return null;
        }

        /// <summary>
        /// 搜索Activator类名
        /// </summary>
        /// <returns></returns>
        public String FindBundleActivatorClassName()
        {
            Type[] bundleAssemblyTypes = bundleAssembly.GetTypes();

            //搜索BundleActivator
            foreach (Type type in bundleAssemblyTypes)
            {
                if (type.GetInterface(typeof(BundleActivator).FullName) != null)
                {
                    return type.FullName;
                }
            }
            return null;
        }

        /// <summary>
        /// 得到清单中的资源名称
        /// </summary>
        /// <returns></returns>
        public String[] GetManifestResourceNames()
        {
            return bundleAssembly.GetManifestResourceNames();
        }

        public Stream GetManifestResourceStream(String name)
        {
            return bundleAssembly.GetManifestResourceStream(name);
        }

        /// <summary>
        /// 获取当前应用程序域已加载的程序集信息
        /// </summary>
        /// <returns></returns>
        public String GetAssemblies()
        {
            StringBuilder sb = new StringBuilder();
            Assembly[] assemblies = refAssemblyDict.Values.ToArray();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                AssemblyName assemblyName = assembly.GetName();
                sb.Append(String.Format("[{0}]: {1}, Version:{2}", i, assemblyName.Name, assemblyName.Version));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void start()
        {
            start(0);
        }

        public void start(int options)
        {
            if (this.state == Bundle_Const.INSTALLED)
            {
                try
                {
                    resolve();
                    this.state = Bundle_Const.RESOLVED;
                }
                catch
                {
                    this.state = Bundle_Const.INSTALLED;
                    throw;
                }
            }
            if (this.state == Bundle_Const.RESOLVED && this.activatorClassName != null)
            {
                this.state = Bundle_Const.STARTING;

                try
                {
                    framework.fireBundleEvent(new BundleEvent(BundleEvent.STARTING, this));
                    bundleActivator = Activator.CreateInstance(bundleAssembly.GetType(activatorClassName)) as BundleActivator;
                    bundleActivator.start(bundleContext);
                    this.state = Bundle_Const.ACTIVE;
                    framework.fireBundleEvent(new BundleEvent(BundleEvent.STARTED, this));
                }
                catch
                {
                    this.state = Bundle_Const.RESOLVED;
                    throw;
                }
            }
        }

        public void stop()
        {
            stop(0);
        }

        public void stop(int options)
        {
            if (this.state == Bundle_Const.ACTIVE)
            {
                this.state = Bundle_Const.STOPPING;
                if (bundleActivator != null)
                {
                    framework.fireBundleEvent(new BundleEvent(BundleEvent.STOPPING, this));
                    bundleActivator.stop(bundleContext);
                    bundleContext.stop();
                    unloadAssemblyLoadContext();
                    framework.fireBundleEvent(new BundleEvent(BundleEvent.STOPPED, this));
                    bundleActivator = null;
                }
                this.state = Bundle_Const.RESOLVED;
            }
        }

        private void unloadAssemblyLoadContext()
        {
            bundleAssembly = null;
            if (bundleAssemblyLoadContext != null)
            {
                bundleAssemblyLoadContext.Unload();
                bundleAssemblyLoadContext = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void uninstall()
        {
            framework.uninstall(this);
            this.removeAllRefAssembly();
            unloadAssemblyLoadContext();
        }

        public void update()
        {
            update(null);
        }

        public void update(System.IO.Stream input)
        {
            Int32 preState = state;
            //如果input不为空，则开始安装插件
            if (input != null)
            {
                //验证要安装的插件与当前插件是否匹配
                String tmpBundleDirectoryPath = Path.Combine(Path.GetDirectoryName(bundleDirectoryPath), Int64.MaxValue.ToString());
                framework.extractBundleFile("Stream:", input, tmpBundleDirectoryPath, false);

                try
                {
                    BundleImpl tmpBundleImpl = new BundleImpl(framework, tmpBundleDirectoryPath);
                    if (!this.getSymbolicName().Equals(tmpBundleImpl.getSymbolicName()))
                    {
                        throw new ArgumentException(String.Format("要更新的插件[{0}]与输入流中的插件[{1}]不匹配！", this.getSymbolicName(), tmpBundleImpl.getSymbolicName()));
                    }
                }
                catch
                {
                    Directory.Delete(tmpBundleDirectoryPath, true);
                    throw;
                }

                // 注意，如果有要保存的文件，那么在删除代码之前将文件移动到临时插件目录中
                //先删除自身
                Directory.Delete(bundleDirectoryPath, true);
                //临时目录移动到正式目录
                Directory.Move(tmpBundleDirectoryPath, bundleDirectoryPath);
            }

            if (preState == Bundle_Const.INSTALLED)
            {
                this.init();
            }
            else if (preState == Bundle_Const.RESOLVED)
            {
                this.init();
                this.resolve();
            }
            else if (preState == Bundle_Const.ACTIVE)
            {
                this.stop();
                this.init();
                this.resolve();
                this.start();
            }
        }

        public int CompareTo(Bundle other)
        {
            return this.getBundleId().CompareTo(other.getBundleId());
        }

        public override String ToString()
        {
            return String.Format("Bundle[ID:{0},Name:{1},Version:{2}]", bundleId, bundleSymbolicName, bundleVersion);
        }

        public string GetBundleAssemblyFullName()
        {
            return bundleAssemblyFullName;
        }

        public string GetBundleDirectoryPath()
        {
            return bundleDirectoryPath;
        }
    }
}
