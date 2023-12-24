using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.GameSystem
{
    /// <summary>
    /// 基礎系統
    /// </summary>
    public interface IBaseGameSystem
    {
        public void SetManager(GameSystemManager gameSystemManager);
        public bool Init();
        public void Update();
    }

    /// <summary>
    /// 基礎系統實作
    /// </summary>
    public abstract class BaseGameSystem : MonoBehaviour, IBaseGameSystem
    {
        protected GameSystemManager _gameSystemManager;

        void IBaseGameSystem.SetManager(GameSystemManager gameSystemManager)
        {
            _gameSystemManager = gameSystemManager;
        }

        bool IBaseGameSystem.Init()
        {
            return DoInit();
        }

        void IBaseGameSystem.Update()
        {
            DoUpdate();
        }

        protected virtual bool DoInit() { return true; }

        protected virtual void DoUpdate() { }
    }
}
