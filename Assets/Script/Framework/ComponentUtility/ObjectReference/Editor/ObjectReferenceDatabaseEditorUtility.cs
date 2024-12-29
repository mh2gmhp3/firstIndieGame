using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Framework.ComponentUtility.ObjectReferenceDatabase;

namespace Framework.ComponentUtility.Editor
{
    public static class ObjectReferenceDatabaseEditorUtility
    {
        /// <summary>
        /// 變數定義資料
        /// </summary>
        private class VariableDefineData
        {
            /// <summary>變數類型名稱 </summary>
            public string TypeName;
            /// <summary>變數名稱</summary>
            public string VariableName;
            /// <summary>獲取物件呼教函式</summary>
            public string GetObjectMethod;

            public VariableDefineData(ObjectReference objectRef)
            {
                TypeName = objectRef.GenVariableTypeName();
                VariableName = objectRef.GenVariableName();
                GetObjectMethod = objectRef.GenVariableGetObjectMethodString();
            }

            /// <summary>
            /// 生成變數定義行
            /// </summary>
            /// <returns></returns>
            public string GenVariableDefineLine()
            {
                return $"private {TypeName} {VariableName};";
            }

            /// <summary>
            /// 生成初始化變數獲取物件行
            /// </summary>
            /// <returns></returns>
            public string GenInitGetObjectLine()
            {
                return $"{VariableName} = {GetObjectMethod};";
            }
        }

        /// <summary>初始化區域標記</summary>
        private const string REGION_MARK = "//#";
        /// <summary>物件參考區域標記</summary>
        private const string REGION_REF_MARK = "//#REF#";
        /// <summary>初始化物件參考區域標記</summary>
        private const string REGION_INIT_REF_MARK = "//#INIT_REF#";
        /// <summary>初始化事件註冊區域標記</summary>
        private const string REGION_INIT_EVENT_MARK = "//#INIT_EVENT#";

        /// <summary>所有區域標記</summary>
        private static readonly HashSet<string> ALL_REGION_MARK_SET = new HashSet<string>()
        {
            REGION_REF_MARK,
            REGION_INIT_REF_MARK,
            REGION_INIT_EVENT_MARK,
        };

        /// <summary>
        /// 寫入至腳本內
        /// </summary>
        /// <param name="scriptPath">腳本完整路徑</param>
        /// <param name="objectReferenceDB">要寫入的資料</param>
        public static void WriteToScript(string scriptPath, ObjectReferenceDatabase objectReferenceDB)
        {
            if (objectReferenceDB == null)
            {
                Log.LogError("資料為null, 寫入失敗");
                return;
            }

            if (!File.Exists(scriptPath))
            {
                Log.LogError($"找不到目標腳本 Path:{scriptPath}, 寫入失敗");
                return;
            }

            var filelines = File.ReadAllLines(scriptPath).ToList();
            if (!CheckWriteRegion(filelines))
            {
                Log.LogError($"腳本內寫入區域錯誤 Path:{scriptPath}, 寫入失敗");
                return;
            }

            var success = true;
            success &= WriteUsingNamespaceIfNotExist(filelines, objectReferenceDB.GetAllTypeNamespaceList());
            success &= WriteRefVariableDefine(filelines, objectReferenceDB.GetRefVariableDatas());
            if (success)
                File.WriteAllLines(scriptPath, filelines, Encoding.UTF8);
        }

        #region 區域行為

        /// <summary>
        /// 檢查寫入區域
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static bool CheckWriteRegion(List<string> lines)
        {
            if (!TryGetRegion(lines, REGION_REF_MARK, out _, out _))
                return false;
            if (!TryGetRegion(lines, REGION_INIT_REF_MARK, out _, out _))
                return false;
            if (!TryGetRegion(lines, REGION_INIT_EVENT_MARK, out _, out _))
                return false;
            return true;
        }

        /// <summary>
        /// 獲取區域
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="regionMark"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        private static bool TryGetRegion(
            List<string> lines,
            string regionMark,
            out int startIndex,
            out int endIndex)
        {
            startIndex = -1;
            endIndex = -1;

            int numOfLines = lines.Count;
            for (int i = 0; i < numOfLines; i++)
            {
                var line = lines[i];
                var trim = line.Trim();
                if (!trim.StartsWith(REGION_MARK))
                    continue;

                if (trim.StartsWith(regionMark))
                {
                    if (startIndex == -1)
                    {
                        startIndex = i;
                        continue;
                    }
                    if (endIndex == -1)
                    {
                        endIndex = i;
                        return true;
                    }
                }

                //找到開始 但中間有其他的Mark
                if (startIndex > -1
                    && ALL_REGION_MARK_SET.Contains(trim))
                    return false;
            }

            return false;
        }

