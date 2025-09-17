using Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace CollisionModule
{
    /// <summary>
    /// 碰撞區塊管理者
    /// <para>可註冊Collider來與其他的單位碰撞</para>
    /// </summary>
    public class CollisionAreaManager : ICollisionAreaManager
    {
        /// <summary>
        /// 要註冊的Collider資料
        /// </summary>
        public class RegisterColliderData
        {
            /// <summary>
            /// 自訂Id對註冊的Collider
            /// </summary>
            public Dictionary<int, Collider> IdToRegisterColliderDic =
               new Dictionary<int, Collider>();

            /// <summary>
            /// 是否有任何Collider
            /// </summary>
            public bool HaveCollider => IdToRegisterColliderDic.Count > 0;
        }

        /// <summary>
        /// Collider群組 註冊個Collider
        /// </summary>
        public class ColliderGroup
        {
            /// <summary>
            /// Id對Collider
            /// </summary>
            public Dictionary<int, Collider> IdToColliderDic =
               new Dictionary<int, Collider>();

            /// <summary>
            /// Unity InstanceId對應Collider 反查用
            /// </summary>
            private Dictionary<int, int> _instancIdToIdDic =
                new Dictionary<int, int>();

            /// <summary>
            /// 使用註冊資料填充
            /// </summary>
            /// <param name="colliderData"></param>
            public void FillColliders(RegisterColliderData colliderData)
            {
                IdToColliderDic.Clear();
                foreach (var idToCollider in colliderData.IdToRegisterColliderDic)
                {
                    int id = idToCollider.Key;
                    var collider = idToCollider.Value;
                    IdToColliderDic.Add(idToCollider.Key, collider);
                    _instancIdToIdDic.Add(collider.GetInstanceID(), id);
                }
            }

            /// <summary>
            /// 獲取Collider
            /// </summary>
            /// <param name="instanceId"></param>
            /// <param name="colliderId"></param>
            /// <returns></returns>
            public bool TryGetColliderId(int instanceId, out int colliderId)
            {
                return _instancIdToIdDic.TryGetValue(instanceId, out colliderId);
            }
        }

        #region Registered Collider

        /// <summary>
        /// 下一個ColliderGroupId
        /// </summary>
        private int _nextGroupId = 0;

        /// <summary>
        /// GroupId對ColliderGroup
        /// </summary>
        private Dictionary<int, ColliderGroup> _groupIdToColliderGroupDic =
            new Dictionary<int, ColliderGroup>();

        /// <summary>
        /// Unity InstanceId對GroupId 反查用
        /// </summary>
        private Dictionary<int, int> _instancIdToGroupIdDic =
            new Dictionary<int, int>();

        #endregion

        #region CollisionArea

        /// <summary>
        /// 下一個CollisionAreaId
        /// </summary>
        private int _nextAreaId = 0;

        //TODO 先評估看看Activator.CreateInstance在創建時的消耗 因為有Pool基本上也不會太多次 如果真的有消耗嘗試改用已經實例化出來的做Copy
        /// <summary>
        /// 各類型CollisionAreae的Type
        /// </summary>
        private Dictionary<int, Type> _areaTypeToAreaClassTypeDic =
            new Dictionary<int, Type>();

        /// <summary>
        /// 各類型CollisionAreae的Pool
        /// </summary>
        private Dictionary<int, Queue<CollisionArea>> _areaTypeToAreaPoolDic =
            new Dictionary<int, Queue<CollisionArea>>();

        /// <summary>
        /// 運行中的CollisionArea
        /// </summary>
        private List<CollisionArea> _runningCollisionAreaList =
            new List<CollisionArea>();

        /// <summary>
        /// 不再使用的CollisionArea
        /// </summary>
        private List<CollisionArea> _endCollisionAreaList =
            new List<CollisionArea>();

        private ICollisionAreaTriggerReceiver _collisionAreaTriggerReceiver;

        #endregion

        private static CollisionAreaManager _instance = null;

        public CollisionAreaManager()
        {
            _instance = this;
            CollectionCollisionAreaType();
        }

        #region Public Method

        public void SetCollisionAreaTriggerReceiver(ICollisionAreaTriggerReceiver collisionAreaTriggerReceiver)
        {
            _collisionAreaTriggerReceiver = collisionAreaTriggerReceiver;
        }

        #endregion

        #region Pubilc Unity Method  Update DrawGizmos

        /// <summary>
        /// Update時需要自行呼叫 不呼叫CollisionArea將不會更新
        /// </summary>
        public void DoUpdate()
        {
            ProcessCollisionArea();
        }

        /// <summary>
        /// 需要繪製GUI時呼叫
        /// </summary>
        public void DoOnGUI()
        {
            for (int i = 0; i < _runningCollisionAreaList.Count; i++)
            {
                _runningCollisionAreaList[i].DoOnGUI();
            }
        }

        /// <summary>
        /// 需要繪製Gizmos時呼叫
        /// </summary>
        public void DoDrawGizmos()
        {
            for (int i = 0; i < _runningCollisionAreaList.Count; i++)
            {
                _runningCollisionAreaList[i].DoDrawGizmos();
            }
        }

        #endregion

        #region Public Static Method RegisterCollider UnregisterCollider

        /// <summary>
        /// 註冊Collider
        /// </summary>
        /// <param name="colliderData"></param>
        /// <returns></returns>
        public static int RegisterCollider(RegisterColliderData colliderData)
        {
            if (_instance == null)
            {
                return 0;
            }

            return _instance.InternalRegisterCollider(colliderData);
        }

        /// <summary>
        /// 反註冊Collider
        /// </summary>
        /// <param name="groupId"></param>
        public static void UnregisterCollider(int groupId)
        {
            if (_instance == null)
            {
                return;
            }

            _instance.RemoveCollider(groupId);
        }

        #endregion

        #region Public Static Method CreateCollisionArea

        /// <summary>
        /// 創建碰撞區域
        /// </summary>
        /// <param name="setupData"></param>
        public static int CreateCollisionArea(ICollisionAreaSetupData setupData)
        {
            if (_instance == null)
            {
                return 0;
            }
            return _instance.InternalCreateCollisionArea(setupData);
        }

        /// <summary>
        /// 移除碰撞區域
        /// </summary>
        /// <param name="setupData"></param>
        public static void RemoveCollisionArea(int areaId)
        {
            if (_instance == null)
            {
                return;
            }
            _instance.InternalRemoveCollisionArea(areaId);
        }

        #endregion

        #region Imp ICollisionAreaManager

        bool ICollisionAreaManager.TryGetColliderId(int instanceId, out int groupId, out int colliderId)
        {
            colliderId = 0;
            if (!_instancIdToGroupIdDic.TryGetValue(instanceId, out groupId))
                return false;

            if (!_groupIdToColliderGroupDic.TryGetValue(groupId, out var group))
                return false;

            return group.TryGetColliderId(instanceId, out colliderId);
        }

        void ICollisionAreaManager.NotifyTriggerReceiver(int groupId, int colliderId, ICollisionAreaTriggerInfo triggerInfo)
        {
            if (_collisionAreaTriggerReceiver == null)
                return;

            _collisionAreaTriggerReceiver.OnTrigger(groupId, colliderId, triggerInfo);
        }

        #endregion

        #region Private Method ColliderGroup

        private int GetNextGroupId()
        {
            return ++_nextGroupId;
        }

        private int InternalRegisterCollider(RegisterColliderData colliderData)
        {
            if (colliderData == null)
            {
                Log.LogError("RegisterColliderData is null", true);
                return 0;
            }

            if (!colliderData.HaveCollider)
            {
                Log.LogWarning("RegisterColliderData Collider is empty", true);
                return 0;
            }

            if (!CheckColliderIsValid(colliderData, out string msg))
            {
                Log.LogWarning($"RegisterColliderData Invalid Msg:\n{msg}", true);
                return 0;
            }

            int newGroupId = GetNextGroupId();
            AddCollider(newGroupId, colliderData);

            return newGroupId;
        }

        private bool CheckColliderIsValid(RegisterColliderData colliderData, out string msg)
        {
            msg = string.Empty;
            foreach (var idToCollider in colliderData.IdToRegisterColliderDic)
            {
                int id = idToCollider.Key;
                var collider = idToCollider.Value;
                if (collider == null)
                {
                    msg += $"Id:{idToCollider.Key} Collider is null\n";
                    continue;
                }

                int colliderInsId = collider.GetInstanceID();
                if (_instancIdToGroupIdDic.ContainsKey(colliderInsId))
                {
                    msg += $"Id:{idToCollider.Key} Collider is exist InsId:{colliderInsId}\n";
                    continue;
                }
            }

            return string.IsNullOrEmpty(msg);
        }

        private void AddCollider(int groupId, RegisterColliderData colliderData)
        {
            //groupId基本上是遞增的 照正常來說不可能有重複的 必免內部使用錯誤先直接return
            if (_groupIdToColliderGroupDic.ContainsKey(groupId))
            {
                Log.LogError("GroupId duplicate");
                return;
            }

            //TODO 先不用Pool 雖然在怪物大量生成時有機會導致連續new的狀況 但先不處理
            var newColliderGroup = new ColliderGroup();
            newColliderGroup.FillColliders(colliderData);

            foreach (var collider in colliderData.IdToRegisterColliderDic.Values)
            {
                if (collider == null)
                {
                    Log.LogWarning($"GroupId:{groupId} have null collider");
                    continue;
                }
                _instancIdToGroupIdDic.Add(collider.GetInstanceID(), groupId);
            }

            _groupIdToColliderGroupDic.Add(groupId, newColliderGroup);
        }

        private void RemoveCollider(int groupId)
        {
            if (!_groupIdToColliderGroupDic.TryGetValue(groupId, out var colliderGroup))
            {
                Log.LogError("GroupId not exist");
                return;
            }

            foreach (var collider in colliderGroup.IdToColliderDic.Values)
            {
                if (collider == null)
                {
                    Log.LogWarning($"GroupId:{groupId} have null collider");
                    continue;
                }
                _instancIdToGroupIdDic.Remove(collider.GetInstanceID());
            }

            _groupIdToColliderGroupDic.Remove(groupId);
        }

        #endregion

        #region Private Method CollisionArea

        private void CollectionCollisionAreaType()
        {
            var collisionAreaTypeList = AttributeUtility.GetAllAtttibuteTypeList<CollisionAreaAttribute>();
            for (int i = 0; i < collisionAreaTypeList.Count; i++)
            {
                int areaType = collisionAreaTypeList[i].Attribute.AreaType;
                var classType = collisionAreaTypeList[i].Type;

                if (_areaTypeToAreaClassTypeDic.ContainsKey(areaType))
                {
                    Log.LogWarning($"AreaType:{areaType} have duplicates type:{classType.Name} ignore this type", true);
                    continue;
                }

                _areaTypeToAreaClassTypeDic.Add(areaType, classType);
            }
        }

        private int GetNextAreaId()
        {
            return ++_nextAreaId;
        }

        private int InternalCreateCollisionArea(ICollisionAreaSetupData setupData)
        {
            if (setupData == null)
            {
                Log.LogWarning("SetupData is null, do not create CollsionArea");
                return 0;
            }

            if (!TryGetCollisionArea(setupData.AreaType, out var collisionArea))
            {
                Log.LogWarning("SetupData Invalid, get collisionArea failed");
                return 0;
            }

            var nextAreaId = GetNextAreaId();
            collisionArea.Setup(nextAreaId, setupData);
            collisionArea.DoCreate();
            _runningCollisionAreaList.Add(collisionArea);
            return nextAreaId;
        }

        private void InternalRemoveCollisionArea(int areaId)
        {
            CollisionArea removeArea = null;
            for (int i = 0; i < _runningCollisionAreaList.Count; i++)
            {
                var runningCollisionArea = _runningCollisionAreaList[i];
                if (runningCollisionArea.Id != areaId)
                    continue;

                _runningCollisionAreaList.RemoveAt(i);
                removeArea = runningCollisionArea;
                break;
            }

            if (removeArea == null)
                return;

            _endCollisionAreaList.Add(removeArea);
        }

        private void ProcessCollisionArea()
        {
            for (int i = 0; i < _runningCollisionAreaList.Count; i++)
            {
                var runningCollisionArea = _runningCollisionAreaList[i];
                runningCollisionArea.DoUpdate();
                if (runningCollisionArea.IsEnd)
                    _endCollisionAreaList.Add(runningCollisionArea);
            }

            int endCount = _endCollisionAreaList.Count;
            if (endCount > 0)
            {
                for (int i = 0; i < endCount; i++)
                {
                    var endCollisionArea = _endCollisionAreaList[i];
                    _runningCollisionAreaList.Remove(endCollisionArea);
                    RecycleCollisionArea(endCollisionArea);
                }
                _endCollisionAreaList.Clear();
            }
        }

        private bool TryGetCollisionArea(int areaType, out CollisionArea collisionArea)
        {
            if (_areaTypeToAreaPoolDic.TryGetValue(areaType, out var pool) &&
                pool.Count > 0)
            {
                collisionArea =  pool.Dequeue();
                return true;
            }

            if (!_areaTypeToAreaClassTypeDic.TryGetValue(areaType, out var classType))
            {
                collisionArea = null;
                Log.LogError($"AreaType:{areaType} ClassType not exist");
                return false;
            }

            collisionArea = Activator.CreateInstance(classType, this, areaType) as CollisionArea;
            return true;
        }

        private void RecycleCollisionArea(CollisionArea collisionArea)
        {
            if (collisionArea == null)
            {
                Log.LogWarning("collisionArea is null, do not recycle CollsionArea");
                return;
            }

            int areaType = collisionArea.AreaType;
            if (!_areaTypeToAreaPoolDic.TryGetValue(areaType, out var pool))
            {
                pool = new Queue<CollisionArea>();
                _areaTypeToAreaPoolDic.Add(areaType, pool);
            }

            collisionArea.DoRecycle();
            pool.Enqueue(collisionArea);
        }

        #endregion
    }
}
