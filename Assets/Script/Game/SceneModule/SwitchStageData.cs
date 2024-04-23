using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneModule
{
    public class SwitchStageData : ISwitchStageData
    {
        public string StagePath;

        public SwitchStageData(string stagePath)
        {
            StagePath = stagePath;
        }
    }
}
