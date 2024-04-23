using CameraModule;
using GameSystem;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneModule
{
    [GameSystem(GameSystemPriority.SCENE_SYSTEM)]
    public class SceneSystem : BaseGameSystem<SceneSystem>
    {
        private ISceneBehavior _sceneBehavior;

        protected override bool DoInit()
        {
            SetSceneBehavior(new DefaultSceneBehavior());
            return true;
        }

        #region Public Static Method SetSceneBehavior

        public static void SetSceneBehavior(ISceneBehavior sceneBehavior)
        {
            _instance.DoSetSceneBehavior(sceneBehavior);
        }

        private void DoSetSceneBehavior(ISceneBehavior sceneBehavior)
        {
            if (sceneBehavior == null)
            {
                Log.LogError($"SetSceneBehavior, sceneBehavior is null");
                return;
            }

            _sceneBehavior = sceneBehavior;
        }

        #endregion

        #region Public Static Method SwitchScene

        public static void SwitchScene(ISwitchSceneData switchSceneData)
        {
            _instance.DoSwitchScene(switchSceneData);
        }

        private void DoSwitchScene(ISwitchSceneData switchSceneData)
        {
            _sceneBehavior.SwitchScene(switchSceneData);
        }

        #endregion

        #region Public Static Method SwitchStage

        public static void SwitchStage(ISwitchStageData switchStageData)
        {
            _instance.DoSwitchStage(switchStageData);
        }

        private void DoSwitchStage(ISwitchStageData switchStageData)
        {
            _sceneBehavior.SwitchStage(switchStageData);
        }

        #endregion
    }
}
