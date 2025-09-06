using AssetModule;
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
        /// 已經讀取好的Window
        /// </summary>
        private Dictionary<string, UIWindow> _nameToWindow =
            new Dictionary<string, UIWindow>();

        /// <summary>
        /// 正在讀取的Window
        /// </summary>
        private HashSet<string> _loadingWindowNameSet =
            new HashSet<string>();

        /// <summary>
        /// Window Prefab Resource路徑
        /// </summary>
        private string _uiWindowPath = "UI/UIWindow";

        //TODO 暫時測試用
        private int _nowTopSortingOrder = 0;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_GUIRoot)
            {
                var obj = AssetSystem.LoadAsset<GameObject>(GUI_ROOT_RESOURCE_PATH);
                _guiRoot = ObjectUtility.InstantiateWithoutClone(obj);
                _guiRootTrans = _guiRoot.transform;
                _guiRootRectTrans = _guiRootTrans as RectTransform;

                _guiRootRectTrans.SetParent(_transform);
            }
        }

        #region Public Static Method UIWindow

        /// <summary>
        /// 預先讀取Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoadedCallback"></param>
        public static void PreLoadUIWindow(string name, Action<UIWindow> onLoadedCallback = null)
        {
            _instance.DoPreLoadUIWindow(name, onLoadedCallback);
        }

        /// <summary>
        /// 開啟Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        public static void OpenUIWindow(string name, IUIData uiData, Action<UIWindow> onOpenCallback = null)
        {
            _instance.DoOpenUIWindow(name, uiData, onOpenCallback);
        }

        /// <summary>
        /// 關閉Window
        /// </summary>
        /// <param name="name"></param>
        public static void CloseUIWindow(string name)
        {
            _instance.DoCloseUIWindow(name);
        }

        #endregion

        #region Private Method UIWindow

        /// <summary>
        /// 預先讀取Window 預先讀取不開啟
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onLoadedCallback"></param>
        private void DoPreLoadUIWindow(string name, Action<UIWindow> onLoadedCallback = null)
        {
            //存在直接呼叫callback
            if (_nameToWindow.TryGetValue(name, out UIWindow window))
            {
                Log.LogWarning($"{name} is already preload");
                if (onLoadedCallback != null)
                    onLoadedCallback.Invoke(window);
                return;
            }

            //讀取中
            if (_loadingWindowNameSet.Contains(name))
                return;

            _loadingWindowNameSet.Add(name);
            AssetSystem.LoadAssetAsync(GetWindowPath(name),
                (obj) =>
                {
                    _loadingWindowNameSet.Remove(name);
                    if (!TryCreateUIWindow(obj, out var window))
                    {
                        Log.LogError($"UIWindow:{name} can't Create", true);
                        return;
                    }
                    HandleLoadedPreLoadUIWindow(
                        name,
                        window,
                        onLoadedCallback);
                });
        }

        /// <summary>
        /// 處理讀取好預先讀取的Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="window"></param>
        /// <param name="onLoadedCallback"></param>
        private void HandleLoadedPreLoadUIWindow(
            string name,
            UIWindow window,
            Action<UIWindow> onLoadedCallback = null)
        {
            //添加與初始化Window
            AddLoadedAndInitUIWindow(name, window);
            //先關閉
            window.Close();
            if (onLoadedCallback != null)
                onLoadedCallback.Invoke(window);
        }

        /// <summary>
        /// 開啟Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void DoOpenUIWindow(string name, IUIData uiData, Action<UIWindow> onOpenCallback = null)
        {
            //已存在直接開啟
            if (_nameToWindow.TryGetValue(name, out UIWindow window))
            {
                OnOpenUIWindow(
                    name,
                    window,
                    uiData,
                    onOpenCallback);
                return;
            }

            //讀取中
            if (_loadingWindowNameSet.Contains(name))
                return;

            _loadingWindowNameSet.Add(name);
            AssetSystem.LoadAssetAsync(GetWindowPath(name),
                (obj) =>
                {
                    _loadingWindowNameSet.Remove(name);
                    if (!TryCreateUIWindow(obj, out var window))
                    {
                        Log.LogError($"UIWindow:{name} can't Create", true);
                        return;
                    }
                    HandleLoadedOpenUIWindow(
                        name,
                        window,
                        uiData,
                        onOpenCallback);
                });
        }

        /// <summary>
        /// 處裡讀取好要開啟的Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="window"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void HandleLoadedOpenUIWindow(
            string name,
            UIWindow window,
            IUIData uiData,
            Action<UIWindow> onOpenCallback = null)
        {
            //添加與初始化Window
            AddLoadedAndInitUIWindow(name, window);
            OnOpenUIWindow(
                name,
                window,
                uiData,
                onOpenCallback);
        }

        /// <summary>
        /// 執行開啟Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="window"></param>
        /// <param name="uiData"></param>
        /// <param name="onOpenCallback"></param>
        private void OnOpenUIWindow(
            string name,
            UIWindow window,
            IUIData uiData,
            Action<UIWindow> onOpenCallback = null)
        {
            //獲取階層
            GetWindowSorting(name, out string sortingLayerName, out int sortingOrder);
            window.SetSortingLayer(sortingLayerName, sortingOrder);
            //開啟
            window.Open(uiData);
            if (onOpenCallback != null)
                onOpenCallback.Invoke(window);
        }

        /// <summary>
        /// 添加與初始化Window
        /// </summary>
        /// <param name="name"></param>
        /// <param name="window"></param>
        private void AddLoadedAndInitUIWindow(string name, UIWindow window)
        {
            if (_nameToWindow.ContainsKey(name))
                return;

            _nameToWindow.Add(name, window);
            window.WindowName = name;
            window.Init();
        }

        /// <summary>
        /// 關閉Window
        /// </summary>
        /// <param name="name"></param>
        private void DoCloseUIWindow(string name)
        {
            if (!_nameToWindow.TryGetValue(name, out UIWindow window))
                return;

            window.Close();
        }

        /// <summary>
        /// 嘗試建立Window實體
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        private bool TryCreateUIWindow(UObject obj, out UIWindow window)
        {
            window = null;
            if (obj == null)
                return false;

            window = obj.GetComponent<UIWindow>();
            if (window == null)
                return false;

            var go = ObjectUtility.InstantiateWithoutClone(obj);
            window = go.GetComponent<UIWindow>();
            window.transform.SetParent(_guiRootTrans);
            return true;
        }

        /// <summary>
        /// 獲取Window階層
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sortingLayerName"></param>
        /// <param name="sortingOrder"></param>
        private void GetWindowSorting(string name, out string sortingLayerName, out int sortingOrder)
        {
            sortingLayerName = "Default";
            sortingOrder = ++_nowTopSortingOrder;
        }

        /// <summary>
        /// 獲取Window Resource prefab路徑
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetWindowPath(string name)
        {
            return _uiWindowPath + "/" + name;
        }

        #endregion
    }
}
