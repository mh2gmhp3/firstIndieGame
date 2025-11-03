using AssetModule;
using Extension;
using Logging;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace UnitModule
{
    public class UnitAvatarManager
    {
        public class UnitAvatarInstance
        {
            public string AvatarName { get; }
            public UnitSetting UnitSetting { get; }
            public UnitAvatarSetting UnitAvatarSetting { get; }

            public UnitAvatarInstance(string avatarName, UnitSetting unitSetting, UnitAvatarSetting unitAvatarSetting)
            {
                AvatarName = avatarName;
                UnitSetting = unitSetting;
                UnitAvatarSetting = unitAvatarSetting;
            }
        }

        private Vector3 _pooledPosition = new Vector3(5000, 5000, 5000);

        //Pool Unit
        private Dictionary<string, Queue<UnitAvatarInstance>> _nameToAvatarInsPool = new Dictionary<string, Queue<UnitAvatarInstance>>();

        //Using
        private Dictionary<int, UnitAvatarInstance> _idToUsingAvatarInsDic = new Dictionary<int, UnitAvatarInstance>();

        private Transform _unitRootTrans;
        private UnitSetting _unitRootTemplate;

        private string _avatarFolderPath;

        public void Init(UnitSetting unitRootTemplate, Transform parent, string avatarFolderPath)
        {
            _unitRootTemplate = unitRootTemplate;
            var unitManagerRoot = new GameObject("UnitAvatarManager");
            _unitRootTrans = unitManagerRoot.transform;
            _unitRootTrans.SetParent(parent);
            _unitRootTrans.Reset();
            _avatarFolderPath = avatarFolderPath;
        }

        #region Register

        /// <summary>
        /// 註冊Avatar使用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="avatarName"></param>
        /// <param name="avatarInstance"></param>
        /// <returns></returns>
        public bool RegisterAvatar(int id, string avatarName, out UnitAvatarInstance avatarInstance)
        {
            avatarInstance = default;
            if (_idToUsingAvatarInsDic.ContainsKey(id))
            {
                Log.LogWarning($"UnitAvatarManager.RegisterAvatar Warning, 此Id已註冊Avatar, 如需替換需先UnregisterAvatar Id:{id}");
                return false;
            }

            if (!TryGetInsFromPool(avatarName, out avatarInstance))
            {
                Log.LogError($"UnitAvatarManager.RegisterAvatar Error, 無法獲取AvatarInstance Id:{id} AvatarName:{avatarName}");
                return false;
            }

#if UNITY_EDITOR
            avatarInstance.UnitSetting.RootTransform.name = $"{avatarName}_{id}";
#endif
            _idToUsingAvatarInsDic.Add(id, avatarInstance);
            return true;
        }

        /// <summary>
        /// 反註冊Avatar使用
        /// </summary>
        /// <param name="id"></param>
        public void UnregisterAvatar(int id)
        {
            if (!_idToUsingAvatarInsDic.TryGetValue(id, out var avatarInstance))
                return;

            ReturnPool(avatarInstance);
            _idToUsingAvatarInsDic.Remove(id);
        }

        #endregion

        #region Pool

        private bool TryGetInsFromPool(string avatarName, out UnitAvatarInstance avatarInstance)
        {
            avatarInstance = default;
            if (_nameToAvatarInsPool.TryGetValue(avatarName, out var pool) && pool.Count > 0)
            {
                avatarInstance = pool.Dequeue();
                return true;
            }
            else
            {
                //可能可以Delay
                var avatarPath = Path.Combine(_avatarFolderPath, avatarName);
                var avatarAsset = AssetSystem.LoadAsset<GameObject>(avatarPath);
                if (avatarAsset == null)
                {
                    Log.LogError($"UnitAvatarManager.TryGetInsFromPool 找不到AvatarAsset Path:{avatarPath}");
                    return false;
                }
                var avatarObject = ObjectUtility.InstantiateWithoutClone(avatarAsset);
                var avatarSetting = avatarObject.GetComponent<UnitAvatarSetting>();
                if (avatarSetting == null)
                {
                    Object.Destroy(avatarObject);
                    Log.LogError($"UnitAvatarManager.TryGetInsFromPool Avatar上不存在UnitAvatarRootSetting的Component Path:{avatarPath}");
                    return false;
                }

                var unitRootSetting = ObjectUtility.InstantiateWithoutClone(_unitRootTemplate, _unitRootTrans);
                if (unitRootSetting == null)
                {
                    Log.LogError($"UnitAvatarManager.TryGetInsFromPool UnitRootSetting 生成失敗");
                    return false;
                }

                unitRootSetting.RootTransform.Reset();
                avatarSetting.RootTransform.SetParent(unitRootSetting.RotateTransform);
                avatarInstance = new UnitAvatarInstance(avatarName, unitRootSetting, avatarSetting);
                return true;
            }
        }

        private void ReturnPool(UnitAvatarInstance avatarInstance)
        {
            if (avatarInstance == null)
                return;

            if (!_nameToAvatarInsPool.TryGetValue(avatarInstance.AvatarName, out var pool))
            {
                pool = new Queue<UnitAvatarInstance>();
                _nameToAvatarInsPool.Add(avatarInstance.AvatarName, pool);
            }

            pool.Enqueue(avatarInstance);

            //to pooled position
            avatarInstance.UnitSetting.RootTransform.position = _pooledPosition;
        }

        #endregion

        #region TryGetAvatarIns

        public bool TryGetAvatarIns(int id, out UnitAvatarInstance avatarInstance)
        {
            return _idToUsingAvatarInsDic.TryGetValue(id, out avatarInstance);
        }

        #endregion
    }
}
