using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem.Collision
{
    public interface ICollisionAreaManager
    {
        public bool TryGetColliderId(int instanceId, out int colliderId);
    }

    public interface ICollisionAreaTriggerReceiver
    {
        public void OnTrigger();
    }

    public interface ICollisionAreaSetupData
    {
        public int AreaType { get; }

        public ICollisionAreaTriggerReceiver TriggerReceiver { get; }
    }

    //先嘗試看看把碰撞後要觸發的行為在CollsionArea內處裡掉 只會有跟CollisionAreaManager碰撞單位的行為
    public abstract class CollsionArea
    {
        //ICollisionAreaManager設定成private 對此的動作用protected包避免直接對此variable做操作
        private ICollisionAreaManager _collisionAreaManager = null;

        public CollsionArea(ICollisionAreaManager collisionAreaManager)
        {
            _collisionAreaManager = collisionAreaManager;
        }

        protected bool TryGetColliderId(int instanceId, out int colliderId)
        {
            return _collisionAreaManager.TryGetColliderId(instanceId, out colliderId);
        }

        public abstract void Setup(ICollisionAreaSetupData setupData);

        public virtual void OnCreate() { }
        public virtual void OnUpdate() { }
        public virtual void OnDestroy() { }
    }
}
