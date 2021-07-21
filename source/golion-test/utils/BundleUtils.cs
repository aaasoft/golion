﻿using org.osgi.framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace test.utils
{
    public class BundleUtils
    {
        private static IDictionary<String, String> bundleStateIntStringDict;
        static BundleUtils()
        {
            bundleStateIntStringDict = new Dictionary<String, String>();

            Type bundleConstType = typeof(Bundle_Const);
            foreach (FieldInfo fi in bundleConstType.GetFields())
            {
                if (!fi.IsPublic || !fi.IsStatic || !fi.IsLiteral || !fi.FieldType.Equals(typeof(Int32))) continue;
                Int32 fieldValue = (Int32)fi.GetValue(null);
                String stateString = fi.Name;
                bundleStateIntStringDict.Add(fieldValue.ToString(), stateString);
            }
        }

        public static String GetBundleStateString(Int32 state)
        {
            String stateStr = state.ToString();
            if (bundleStateIntStringDict.ContainsKey(stateStr))
            {
                stateStr = bundleStateIntStringDict[stateStr];
            }
            return stateStr;
        }
    }
}
