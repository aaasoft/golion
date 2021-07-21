using org.osgi.framework;
using org.osgi.framework.launch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using log4net;
using golion.utils;
using System.Threading;

namespace golion.launch
{
    public class FrameworkImpl : Framework
    {
        private static ILog log = LogManager.GetLogger(typeof(FrameworkImpl));
        private const String DEFAULT_BUNDLE_DIRECTORY_NAME = "plugins";
        private const String BUNDLE_ID_FILE_NAME = "bundle.id";
        private const String BUNDLE_STATE_FILE_NAME = "state";
        private const String FRAMEWORK_LOCK_FILE = "lock";
        
        // Bundle缓存目录
        public const String ORG_OSGI_FRAMEWORK_STORAGE = "org.osgi.framework.storage";
        // 传递到各Bundle的程序集
        public const String ORG_OSGI_FRAMEWORK_BOOTDELEGATION = "org.osgi.framework.bootdelegation";

        private List<Bundle> bundleList = new List<Bundle>();
        private IList<ServiceListener> serviceListenerList = new List<ServiceListener>();
        private IList<BundleListener> bundleListenerList = new List<BundleListener>();
        private IList<FrameworkListener> frameworkListenerList = new List<FrameworkListener>();

        private IDictionary<String, IList<ServiceReference>> serviceReferenceDictionary = new Dictionary<String, IList<ServiceReference>>();
        private IDictionary<ServiceReference, IList<Bundle>> usingServiceBundleDict = new Dictionary<ServiceReference, IList<Bundle>>();

        private String bundleSymbolicName;
        private Version bundleVersion;
        private String bundlesDirectoryPath;
        private BundleContext bundleContext;
        private Int64 nextBundleId;
        private Int32 state;
        private IDictionary<String, String> headerDictionary;
        private IDictionary<string, string> configuration;
        private FileStream lockFileStream;

        /// <summary>
        /// 得到下一个插件的编号
        /// </summary>
        /// <returns></returns>
        public Int64 getNextBundleId()
        {
            return nextBundleId;
        }

        /// <summary>
        /// 自增
        /// </summary>
        public void IncraseNextBundleId()
        {
            nextBundleId++;
        }

        public FrameworkImpl()
            : this(null)
        {
        }

        public FrameworkImpl(IDictionary<string, string> configuration)
        {
            this.configuration = configuration;
            bundleContext = new BundleContextImpl(this, this);
            //加载配置
            headerDictionary = new Dictionary<String, String>();
            AssemblyResolver assemblyResolver = new AssemblyResolver();
            assemblyResolver.Init(typeof(FrameworkImpl).Assembly);
            this.bundleSymbolicName = assemblyResolver.GetAssemblyName();
            headerDictionary.Add("SymbolicName", bundleSymbolicName);
            this.bundleVersion = assemblyResolver.GetVersion();
            headerDictionary.Add("Version", bundleVersion.ToString());
            headerDictionary.Add("Name", assemblyResolver.GetAssemblyTitle());
            headerDictionary.Add("Vendor", assemblyResolver.GetVendor());
            headerDictionary.Add("Require-Bundle", assemblyResolver.GetAssemblyRequiredAssembly());
            assemblyResolver = null;
        }

