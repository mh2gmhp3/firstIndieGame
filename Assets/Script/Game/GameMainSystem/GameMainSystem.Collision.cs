using GameMainSystem.Collision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
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

        private void CreateTestCollisionArea(float direction)
        {
            var testCollisionAreaSetupData = new TestCollisionAreaSetupData(direction);
            testCollisionAreaSetupData.TriggerReceiver = new TestCollisionAreaTriggerReceiver();
            _collisionAreaManager.CreateCollisionArea(testCollisionAreaSetupData);
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
