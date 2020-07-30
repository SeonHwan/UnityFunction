using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class StaticFunction
{
    public static long ConvertByte2KB(long bytes)
    {
        return bytes / 1024;
    }

    public static long ConvertKB2MB(long kbytes)
    {
        return kbytes / 1024;
    }

    public static long ConvertByte2MB(long bytes)
    {
        return ConvertKB2MB(ConvertByte2KB(bytes));
    }

    public static string ConvertByte2KBStr(long bytes)
    {
        return string.Format("{0:N2} KB", ConvertByte2KB(bytes));
    }

    public static string ConvertKB2MBStr(long kbytes)
    {
        return string.Format("{0:N2} MB", ConvertKB2MB(kbytes));
    }

    public static string ConvertByte2MBStr(long bytes)
    {
        return string.Format("{0:N2} MB", ConvertByte2MB(bytes));
    }

    //Namespace.ClassName.FieldName 형태의 스트링을 넘겨주면 그 값을 직접 받을 수 있는 함수
    //C# Reflection
    public static object GetValueReflection(string fieldName, params BindingFlags[] flags)
    {
        int getFlags = 0x00;
        foreach(var flag in flags)
        {
            getFlags |= (int)flag;
        }
        var asms = AppDomain.CurrentDomain.GetAssemblies();

        int endDotIdx = fieldName.LastIndexOf('.');
        string qualifiedName = fieldName.Substring(0, endDotIdx);
        string propertyName = fieldName.Substring(endDotIdx + 1);

        object findObject = null;
        string fullName = null;
        foreach(var asm in asms)
        {
            fullName = asm.FullName;
            Type checkType = Type.GetType($"{qualifiedName}, {fullName}");
            if(checkType != null)
            {
                PropertyInfo pi = checkType.GetProperty(propertyName, (BindingFlags)getFlags);
                findObject = pi.GetValue(null, null);
                break;
            }
        }

        return findObject;
    }
}