        public T adapt<T>(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<System.IO.Stream> findEntries(string path, string filePattern, bool recurse)
        {
            throw new NotImplementedException();
        }

        public long getBundleId()
        {
            return 0;
        }

        public System.IO.Stream getEntry(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> getEntryPaths(string path)
        {
            throw new NotImplementedException();
        }

        public string getLocation()
        {
            return "https://github.com/aaasoft/golion";
        }

        public string getSymbolicName()
        {
            return bundleSymbolicName;
        }

        public void init()
        {
            Assembly selfAssembly = typeof(FrameworkImpl).Assembly;
            BundleImpl.AddBootDelegationAssembly(selfAssembly.GetName().Name, selfAssembly);

            // 初始化配置参数
            if (configuration != null)
            {
                //设置Bundle缓存位置
                if (configuration.ContainsKey(ORG_OSGI_FRAMEWORK_STORAGE))
                    bundlesDirectoryPath = configuration[ORG_OSGI_FRAMEWORK_STORAGE];
                //设置boot delegation
                if (configuration.ContainsKey(ORG_OSGI_FRAMEWORK_BOOTDELEGATION))
                {
                    String[] bootDelegationNames = configuration[ORG_OSGI_FRAMEWORK_BOOTDELEGATION].Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        String assemblyName = assembly.GetName().Name;
                        foreach (String bootDelegationName in bootDelegationNames)
                        {
                            if (assemblyName.Equals(bootDelegationName))
                            {
                                BundleImpl.AddBootDelegationAssembly(assemblyName, assembly);
                            }
                        }
                    }
                }
            }
            // 设置插件目录
            if (String.IsNullOrEmpty(bundlesDirectoryPath))
                bundlesDirectoryPath = System.IO.Path.Combine(Environment.CurrentDirectory, DEFAULT_BUNDLE_DIRECTORY_NAME);
            //先将自己添加上去
            bundleList.Add(this);

            IList<String> installedBundleNameVersionList = new List<String>();
            if (Directory.Exists(bundlesDirectoryPath))
            {
                //读取已经安装的Bundle
                DirectoryInfo di = new DirectoryInfo(bundlesDirectoryPath);
                List<Int64> installedBundleIdList = new List<Int64>();
                foreach (DirectoryInfo subDi in di.GetDirectories())
                {
                    Int64 tmpInt64;
                    if (!Int64.TryParse(subDi.Name, out tmpInt64)) continue;
                    installedBundleIdList.Add(tmpInt64);
                }
                installedBundleIdList.Sort();
                foreach (Int64 bundleId in installedBundleIdList)
                {
                    String bundleDirectoryName = Path.Combine(bundlesDirectoryPath, bundleId.ToString());
                    try
                    {
                        Bundle bundle = new BundleImpl(this, bundleDirectoryName);
                        String bundleNameVersionString = String.Format("{0}({1}))", bundle.getSymbolicName(), bundle.getVersion());
                        if (installedBundleNameVersionList.Contains(bundleNameVersionString))
                        {
                            log.Warn(String.Format("插件[{0})]存在多重安装，只加载第一个！", bundleNameVersionString));
                            continue;
                        }
                        bundleList.Add(bundle);
                        installedBundleNameVersionList.Add(bundleNameVersionString);
                    }
                    catch (Exception ex)
                    {
                        log.Error("加载Bundle时出现异常！", ex);
                    }
                }
                nextBundleId = bundleList[bundleList.Count - 1].getBundleId() + 1;
            }
            else
            {
                Directory.CreateDirectory(bundlesDirectoryPath);
                nextBundleId = 1;
            }
        }

        public void start()
        {
            start(0);
        }

        public void start(int options)
        {
            //锁定文件
            String lockFileName = Path.Combine(bundlesDirectoryPath, FRAMEWORK_LOCK_FILE);
            try
            {
                lockFileStream = File.Open(lockFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                throw new IOException("锁定Bundle缓存目录失败，请确认没有另外的golion实例正在运行或者当前用户有访问此目录的权限。", ex);
            }

            state = Bundle_Const.STARTING;

            foreach (Bundle bundle in bundleList)
            {
                if (bundle.Equals(this)) continue;

                String bundleDirectoryPath = (bundle as BundleImpl).GetBundleDirectoryPath();
                String bundleStateFilePath = Path.Combine(bundleDirectoryPath, BUNDLE_STATE_FILE_NAME);
                if (File.Exists(bundleStateFilePath))
                {
                    Int32 preState = Int32.Parse(File.ReadAllText(bundleStateFilePath));
                    if (preState != Bundle_Const.ACTIVE) continue;
                }

                if (bundle.getState() == Bundle_Const.INSTALLED
                    || bundle.getState() == Bundle_Const.RESOLVED)
                {
                    try
                    {
                        bundle.start(options);
                    }
                    catch (Exception ex)
                    {
                        log.Error(String.Format("启动插件[{0}]时出现异常", bundle), ex);
                        break;
                    }
                }
            }
            state = Bundle_Const.ACTIVE;
            log.Debug("框架启动完成！");
        }

        public void stop()
        {
            stop(0);
        }

        public void stop(int options)
        {
            if (lockFileStream != null) lockFileStream.Close();
            state = Bundle_Const.STOPPING;
            foreach (Bundle bundle in bundleList)
            {
                if (bundle.Equals(this)) continue;
                String bundleDirectoryPath = (bundle as BundleImpl).GetBundleDirectoryPath();
                String bundleStateFilePath = Path.Combine(bundleDirectoryPath, BUNDLE_STATE_FILE_NAME);
                File.WriteAllText(bundleStateFilePath, bundle.getState().ToString());
                if (bundle.getState() == Bundle_Const.ACTIVE)
                {
                    bundle.stop(options);
                }
            }
            state = Bundle_Const.RESOLVED;
        }

        // 解压插件文件到插件目录
        internal void extractBundleFile(String location, Stream input, String bundleDirectoryName, Boolean isInputStreamOwner)
        {
            // 清理删除插件的引用程序集目录
            String libsDirPath = Path.Combine(bundleDirectoryName, BundleImpl.BUNDLE_LIBS_DIRECTORY_NAME);
            if (Directory.Exists(libsDirPath)) Directory.Delete(libsDirPath, true);
            // 清理主插件程序集
            String bundleAssemblyFilePath = Path.Combine(bundleDirectoryName, BundleImpl.BUNDLE_FILE_NAME);
            if (File.Exists(bundleAssemblyFilePath)) File.Delete(bundleAssemblyFilePath);
            
            //开始解压
            FastZip fastZip = new FastZip();
            fastZip.ExtractZip(input, bundleDirectoryName, FastZip.Overwrite.Always, null, null, null, true, isInputStreamOwner);
            File.WriteAllText(Path.Combine(bundleDirectoryName, BundleImpl.BUNDLE_LOCATION_FILE_NAME), location);
        }

        internal Bundle installBundle(string location, Stream input)
        {
            Boolean isInputStreamOwner;
            Exception installException = null;

            if (input == null)
            {
                if (File.Exists(location))
                {
                    input = new FileStream(location, FileMode.Open);
                }
                else
                {
                    throw new ArgumentException(String.Format("无法找到路径[{0}]对应的文件！", location));
                }
                isInputStreamOwner = true;
            }
            else
            {
                location = "Stream:";
                isInputStreamOwner = false;
            }

            String newBundleDirectoryName = Path.Combine(bundlesDirectoryPath, getNextBundleId().ToString());
            try
            {
                if (!Directory.Exists(newBundleDirectoryName))
                    Directory.CreateDirectory(newBundleDirectoryName);
                //解压插件文件到插件目录
                extractBundleFile(location, input, newBundleDirectoryName, isInputStreamOwner);
                //验证并加载插件
                Bundle bundle = new BundleImpl(this, newBundleDirectoryName);
                foreach (Bundle installedBundle in bundleList)
                {
                    //如果此插件的相同版本已经安装
                    if (installedBundle.getSymbolicName().Equals(bundle.getSymbolicName())
                        && installedBundle.getVersion().Equals(bundle.getVersion()))
                    {
                        throw new Exception(String.Format("名称为[{0}]，版本为[{1}]的插件已经存在，安装失败！", installedBundle.getSymbolicName(), installedBundle.getVersion()));
                    }
                }
                bundleList.Add(bundle);
                IncraseNextBundleId();
                return bundle;
            }
            catch (Exception ex)
            {
                installException = ex;
                throw;
            }
            finally
            {
                if (installException != null && Directory.Exists(newBundleDirectoryName))
                    Directory.Delete(newBundleDirectoryName, true);
            }
        }

        public void uninstall()
        {
            throw new NotImplementedException();
        }

        internal void uninstall(BundleImpl bundleImpl)
        {
            bundleList.Remove(bundleImpl);
            Directory.Delete(bundleImpl.GetBundleDirectoryPath(), true);
        }

        public void update()
        {
            update(null);
        }

        public void update(System.IO.Stream input)
        {
            this.stop();
            this.start();
        }

        public org.osgi.framework.FrameworkEvent waitForStop(long timeout)
        {
            throw new NotImplementedException();
        }


        public org.osgi.framework.BundleContext getBundleContext()
        {
            return bundleContext;
        }

        public String getDataFile(string filename)
        {
            throw new NotImplementedException();
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
            return 0;
        }

        public org.osgi.framework.ServiceReference[] getRegisteredServices()
        {
            return new ServiceReference[0];
        }

        public System.IO.Stream getResource(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<System.IO.Stream> getResources(string name)
        {
            throw new NotImplementedException();
        }

        public org.osgi.framework.ServiceReference[] getServicesInUse()
        {
            return new ServiceReference[0];
        }

        public int getState()
        {
            return state;
        }

        public Version getVersion()
        {
            return bundleVersion;
        }

        public Type loadClass(string name)
        {
            return Type.GetType(name);
        }

        public int CompareTo(org.osgi.framework.Bundle other)
        {
            return this.getBundleId().CompareTo(other.getBundleId());
        }

        internal Bundle getBundle(long id)
        {
            foreach (Bundle bundle in bundleList)
            {
                if (bundle.getBundleId() == id)
                    return bundle;
            }
            return null;
        }

        internal Bundle[] getBundles()
        {
            Bundle[] bundles = new Bundle[bundleList.Count];
            bundleList.CopyTo(bundles, 0);
            return bundles;
        }

        internal void addServiceListener(ServiceListener listener, string filter)
        {
            serviceListenerList.Add(listener);
        }

        internal void removeServiceListener(ServiceListener listener)
        {
            serviceListenerList.Remove(listener);
        }

        internal void addBundleListener(BundleListener listener)
        {
            bundleListenerList.Add(listener);
        }

        internal void removeBundleListener(BundleListener listener)
        {
            bundleListenerList.Remove(listener);
        }

        internal void addFrameworkListener(FrameworkListener listener)
        {
            frameworkListenerList.Add(listener);
        }

        internal void removeFrameworkListener(FrameworkListener listener)
        {
            frameworkListenerList.Remove(listener);
        }

        internal void fireFrameworkEvent(FrameworkEvent frameworkEvent)
        {
            foreach (FrameworkListener listener in frameworkListenerList)
            {
                listener.frameworkEvent(frameworkEvent);
            }
        }

        internal void fireBundleEvent(BundleEvent bundleEvent)
        {
            foreach (BundleListener listener in bundleListenerList)
            {
                listener.bundleChanged(bundleEvent);
            }
        }

        internal void fireServiceEvent(ServiceEvent serviceEvent)
        {
            foreach (ServiceListener listener in serviceListenerList)
            {
                listener.serviceChanged(serviceEvent);
            }
        }

        internal ServiceRegistration registerService(BundleContext bundleContext, string[] clazzes, object service, IDictionary<string, object> properties)
        {
            BundleContextImpl bundleContextImpl = bundleContext as BundleContextImpl;

            ServiceReferenceImpl reference = new ServiceReferenceImpl(bundleContextImpl, clazzes, properties, service);
            foreach (String clazz in clazzes)
            {
                IList<ServiceReference> serviceReferenceList = null;
                if (serviceReferenceDictionary.ContainsKey(clazz))
                {
                    serviceReferenceList = serviceReferenceDictionary[clazz];
                }
                else
                {
                    serviceReferenceList = new List<ServiceReference>();
                    serviceReferenceDictionary.Add(clazz, serviceReferenceList);
                }
                serviceReferenceList.Add(reference);
            }
            fireServiceEvent(new ServiceEvent(ServiceEvent.REGISTERED, reference));
            return new ServiceRegistrationImpl(this, bundleContextImpl, reference);
        }

        internal void unregisterService(ServiceReference serviceReference)
        {
            fireServiceEvent(new ServiceEvent(ServiceEvent.UNREGISTERING, serviceReference));
            foreach (String clazz in serviceReferenceDictionary.Keys)
            {
                IList<ServiceReference> serviceReferenceList = serviceReferenceDictionary[clazz];
                if (serviceReferenceList.Contains(serviceReference))
                {
                    serviceReferenceList.Remove(serviceReference);
                }
            }
            if (usingServiceBundleDict.ContainsKey(serviceReference))
            {
                usingServiceBundleDict.Remove(serviceReference);
            }
        }

        internal ServiceReference getServiceReference(string clazz)
        {
            foreach (String tmpClazz in serviceReferenceDictionary.Keys)
            {
                if (!tmpClazz.Equals(clazz)) continue;
                IList<ServiceReference> serviceReferenceList = serviceReferenceDictionary[clazz];
                if (serviceReferenceList.Count > 0)
                {
                    return serviceReferenceList[0];
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        internal Bundle[] getUsingBundles(ServiceReferenceImpl serviceReferenceImpl)
        {
            if (usingServiceBundleDict.ContainsKey(serviceReferenceImpl))
            {
                IList<Bundle> usingBundleList = usingServiceBundleDict[serviceReferenceImpl];
                Bundle[] usingBundles = new Bundle[usingBundleList.Count];
                usingBundleList.CopyTo(usingBundles, 0);
                return usingBundles;
            }
            else
            {
                return new Bundle[0];
            }
        }

        internal object getService(ServiceReference reference, Bundle bundle)
        {
            ServiceReferenceImpl sri = reference as ServiceReferenceImpl;
            if (sri == null) return null;

            IList<Bundle> usingBundleList = null;
            if (usingServiceBundleDict.ContainsKey(reference))
            {
                usingBundleList = usingServiceBundleDict[reference];
            }
            else
            {
                usingBundleList = new List<Bundle>();
            }
            usingBundleList.Add(bundle);
            return sri.getService();
        }

        internal bool ungetService(ServiceReference reference, Bundle bundle)
        {
            if (usingServiceBundleDict.ContainsKey(reference))
            {
                IList<Bundle> usingBundleList = usingServiceBundleDict[reference];
                return usingBundleList.Remove(bundle);
            }
            return false;
        }
    }
}
