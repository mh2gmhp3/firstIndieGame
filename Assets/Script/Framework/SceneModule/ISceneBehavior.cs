using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneModule
{
    public interface ISceneBehavior
    {
        public void SwitchScene(ISwitchSceneData switchSceneData);
        public void SwitchStage(ISwitchStageData switchStageData);
    }
}
