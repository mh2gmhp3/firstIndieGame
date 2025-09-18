using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneModule
{
    public abstract class BaseSceneBehavior : ISceneBehavior
    {
        public virtual void SwitchScene(ISwitchSceneData switchSceneData)
        {

        }

        public virtual void SwitchStage(ISwitchStageData switchStageData)
        {

        }
    }
}
