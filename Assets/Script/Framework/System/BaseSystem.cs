using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.System
{
    /// <summary>
    /// ��¦�t��
    /// </summary>
    public interface IBaseSystem
    {
        public void Init();
        public void Update();
    }

    /// <summary>
    /// ��¦�t�ι�@
    /// </summary>
    public abstract class BaseSystem : MonoBehaviour, IBaseSystem
    {
        void IBaseSystem.Init()
        {
            DoInit();
        }

        void IBaseSystem.Update()
        {
            DoUpdate();
        }

        protected virtual void DoInit() { }

        protected virtual void DoUpdate() { }
    }
}
