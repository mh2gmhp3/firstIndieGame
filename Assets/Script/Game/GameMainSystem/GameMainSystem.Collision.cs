using CollisionModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        //TODO need to init?
        private CollisionAreaManager _collisionAreaManager = new CollisionAreaManager();

        #region Test

        private float _testRepeatCollisionAreaElapsedTime = 0;
        [SerializeField]
        private float _testRepeatCollisionAreaIntervalTime = 1;
        [SerializeField]
        private float _testRepeatCollisionAreaExistTime = 1;

        #endregion

        private void CreateTestCollisionArea(float duration)
        {
            var testCollisionAreaSetupData = new TestCollisionAreaSetupData(duration);
            testCollisionAreaSetupData.TriggerReceiver = new TestCollisionAreaTriggerReceiver();
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
    }
}
