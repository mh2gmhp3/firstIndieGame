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
    [GameSystem(GameSystemPriority.UI_SYSTEM)]
    public partial class UISystem : BaseGameSystem<UISystem>
    {
        private const string GUI_ROOT_RESOURCE_PATH = "Framework/UI/GUIRoot";

        private GameObject _guiRoot = null;
        private Transform _guiRootTrans = null;
        private RectTransform _guiRootRectTrans = null;

        private Dictionary<string, UIWindows> _nameToWindows =
            new Dictionary<string, UIWindows>();

        private HashSet<string> _loadingWindowsNameSet =
            new HashSet<string>();

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

        public static void PreLoadUIWindows(string name, Action<UIWindows> onLoadedCallback = null)
        {
            _instance.DoPreLoadUIWindows(name, onLoadedCallback);
        }

        public static void OpenUIWindows(string name, UIData uiData, Action<UIWindows> onOpenCallback = null)
        {
            _instance.DoOpenUIWindows(name, uiData, onOpenCallback);
        }

        public static void CloseUIWindows(string name)
        {
            _instance.DoCloseUIWindows(name);
        }

        #endregion

        #region Private Method UIWindows

        private void DoPreLoadUIWindows(string name, Action<UIWindows> onLoadedCallback = null)
        {
            if (_nameToWindows.TryGetValue(name, out UIWindows windows))
            {
                Log.LogWarning($"{name} is already preload");
                if (onLoadedCallback != null)
                    onLoadedCallback.Invoke(windows);
                return;
            }

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

        private void HandleLoadedPreLoadUIWindows(
            string name,
            UIWindows windows,
            Action<UIWindows> onLoadedCallback = null)
        {
            AddLoadedAndInitUIWindow(name, windows);
            //先關閉
            windows.Close();
            if (onLoadedCallback != null)
                onLoadedCallback.Invoke(windows);
        }

        private void DoOpenUIWindows(string name, UIData uiData, Action<UIWindows> onOpenCallback = null)
        {
            if (_nameToWindows.TryGetValue(name, out UIWindows windows))
            {
                OnOpenUIWindows(
                    name,
                    windows,
                    uiData,
                    onOpenCallback);
                return;
            }

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

        private void HandleLoadedOpenUIWindows(
            string name,
            UIWindows windows,
            UIData uiData,
            Action<UIWindows> onOpenCallback = null)
        {
            AddLoadedAndInitUIWindow(name, windows);
            OnOpenUIWindows(
                name,
                windows,
                uiData,
                onOpenCallback);
        }

        private void OnOpenUIWindows(
            string name,
            UIWindows windows,
            UIData uiData,
            Action<UIWindows> onOpenCallback = null)
        {
            GetWindowsSorting(name, out string sortingLayerName, out int sortingOrder);
            windows.SetSortingLayer(sortingLayerName, sortingOrder);
            windows.Open(uiData);
            if (onOpenCallback != null)
                onOpenCallback.Invoke(windows);
        }

        private void AddLoadedAndInitUIWindow(string name, UIWindows windows)
        {
            if (_nameToWindows.ContainsKey(name))
                return;

            _nameToWindows.Add(name, windows);
            windows.Init();
        }

        private void DoCloseUIWindows(string name)
        {
            if (!_nameToWindows.TryGetValue(name, out UIWindows windows))
                return;

            windows.Close();
        }

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

        private void GetWindowsSorting(string name, out string sortingLayerName, out int sortingOrder)
        {
            sortingLayerName = "Default";
            sortingOrder = ++_nowTopSortingOrder;
        }

        private string GetWindowsPath(string name)
        {
            return _uiWindowsPath + "/" + name;
        }

        #endregion
    }
}
