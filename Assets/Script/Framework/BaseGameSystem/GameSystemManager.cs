using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logging;
using Utility;

namespace GameSystem
{
    /// <summary>
    /// 各系統管理者
    /// 屬於遊戲底層單一管理者 先不抽出Interface
    /// </summary>
    public partial class GameSystemManager : MonoBehaviour
    {
        public enum GameState
        {
            None                    = 0,
            Initialize              = 1,
            ProcessingEnterGameStep = 2,
            GameUpdate              = 3,
        }

        /// <summary>
        /// 系統列表
        /// </summary>
        private List<IBaseGameSystem> _gameSystemList = new List<IBaseGameSystem>();

        /// <summary>
        /// 當前狀態
        /// </summary>
        private GameState _state = GameState.None;

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
                    baseSystem.InitBaseGameSystem(this);
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
            SetState(GameState.Initialize);
            Log.LogInfo("InitAllGameSystem");
        }

        private void OniIitialized()
        {
            _enterGameFlowStepProcessor.SetActive(true);
            SetState(GameState.ProcessingEnterGameStep);
            Log.LogInfo("OniIitialized");
        }

        private void OnEnterGameFlowStepFinish()
        {
            _enterGameFlowStepProcessor.SetActive(false);
            _updateProcessor.SetActice(true);
            SetState(GameState.GameUpdate);
            Log.LogInfo("OnEnterGameFlowStepFinish");
        }

        private void Update()
        {
            if (IsState(GameState.Initialize))
            {
                bool finish = _initProcessor.ProcessInit();
                if (finish)
                    OniIitialized();
                return;
            }

            if (IsState(GameState.ProcessingEnterGameStep))
            {
                bool finish = _enterGameFlowStepProcessor.ProcessEnterGameFlowStep();
                if (finish)
                    OnEnterGameFlowStepFinish();
                return;
            }

            _updateProcessor.ProcessUpdate();
        }

        private void FixedUpdate()
        {
            if (!IsState(GameState.GameUpdate))
                return;

            _updateProcessor.ProcessFixedUpdate();
        }

        private void LateUpdate()
        {
            if (!IsState(GameState.GameUpdate))
                return;

            _updateProcessor.ProcessLateUpdate();
        }

        #region State

        private void SetState(GameState state)
        {
            _state = state;
        }

        public bool IsState(GameState state)
        {
            return _state == state;
        }

        #endregion
    }
}
