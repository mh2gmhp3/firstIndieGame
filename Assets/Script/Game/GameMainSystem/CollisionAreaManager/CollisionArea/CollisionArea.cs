using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem.Collision
{
    public interface ICollisionAreaManager
    {
        public bool TryGetColliderId(int instanceId, out int groupId, out int colliderId);
    }

    public interface ICollisionAreaTriggerReceiver
    {
        public void OnTrigger(int groupId, int colliderId);
    }

    public interface ICollisionAreaSetupData
    {
        public int AreaType { get; }

        public Vector3 WorldPosition { get; set; }
        public Vector3 Direction { get; set; }

        public float TimeDuration { get; set; }

        public ICollisionAreaTriggerReceiver TriggerReceiver { get; set; }
    }

    //先嘗試看看把碰撞後要觸發的行為在CollsionArea內處裡掉 只會有跟CollisionAreaManager碰撞單位的行為
    public abstract class CollisionArea
    {
        //ICollisionAreaManager設定成private 對此的動作用protected包避免直接對此variable做操作
        private ICollisionAreaManager _collisionAreaManager = null;

        private int _areaType = 0;

        protected Vector3 _worldPosition;
        protected Vector3 _direction;

        protected float _startTime;
        protected float _endTime;
        protected float _timeDuration;

        protected ICollisionAreaTriggerReceiver _triggerReceiver;

        public int AreaType => _areaType;

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

        public void Setup(ICollisionAreaSetupData setupData)
        {
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
        public virtual void DoDrawGizmos() { }
    }
}
