using Framework.GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Framework.Logging;
using Framework.Utility;

namespace Framework.GameSystem
{
    /// <summary>
    /// 各系統管理者
    /// 屬於遊戲底層單一管理者 先不抽出Interface
    /// </summary>
    public partial class GameSystemManager : MonoBehaviour
    {
        /// <summary>
        /// 系統列表
        /// </summary>
        private List<IBaseGameSystem> _gameSystemList = new List<IBaseGameSystem>();

        /// <summary>
        /// 是否被初始化過
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// 實體的Transform
        /// </summary>
        private Transform _transform;
        /// <summary>
        /// 實體的GameObject
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// 初始化各系統處理者
        /// </summary>
        private GameSystemInitProcessor _initProcessor;

        /// <summary>
        /// Update各系統處理者
        /// </summary>
        private GameSystemUpdateProcessor _updateProcessor;

        /// <summary>
        /// 進入遊戲流程處理者
        /// </summary>
        private GameSystemEnterGameFlowStepProcessor _enterGameFlowStepProcessor;

        /// <summary>
        /// GameSystemManager唯一實體
        /// </summary>
        private static GameSystemManager _instance = null;

        /// <summary>
        /// 初始化實體
        /// 建立單一實體GameSystemManager 並初始化
        /// </summary>
        public static void InitInstance()
        {
            GameObject systemManagerGo = new GameObject("GameSystemManager");
            GameSystemManager systemManager = systemManagerGo.AddComponent<GameSystemManager>();
            _instance = systemManager;
            DontDestroyOnLoad(_instance);
            _instance.Init();
        }

        /// <summary>
        /// 建立所有具SystemAttribute單一實體 並初始化
        /// </summary>
        private void Init()
        {
            _transform = transform;
            _gameObject = gameObject;
            _gameSystemList = GenAllGameSystem();
            InitAllGameSystem();
            InitAllProcessor();
        }

        /// <summary>
        /// 生成所有GameSystem
        /// </summary>
        /// <returns></returns>
        private List<IBaseGameSystem> GenAllGameSystem()
        {
            List<IBaseGameSystem> result = new List<IBaseGameSystem>();

            //獲得所有具有GameSystemAttribute的Type
            List<(Type Type, GameSystemAttribute Attribute)> systemInfoList =
                AttributeUtility.GetAllAtttibuteTypeList<GameSystemAttribute>();

            //照優限度排序
            systemInfoList.Sort(
                (x, y) =>
                {
                    return x.Attribute.Priority.CompareTo(y.Attribute.Priority);
                });

            //生成所有System
            foreach (var systemInfo in systemInfoList)
            {
                string systemName = systemInfo.Type.Name;
                GameObject systemGo = new GameObject(systemName);
                Component systemComponent = systemGo.AddComponent(systemInfo.Type);
                systemGo.transform.SetParent(_transform);
                if (systemComponent is IBaseGameSystem baseSystem)
                {
                    baseSystem.SetManager(this);
                    result.Add(baseSystem);
                    Log.LogInfo("System Instance Success, System Name : " + systemName);
                }
                else
                {
                    Log.LogWarning("System Instance Failed system is not IBaseSystem!, System Name :" + systemName);
                }
            }

            return result;
        }

        private void InitAllProcessor()
        {
            _initProcessor = new GameSystemInitProcessor(_gameSystemList);
            _updateProcessor = new GameSystemUpdateProcessor(_gameSystemList);
            _enterGameFlowStepProcessor = new GameSystemEnterGameFlowStepProcessor(_gameSystemList);
        }

        private void InitAllGameSystem()
        {
            _initialized = false;
        }

        private void OniIitialized()
        {
            _enterGameFlowStepProcessor.SetActive(true);
            _updateProcessor.SetActice(true);
        }

        private void Update()
        {
            if (!_initialized)
            {
                _initialized = _initProcessor.ProcessInit();
                if (_initialized)
                    OniIitialized();
                return;
            }

            _enterGameFlowStepProcessor.ProcessEnterGameFlowStep();
            _updateProcessor.ProcessUpdate();
        }

        private void FixedUpdate()
        {
            _updateProcessor.ProcessFixedUpdate();
        }

        private void LateUpdate()
        {
            _updateProcessor.ProcessLateUpdate();
        }
    }
}
