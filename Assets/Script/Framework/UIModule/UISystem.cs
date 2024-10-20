using AssetsModule;
using GameSystem;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Utility;
using UObject = UnityEngine.Object;

namespace UIModule
{
    /// <summary>
    /// UI管理主系統
    /// </summary>
    [GameSystem(GameSystemPriority.UI_SYSTEM)]
    public partial class UISystem : BaseGameSystem<UISystem>
    {
        private const string GUI_ROOT_RESOURCE_PATH = "Framework/UI/GUIRoot";

        /// <summary>
        /// GUI Root GameObject
        /// </summary>
        private GameObject _guiRoot = null;
        /// <summary>
        /// GUI Root Transform
        /// </summary>
        private Transform _guiRootTrans = null;
        /// <summary>
        /// GUI Root RectTrandform
        /// </summary>
        private RectTransform _guiRootRectTrans = null;

        /// <summary>
        /// 已經讀取好的Windows
        /// </summary>
        private Dictionary<string, UIWindows> _nameToWindows =
            new Dictionary<string, UIWindows>();

        /// <summary>
        /// 正在讀取的Windows
        /// </summary>
        private HashSet<string> _loadingWindowsNameSet =
            new HashSet<string>();

        /// <summary>
        /// Windows Prefab Resource路徑
        /// </summary>
        private string _uiWindowsPath = "UI/UIWindows";

