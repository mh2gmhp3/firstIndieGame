using CollisionModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private CollisionAreaManager _collisionAreaManager = new CollisionAreaManager();

        #region Test

        private float _testRepeatCollisionAreaElapsedTime = 0;
        [SerializeField]
        private float _testRepeatCollisionAreaIntervalTime = 1;
        [SerializeField]
        private float _testRepeatCollisionAreaExistTime = 1;

        #endregion

        #region Test

        private void CreateTestCollisionArea(float duration)
        {
            var testCollisionAreaSetupData = new TestCollisionAreaSetupData(duration);
            CollisionAreaManager.CreateCollisionArea(testCollisionAreaSetupData);
        }

        private void RepeatCreateTestCollisionArea()
        {
            _testRepeatCollisionAreaElapsedTime += Time.deltaTime;
            if (_testRepeatCollisionAreaElapsedTime < _testRepeatCollisionAreaIntervalTime)
                return;

            _testRepeatCollisionAreaElapsedTime = 0;
            CreateTestCollisionArea(_testRepeatCollisionAreaExistTime);
        }

        #endregion

        public void InitCollision()
        {
            _collisionAreaManager.SetCollisionAreaTriggerReceiver(new GameCollisionAreaTriggerReceiver());
        }
    }
}
