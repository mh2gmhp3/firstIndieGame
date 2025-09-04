using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    [Serializable]
    public class AttackBehavior
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private float _lockNextBehaviorTime = 0.1f;
        [SerializeField]
        private float _behaviorTime = 0.5f;

        [SerializeField]
        private float _startElapsedTime = 0;

        public string Name => _name;

        public bool CanNextBehavior => _startElapsedTime >= _lockNextBehaviorTime;

        public bool IsEnd => _startElapsedTime > _behaviorTime;

        public AttackBehavior()
        {

        }

        public AttackBehavior(string name, float lockNextBehaviorTime, float behaviorTime)
        {
            _name = name;
            _lockNextBehaviorTime = lockNextBehaviorTime;
            _behaviorTime = behaviorTime;
        }

        public void OnStart()
        {
            Reset();
        }

        public void OnUpdate()
        {
            _startElapsedTime += Time.deltaTime;
        }

        public void OnEnd()
        {

        }

        public void Reset()
        {
            _startElapsedTime = 0;
        }
    }
}