        /// <summary>
        /// 寫入內容至區域內
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lines"></param>
        /// <param name="region"></param>
        /// <param name="writeContentList"></param>
        /// <param name="writeContentCallback"></param>
        /// <returns></returns>
        private static bool WriteToRegion<T>(
            List<string> lines,
            string region,
            List<T> writeContentList,
            Func<T, string> writeContentCallback)
        {
            if (lines == null ||
                writeContentList == null ||
                writeContentCallback == null)
            {
                return false;
            }

            if (!TryGetRegion(lines, region, out var startIndex, out var endIndex))
                return false;

            //取縮排
            string startLine = lines[startIndex];
            string indent = string.Empty;
            int numOfStartLine = startLine.Length;
            for (int i = 0; i < numOfStartLine; i++)
            {
                char c = startLine[i];
                if (!char.IsWhiteSpace(c))
                    break;

                indent += c;
            }

            //刪除區間內舊的內容
            lines.RemoveRange(startIndex + 1, endIndex - (startIndex + 1));
            //寫入新內容
            int insertIndex = startIndex + 1;
            var numOfWriteContentList = writeContentList.Count;
            for (int i = 0; i < numOfWriteContentList; i++)
            {
                lines.Insert(insertIndex, indent + writeContentCallback.Invoke(writeContentList[i]));
                insertIndex++;
            }

            return true;
        }

        #endregion

        #region Using namespace

        /// <summary>
        /// 獲取ObjectReferenceDatabase 所有Namespace使用列表
        /// </summary>
        /// <param name="objectRefDB"></param>
        /// <returns></returns>
        private static List<string> GetAllTypeNamespaceList(this ObjectReferenceDatabase objectRefDB)
        {
            var result = new HashSet<string>();

            var objectRefList = objectRefDB.ObjectReferenceList;
            int numOfObjectRefList = objectRefList.Count;
            for (int i = 0; i < numOfObjectRefList; i++)
            {
                var objectRef = objectRefList[i];
                if (!objectRef.TryGetObjectType(out var type))
                    continue;

                result.Add(type.Namespace);
            }

            return result.ToList();
        }

        /// <summary>
        /// 寫入Using Namespace 如果不存在的話
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="namespaceList"></param>
        private static bool WriteUsingNamespaceIfNotExist(List<string> lines, List<string> namespaceList)
        {
            var oriUsing = new HashSet<string>();
            int lastUsingIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (!line.StartsWith("using"))
                    continue;

                var ns = line.Replace("using", string.Empty);
                ns = ns.Replace(";", string.Empty);
                oriUsing.Add(ns.Trim());
                lastUsingIndex = i;
            }

            for (int i = 0; i < namespaceList.Count; i++)
            {
                if (oriUsing.Contains(namespaceList[i]))
                    continue;

                lines.Insert(lastUsingIndex + 1, $"using {namespaceList[i]};");
            }
            return true;
        }

        #endregion

        #region RefVariable 參考變數

        /// <summary>
        /// 獲取所有變數定義
        /// </summary>
        /// <param name="objectRefDB"></param>
        /// <returns></returns>
        private static List<VariableDefineData> GetRefVariableDatas(this ObjectReferenceDatabase objectRefDB)
        {
            var result = new List<VariableDefineData>();
            var numOfObjectRefList = objectRefDB.ObjectReferenceList.Count;
            for (int i = 0; i < numOfObjectRefList; i++)
            {
                var objectRef = objectRefDB.ObjectReferenceList[i];
                result.Add(new VariableDefineData(objectRef));
            }
            return result;
        }

        /// <summary>
        /// 生成變數類型字串
        /// </summary>
        /// <param name="objectReference"></param>
        /// <returns></returns>
        private static string GenVariableTypeName(this ObjectReference objectReference)
        {
            string result = string.Empty;
            if (objectReference == null)
                return result;

            if (objectReference.ObjectList.Count > 1)
                return $"List<{objectReference.TypeName}>";
            else
                return $"{objectReference.TypeName}";
        }

        /// <summary>
        /// 生成變數名稱字串
        /// </summary>
        /// <param name="objectReference"></param>
        /// <returns></returns>
        private static string GenVariableName(this ObjectReference objectReference)
        {
            if (objectReference == null)
                return string.Empty;

            return $"{objectReference.TypeName}_{objectReference.Name}";
        }

        /// <summary>
        /// 生成變數獲取物件字串
        /// </summary>
        /// <param name="objectReference"></param>
        /// <returns></returns>
        private static string GenVariableGetObjectMethodString(this ObjectReference objectReference)
        {
            string result = string.Empty;
            if (objectReference == null)
                return result;

            if (objectReference.ObjectList.Count > 1)
                return $"_objectReferenceDb.GetObjectList<{objectReference.TypeName}>(\"{objectReference.Name}\")";
            else
                return $"_objectReferenceDb.GetObject<{objectReference.TypeName}>(\"{objectReference.Name}\")";
        }

        /// <summary>
        /// 生成變數類型
        /// </summary>
        /// <param name="objectReference"></param>
        /// <returns></returns>
        private static bool TryGetObjectType(this ObjectReference objectReference, out Type type)
        {
            type = null;
            if (objectReference == null)
                return false;

            if (objectReference.ObjectList.Count == 0)
                return false;

            type = objectReference.ObjectList[0].GetType();
            return true;
        }

        /// <summary>
        /// 寫入參考變數
        /// </summary>
        /// <returns></returns>
        private static bool WriteRefVariableDefine(
            List<string> lines,
            List<VariableDefineData> variableDefineList)
        {
            bool result = WriteToRegion(
                lines,
                REGION_REF_MARK,
                variableDefineList,
                (v) => { return v.GenVariableDefineLine(); });
            result &= WriteToRegion(
                lines,
                REGION_INIT_REF_MARK,
                variableDefineList,
                (v) => { return v.GenInitGetObjectLine(); });

            return result;
        }

        #endregion
    }
}
