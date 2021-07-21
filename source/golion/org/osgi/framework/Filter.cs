using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace org.osgi.framework
{
    public interface Filter
    {
        Boolean Equals(Object obj);
        int GetHashCode();
        Boolean match(IDictionary<String, Object> dictionary);
        Boolean match(ServiceReference reference);
        Boolean matchCase(IDictionary<String, Object> dictionary);
        Boolean matches(Hashtable hashtable);
        String ToString();
    }
}
