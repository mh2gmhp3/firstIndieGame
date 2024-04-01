using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logging
{
    public static class Log
    {
        public static void LogInfo(object message)
        {
            Debug.Log(message);
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
}
