using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Logging
{
    public static class Log
    {
        public static void LogInfo(object message, bool withMethod = false)
        {
            Debug.Log(GetMessage(message, withMethod));
        }

        public static void LogWarning(object message, bool withMethod = false)
        {
            Debug.LogWarning(GetMessage(message, withMethod));
        }

        public static void LogError(object message, bool withMethod = false)
        {
            Debug.LogError(GetMessage(message, withMethod));
        }

        private static string GetMessage(object message, bool withMethod)
        {
            string msg = message == null? "null message" : message.ToString();
            if (withMethod)
                msg = GetCallerMethodName(3) + " " + msg;
            //GetCallerMethodName => 0
            //GetMessage => 1
            //各Log接口 => 2
            //呼叫的Method => 3 使用
            return msg;
        }

        private static string GetCallerMethodName(int stackFrameIndex)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(stackFrameIndex);

            var method = sf.GetMethod();
            if (method == null)
                return "Method is invalid";

            return $"{method.DeclaringType.Name}.{method.Name}";
        }
    }
}
