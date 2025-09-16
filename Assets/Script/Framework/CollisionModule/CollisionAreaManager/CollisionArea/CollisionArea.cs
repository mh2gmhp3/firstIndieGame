using Logging;
using UnityEngine;

namespace CollisionModule
{
    /// <summary>
    /// 碰撞區域管理者
    /// </summary>
    public interface ICollisionAreaManager
    {
        /// <summary>
        /// 獲取碰撞群組Id與Collider自定義Id
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="groupId"></param>
        /// <param name="colliderId"></param>
        /// <returns></returns>
        public bool TryGetColliderId(int instanceId, out int groupId, out int colliderId);
    }

    /// <summary>
    /// 碰撞區域觸發接收者
    /// </summary>
    public interface ICollisionAreaTriggerReceiver
    {
        /// <summary>
        /// 碰撞觸發時
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="colliderId"></param>
        public void OnTrigger(int groupId, int colliderId);
    }

    /// <summary>
    /// 碰撞區域設置資料
    /// </summary>
    public interface ICollisionAreaSetupData
    {
        /// <summary>
        /// 區域類型
        /// </summary>
        public int AreaType { get; }

        /// <summary>
        /// 世界座標
        /// </summary>
        public Vector3 WorldPosition { get; set; }
        /// <summary>
        /// 方向
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// 持續時間
        /// </summary>
        public float TimeDuration { get; set; }
        /// <summary>
        /// 觸發接收者
        /// </summary>
        public ICollisionAreaTriggerReceiver TriggerReceiver { get; set; }
    }

    //先嘗試看看把碰撞後要觸發的行為在CollsionArea內處裡掉 只會有跟CollisionAreaManager碰撞單位的行為
    /// <summary>
    /// 碰撞區域
    /// </summary>
    public abstract class CollisionArea
    {
        //ICollisionAreaManager設定成private 對此的動作用protected包避免直接對此variable做操作
        private ICollisionAreaManager _collisionAreaManager = null;

        private int _areaType = 0;
        private int _id = 0;

        protected Vector3 _worldPosition;
        protected Vector3 _direction;

        protected float _startTime;
        protected float _endTime;
        protected float _timeDuration;

        protected ICollisionAreaTriggerReceiver _triggerReceiver;

        public int AreaType => _areaType;

        public int Id => _id;

        public bool IsEnd => Time.time >= _endTime;

        protected float ElapsedTime => Time.time - _startTime;

        protected float ProgressRate
        {
            get
            {
                return Mathf.Clamp01(ElapsedTime / _timeDuration);
            }
        }

        public CollisionArea(ICollisionAreaManager collisionAreaManager, int areaType)
        {
            _collisionAreaManager = collisionAreaManager;
            _areaType = areaType;
        }

        protected bool TryGetColliderId(int instanceId, out int groupId, out int colliderId)
        {
            return _collisionAreaManager.TryGetColliderId(instanceId, out groupId, out colliderId);
        }

        protected void NotifyTriggerReceiver(int groupId, int colliderId)
        {
            if (_triggerReceiver == null)
            {
                Log.LogWarning($"TriggerReceiver not set, notify failed, GroupId:{groupId} ColliderId:{colliderId}");
                return;
            }

            _triggerReceiver.OnTrigger(groupId, colliderId);
        }

        public void Setup(int id, ICollisionAreaSetupData setupData)
        {
            _id = id;

            _worldPosition = setupData.WorldPosition;
            _direction = setupData.Direction;

            _startTime = Time.time;
            _endTime = Time.time + setupData.TimeDuration;

            //強制限制一定要0.01秒持續時間
            _timeDuration = Mathf.Max(setupData.TimeDuration, 0.01f);

            _triggerReceiver = setupData.TriggerReceiver;

            DoSetup(setupData);
        }

        protected virtual void DoSetup(ICollisionAreaSetupData setupData) { }

        public virtual void DoCreate() { }
        public virtual void DoUpdate() { }
        public virtual void DoRecycle() { }
        public virtual void DoOnGUI() { }
        public virtual void DoDrawGizmos() { }
    }
}
