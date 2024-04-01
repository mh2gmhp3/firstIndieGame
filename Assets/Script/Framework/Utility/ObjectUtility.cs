using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public static class ObjectUtility
    {
        public static T InstantiateWithoutClone<T>(T original) where T : Object
        {
            T result = Object.Instantiate(original);
            result.name = result.name.Replace("(Clone)", "");
            return result;
        }
    }
}
