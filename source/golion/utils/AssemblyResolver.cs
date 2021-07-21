using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using org.osgi.framework;

namespace golion.utils
{
    public class AssemblyResolver : IDisposable
    {
        private Assembly assembly;
        private IList<CustomAttributeData> attributeDataList;
        private CollectibleAssemblyLoadContext assemblyLoadContext;

        public void Init(Byte[] assemblyByteArray)
        {
            assemblyLoadContext = new CollectibleAssemblyLoadContext();
            using (var ms = new MemoryStream(assemblyByteArray))
                this.assembly = assemblyLoadContext.LoadFromStream(ms);
            attributeDataList = CustomAttributeData.GetCustomAttributes(assembly);
        }

        public void Init(Assembly assembly)
        {
            this.assembly = assembly;
            attributeDataList = CustomAttributeData.GetCustomAttributes(assembly);
        }

        public String GetAssemblyFullName()
        {
            return assembly.FullName;
        }

        public String GetAssemblyName()
        {
            return assembly.GetName().Name;
        }

        public Version GetVersion()
        {
            return assembly.GetName().Version;
        }

        public String GetAssemblyTitle()
        {
            String title = getCustomAttributeData(typeof(System.Reflection.AssemblyTitleAttribute)).ToString();
            if (String.IsNullOrEmpty(title))
                title = GetAssemblyName();
            return title;
        }

        public String GetVendor()
        {
            return getCustomAttributeData(typeof(System.Reflection.AssemblyCompanyAttribute)).ToString();
        }

        private Object getCustomAttributeData(Type type)
        {
            foreach (CustomAttributeData cad in attributeDataList)
            {
                if (cad.Constructor.DeclaringType.Equals(type))
                {
                    return cad.ConstructorArguments[0].Value;
                }
            }
            return null;
        }

        public String GetAssemblyRequiredAssembly()
        {
            StringBuilder sb = new StringBuilder();
            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                String referenceAssemblyName = assemblyName.Name;
                if (referenceAssemblyName.StartsWith("System.")) continue;
                if (referenceAssemblyName == "mscorlib") continue;
                if (referenceAssemblyName.Equals(typeof(AssemblyResolver).Assembly.GetName().Name)) continue;

                sb.Append(String.Format("{0};bundle-version=\"{1}\"", referenceAssemblyName, assemblyName.Version));
                sb.Append(",");
            }
            if (sb.Length == 0) return "";
            return sb.ToString(0, sb.Length - 1);
        }

        public void Dispose()
        {
            this.assembly = null;
            attributeDataList = null;
            assemblyLoadContext?.Unload();
            assemblyLoadContext = null;
            GC.Collect();
        }
    }
}
