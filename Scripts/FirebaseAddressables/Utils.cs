using System;
using System.Reflection;

using UnityEngine;

using UnityEditor.AddressableAssets.Settings;

namespace NakiedLib
{
    public class Utils
    {
        public static string GetReflectionProfilePath(AddressableAssetProfileSettings pSettings, string profileID, string varName)
        {
            string path = pSettings.GetValueByName(profileID, varName);

            var asms = AppDomain.CurrentDomain.GetAssemblies();

            int pFrom = 0;
            int pTo = 0;

            int lastDotIdx = 0;

            string assemblyQualifiedName = null;
            string propertyName = null;

            while (IsContainsBracket(path))
            {
                pFrom = path.IndexOf("[") + "[".Length;
                pTo = path.IndexOf("]");
                if (pFrom >= pTo)
                    continue;

                String extractStr = path.Substring(pFrom, pTo - pFrom);
                if (pSettings.GetVariableNames().Contains(extractStr))
                {
                    string _pValue = pSettings.GetValueByName(profileID, extractStr);
                    path = path.Replace($"[{extractStr}]", _pValue);
                    continue;
                }

                lastDotIdx = extractStr.LastIndexOf('.');

                assemblyQualifiedName = extractStr.Substring(0, lastDotIdx);
                propertyName = extractStr.Substring(lastDotIdx + 1);

                Debug.Log($"assemblyQualifiedName : {assemblyQualifiedName}, propertyName : {propertyName}");

                object findObject = null;
                string fullName = null;


                foreach (var asm in asms)
                {
                    fullName = asm.FullName;
                    //Debug.Log($"FullName : {fullName}");
                    Type _checkT = Type.GetType($"{assemblyQualifiedName}, {fullName}");
                    if (_checkT != null)
                    {
                        PropertyInfo _pI = _checkT.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                        findObject = _pI.GetValue(null, null);
                        break;
                    }
                }

                if (findObject != null)
                {
                    path = path.Replace($"[{extractStr}]", findObject.ToString());
                }
                else
                {
                    path = path.Replace($"[{extractStr}]", extractStr);
                }
            }

            return path;
        }

        private static bool IsContainsBracket(string src)
        {
            return src.Contains("[") && src.Contains("]");
        }
    }

}