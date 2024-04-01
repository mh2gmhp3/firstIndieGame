using Logging;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.Framework
{
    /// <summary>
    /// 基礎系統
    /// </summary>
    public interface IBaseGameSystem
    {
        public void InitBaseGameSystem(GameSystemManager gameSystemManager);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>是否初始化完成</returns>
        public bool Init();

        /// <summary>
        /// 進入遊戲流程進入階段
        /// </summary>
        /// <param name="flowStep"></param>
        public void EnterGameFlowEnterStep(int flowStep);
        /// <summary>
        /// 進入遊戲流程處理階段
        /// </summary>
        /// <param name="flowStep"></param>
        public bool EnterGameFlowProcessStep(int flowStep);

        public void Update();
        public void FixedUpdate();
        public void LateUpdate();
    }

    /// <summary>
    /// 基礎系統實作
    /// </summary>
    public abstract class BaseGameSystem<T> : MonoBehaviour, IBaseGameSystem
        where T : BaseGameSystem<T>
    {
        protected GameSystemManager _gameSystemManager;

        protected static T _instance;
        protected Transform _transform;

        void IBaseGameSystem.InitBaseGameSystem(GameSystemManager gameSystemManager)
        {
            _gameSystemManager = gameSystemManager;
            _instance = this as T;
            _transform = this.transform;
        }

        bool IBaseGameSystem.Init()
        {
            return DoInit();
        }

        void IBaseGameSystem.EnterGameFlowEnterStep(int flowStep)
        {
            DoEnterGameFlowEnterStep(flowStep);
        }

        bool IBaseGameSystem.EnterGameFlowProcessStep(int flowStep)
        {
            return DoEnterGameFlowProcessStep(flowStep);
        }

        void IBaseGameSystem.Update()
        {
            DoUpdate();
        }

        void IBaseGameSystem.FixedUpdate()
        {
            DoFixedUpdate();
        }

        void IBaseGameSystem.LateUpdate()
        {
            DoLateUpdate();
        }

        protected virtual bool DoInit() { return true; }

        protected virtual void DoEnterGameFlowEnterStep(int flowStep) { }

        protected virtual bool DoEnterGameFlowProcessStep(int flowStep) { return true; }

        protected virtual void DoUpdate() { }

        protected virtual void DoFixedUpdate() { }

        protected virtual void DoLateUpdate() { }
    }
}