        //TODO 暫時測試用
        private int _nowTopSortingOrder = 0;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_GUIRoot)
            {
                var obj = AssetsSystem.LoadAssets<GameObject>(GUI_ROOT_RESOURCE_PATH);
                _guiRoot = ObjectUtility.InstantiateWithoutClone(obj);
                _guiRootTrans = _guiRoot.transform;
                _guiRootRectTrans = _guiRootTrans as RectTransform;

                _guiRootRectTrans.SetParent(_transform);
            }
        }

        #region Public Static Method UIWindow

        /// <summary>
        /// 預先讀取Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoadedCallback"></param>
        public static void PreLoadUIWindows(string name, Action<UIWindows> onLoadedCallback = null)
        {
            _instance.DoPreLoadUIWindows(name, onLoadedCallback);
        }

        /// <summary>
        /// 開啟Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        public static void OpenUIWindows(string name, UIData uiData, Action<UIWindows> onOpenCallback = null)
        {
            _instance.DoOpenUIWindows(name, uiData, onOpenCallback);
        }

        /// <summary>
        /// 關閉Windows
        /// </summary>
        /// <param name="name"></param>
        public static void CloseUIWindows(string name)
        {
            _instance.DoCloseUIWindows(name);
        }

        #endregion

        #region Private Method UIWindows

        /// <summary>
        /// 預先讀取Windows 預先讀取不開啟
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoadedCallback"></param>
        private void DoPreLoadUIWindows(string name, Action<UIWindows> onLoadedCallback = null)
        {
            //存在直接呼叫callback
            if (_nameToWindows.TryGetValue(name, out UIWindows windows))
            {
                Log.LogWarning($"{name} is already preload");
                if (onLoadedCallback != null)
                    onLoadedCallback.Invoke(windows);
                return;
            }

            //讀取中
            if (_loadingWindowsNameSet.Contains(name))
                return;

            _loadingWindowsNameSet.Add(name);
            AssetsSystem.LoadAssetsAsync(GetWindowsPath(name),
                (obj) =>
                {
                    _loadingWindowsNameSet.Remove(name);
                    if (!TryCreateUIwindows(obj, out var windows))
                    {
                        Log.LogError($"UIWindows:{name} can't Create", true);
                        return;
                    }
                    HandleLoadedPreLoadUIWindows(
                        name,
                        windows,
                        onLoadedCallback);
                });
        }

        /// <summary>
        /// 處理讀取好預先讀取的Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windows"></param>
        /// <param name="onLoadedCallback"></param>
        private void HandleLoadedPreLoadUIWindows(
            string name,
            UIWindows windows,
            Action<UIWindows> onLoadedCallback = null)
        {
            //添加與初始化Windows
            AddLoadedAndInitUIWindow(name, windows);
            //先關閉
            windows.Close();
            if (onLoadedCallback != null)
                onLoadedCallback.Invoke(windows);
        }

        /// <summary>
        /// 開啟Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void DoOpenUIWindows(string name, UIData uiData, Action<UIWindows> onOpenCallback = null)
        {
            //已存在直接開啟
            if (_nameToWindows.TryGetValue(name, out UIWindows windows))
            {
                OnOpenUIWindows(
                    name,
                    windows,
                    uiData,
                    onOpenCallback);
                return;
            }

            //讀取中
            if (_loadingWindowsNameSet.Contains(name))
                return;

            _loadingWindowsNameSet.Add(name);
            AssetsSystem.LoadAssetsAsync(GetWindowsPath(name),
                (obj) =>
                {
                    _loadingWindowsNameSet.Remove(name);
                    if (!TryCreateUIwindows(obj, out var windows))
                    {
                        Log.LogError($"UIWindows:{name} can't Create", true);
                        return;
                    }
                    HandleLoadedOpenUIWindows(
                        name,
                        windows,
                        uiData,
                        onOpenCallback);
                });
        }

        /// <summary>
        /// 處裡讀取好要開啟的Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windows"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void HandleLoadedOpenUIWindows(
            string name,
            UIWindows windows,
            UIData uiData,
            Action<UIWindows> onOpenCallback = null)
        {
            //添加與初始化Windows
            AddLoadedAndInitUIWindow(name, windows);
            OnOpenUIWindows(
                name,
                windows,
                uiData,
                onOpenCallback);
        }

        /// <summary>
        /// 執行開啟Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windows"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void OnOpenUIWindows(
            string name,
            UIWindows windows,
            UIData uiData,
            Action<UIWindows> onOpenCallback = null)
        {
            //獲取階層
            GetWindowsSorting(name, out string sortingLayerName, out int sortingOrder);
            windows.SetSortingLayer(sortingLayerName, sortingOrder);
            //開啟
            windows.Open(uiData);
            if (onOpenCallback != null)
                onOpenCallback.Invoke(windows);
        }

        /// <summary>
        /// 添加與初始化Windows
        /// </summary>
        /// <param name="name"></param>
        /// <param name="windows"></param>
        private void AddLoadedAndInitUIWindow(string name, UIWindows windows)
        {
            if (_nameToWindows.ContainsKey(name))
                return;

            _nameToWindows.Add(name, windows);
            windows.Init();
        }

        /// <summary>
        /// 關閉Windows
        /// </summary>
        /// <param name="name"></param>
        private void DoCloseUIWindows(string name)
        {
            if (!_nameToWindows.TryGetValue(name, out UIWindows windows))
                return;

            windows.Close();
        }

        /// <summary>
        /// 嘗試建立Windows實體
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="windows"></param>
        /// <returns></returns>
        private bool TryCreateUIwindows(UObject obj, out UIWindows windows)
        {
            windows = null;
            if (obj == null)
                return false;

            windows = obj.GetComponent<UIWindows>();
            if (windows == null)
                return false;

            var go = ObjectUtility.InstantiateWithoutClone(obj);
            windows = go.GetComponent<UIWindows>();
            windows.transform.SetParent(_guiRootTrans);
            return true;
        }

        /// <summary>
        /// 獲取Windows階層
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sortingLayerName"></param>
        /// <param name="sortingOrder"></param>
        private void GetWindowsSorting(string name, out string sortingLayerName, out int sortingOrder)
        {
            sortingLayerName = "Default";
            sortingOrder = ++_nowTopSortingOrder;
        }

        /// <summary>
        /// 獲取Windows Resource prefab路徑
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetWindowsPath(string name)
        {
            return _uiWindowsPath + "/" + name;
        }

        #endregion
    }
}
