﻿using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UIElements;
using UObject = UnityEngine.Object;

namespace GameSystem.Framework.Assets
{
    /*
        TODO 需處裡讀取行為設定檔 對應設定檔給特定實作類處理
        1.設定檔
        2.各類別實作 必要?
        3.引用數
        4.釋放
        5.改用Bundle讀取 行動平台 PC版應該暫時不用
     */
    [GameSystem(GameSystemPriority.ASSETS_SYSTEM)]
    public partial class AssetsSystem : BaseGameSystem<AssetsSystem>
    {
        private class LoadingRequest
        {
            public string Path;
            public ResourceRequest Request;
            public Action<UObject> OnLoaded;

            public LoadingRequest(
                string path,
                ResourceRequest request,
                Action<UObject> onLoaded)
            {
                Path = path;
                Request = request;
                OnLoaded = onLoaded;
            }

            public void AddOnLoaded(Action<UnityEngine.Object> onLoaded)
            {
                OnLoaded += onLoaded;
            }

            public bool CheckLoaded()
            {
                if (Request == null)
                    return true;

                return Request.isDone;
            }

            public void InvokeLoaded()
            {
                if (Request == null)
                    return;

                if (OnLoaded == null)
                    return;

                OnLoaded.Invoke(Request.asset);
                OnLoaded = null;
            }

            public UObject GetResult()
            {
                if (Request == null)
                    return null;

                return Request.asset;
            }
        }

        private Dictionary<string, LoadingRequest> _pathToLoadingRequest =
            new Dictionary<string, LoadingRequest>();

        private Dictionary<string, UObject> _pathToLoadedAsset =
            new Dictionary<string, UObject>();

        private List<LoadingRequest> _loadingRequest = new List<LoadingRequest>();

        protected override bool DoEnterGameFlowProcessStep(int flowStep)
        {
            //處裡其他System的讀取結果
            return CheckRequestLoaded();
        }

        protected override void DoUpdate()
        {
            CheckRequestLoaded();
        }

        #region Public Static LoadAssets

        /// <summary>
        /// 讀取資源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadAssets<T>(string path)
            where T : UObject
        {
            return _instance.DoLoadAssets<T>(path);
        }

        /// <summary>
        /// 非同步讀取資源
        /// 作用於EnterGameFlowStep與Update
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onLoaded"></param>
        public static void LoadAssetsAsync(string path, Action<UObject> onLoaded)
        {
            _instance.DoLoadAssetsAsync(path, onLoaded);
        }

        #endregion

        #region Private Method CheckRequestLoaded

        /// <summary>
        /// 檢查請求是否讀取完成
        /// </summary>
        /// <returns></returns>
        private bool CheckRequestLoaded()
        {
            if (_loadingRequest.Count == 0)
                return true;

            int count = _loadingRequest.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var loadingRequest = _loadingRequest[i];
                if (!loadingRequest.CheckLoaded())
                    continue;

                loadingRequest.InvokeLoaded();
                var assetResult = loadingRequest.GetResult();
                if (assetResult != null)
                {
                    if (!_pathToLoadedAsset.ContainsKey(loadingRequest.Path))
                    {
                        _pathToLoadedAsset.Add(loadingRequest.Path, assetResult);
                    }
                }

                _pathToLoadingRequest.Remove(loadingRequest.Path);
                _loadingRequest.RemoveAt(i);
            }

            return _loadingRequest.Count == 0;
        }

        #endregion

        #region Private Method Do LoadAssets

        private T DoLoadAssets<T>(string path)
            where T : UObject
        {
            if (_pathToLoadedAsset.TryGetValue(path, out UObject asset))
            {
                var result = asset as T;
                if (result != null)
                    return result;
            }

            var loadResult = Resources.Load<T>(path);
            return loadResult;
        }

        private void DoLoadAssetsAsync(string path, Action<UObject> onLoaded)
        {
            if (_pathToLoadingRequest.TryGetValue(path, out LoadingRequest loadingRequest))
            {
                loadingRequest.AddOnLoaded(onLoaded);
                return;
            }

            if (_pathToLoadedAsset.TryGetValue(path, out UObject result))
            {
                onLoaded.Invoke(result);
                return;
            }

            ResourceRequest resourceRequest = Resources.LoadAsync(path);
            LoadingRequest newLoadingRequest =
                new LoadingRequest(
                    path,
                    resourceRequest,
                    onLoaded);

            _pathToLoadingRequest.Add(path, newLoadingRequest);
            _loadingRequest.Add(newLoadingRequest);
        }

        #endregion
    }
}
