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
        public void Init();
        public void Update();
    }

    /// <summary>
    /// 基礎系統實作
    /// </summary>
    public abstract class BaseGameSystem : MonoBehaviour, IBaseGameSystem
    {
        void IBaseGameSystem.Init()
        {
            DoInit();
        }

        void IBaseGameSystem.Update()
        {
            DoUpdate();
        }

        protected virtual void DoInit() { }

        protected virtual void DoUpdate() { }
    }
}
