using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Utility
{
    public static class TypeUtility
    {
        /// <summary>
        /// 獲取指定類型的繼承類
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allInherits">是否基類存在此繼承就算 false:只會檢查一次基類</param>
        /// <returns></returns>
        public static List<Type> GetTypeListByInheritsType<T>(bool allInherits)
        {
            List<Type> result = new List<Type>();

            var targetType = typeof(T);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    var baseType = type;
                    do
                    {
                        baseType = baseType.BaseType;
                        if (baseType == null)
                            break;
                        if (baseType == targetType)
                        {
                            result.Add(type);
                            break;
                        }
                    } while (allInherits && baseType != null);
                }
            }

            return result;
        }
    }
}