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
    /// �U�t�κ޲z��
    /// �ݩ�C�����h��@�޲z�� ������XInterface
    /// </summary>
    public class SystemManager : MonoBehaviour
    {
        /// <summary>
        /// �t�ΦC��
        /// </summary>
        private List<IBaseSystem> _systemList = new List<IBaseSystem>();

        /// <summary>
        /// �O�_�Q��l�ƹL
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// ���骺Transform
        /// </summary>
        private Transform _transform;
        /// <summary>
        /// ���骺GameObject
        /// </summary>
        private GameObject _gameObject;

        /// <summary>
        /// SystemManager�ߤ@����
        /// </summary>
        private static SystemManager _instance = null;

        /// <summary>
        /// ��l�ƹ��� 
        /// �إ߳�@����SystemManager �ê�l��
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
        /// �إߩҦ���SystemAttribute��@���� �ê�l��
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

            //��o�Ҧ��㦳SystemAttribute��Type
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
