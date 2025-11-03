using CollisionModule;
using GameSystem;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private class TestCollisionAreaProcessor : IUpdateTarget
        {
            private float _testRepeatCollisionAreaElapsedTime = 0;
            [SerializeField]
            private float _testRepeatCollisionAreaIntervalTime = 1;
            [SerializeField]
            private float _testRepeatCollisionAreaExistTime = 1;

            void IUpdateTarget.DoUpdate()
            {
                RepeatCreateTestCollisionArea();
            }

            void IUpdateTarget.DoFixedUpdate()
            {

            }

            void IUpdateTarget.DoLateUpdate()
            {

            }

            void IUpdateTarget.DoOnGUI()
            {

            }

            void IUpdateTarget.DoDrawGizmos()
            {

            }

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
        }

        private CollisionAreaManager _collisionAreaManager = new CollisionAreaManager();

        public void InitCollision()
        {
            _collisionAreaManager.SetCollisionAreaTriggerReceiver(new GameCollisionAreaTriggerReceiver());
            RegisterUpdateTarget(_collisionAreaManager);
            if (TestMode)
            {
                RegisterUpdateTarget(new TestCollisionAreaProcessor());
            }
        }
    }
}
