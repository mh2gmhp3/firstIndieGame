using Framework.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Framework.Logging;

namespace Framework.System
{
    /// <summary>
    /// 各系統管理者
    /// 屬於遊戲底層單一管理者 先不抽出Interface
    /// </summary>
    public class SystemManager : MonoBehaviour
    {
        /// <summary>
        /// 系統列表
        /// </summary>
        private List<IBaseSystem> _systemList = new List<IBaseSystem>();

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
        /// SystemManager唯一實體
        /// </summary>
        private static SystemManager _instance = null;

        /// <summary>
        /// 初始化實體 
        /// 建立單一實體SystemManager 並初始化
        /// </summary>
        public static void InitInstance()
        {
            GameObject systemManagerGo = new GameObject("SystemManager");
            SystemManager systemManager = systemManagerGo.AddComponent<SystemManager>();
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
            _systemList = GenAllSystem();
        }

        private List<IBaseSystem> GenAllSystem()
        {
            List<IBaseSystem> result = new List<IBaseSystem>();

            //獲得所有具有SystemAttribute的Type
            List<(Type Type, GameSystemAttribute Attribute)> systemInfoList = new List<(Type type, GameSystemAttribute attribute)>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    GameSystemAttribute systemAttribute = type.GetCustomAttribute<GameSystemAttribute>(true);
                    if (systemAttribute == null)
                        continue;

                    systemInfoList.Add((type, systemAttribute));
                }
            }

            foreach (var systemInfo in systemInfoList)
            {
                string systemName = systemInfo.Attribute.Name;
                GameObject systemGo = new GameObject(systemName);
                Component systemComponent = systemGo.AddComponent(systemInfo.Type);
                systemGo.transform.SetParent(_transform);
                if (systemComponent is IBaseSystem baseSystem)
                {
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
    }
}
