using Framework.GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Framework.Utility
{
    public static class AttributeUtility
    {
        public static List<(Type Type, T Attribute)> GetAllAtttibuteTypeList<T>() where T : Attribute
        {
            List<(Type Type, T Attribute)> result = new List<(Type Type, T Attribute)>();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    T attribute = type.GetCustomAttribute<T>(true);
                    if (attribute == null)
                        continue;

                    result.Add((type, attribute));
                }
            }

            return result;
        }
    }
}