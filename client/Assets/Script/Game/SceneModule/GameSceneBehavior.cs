using AssetModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace SceneModule
{
    public class GameSceneBehavior : BaseSceneBehavior
    {
        private string STAGE_FOLDER_PATH = "Stage/";

        public override void SwitchStage(ISwitchStageData switchStageData)
        {
            if (!(switchStageData is SwitchStageData stageData))
            {
                Log.LogError("switchStageData is invalid");
                return;
            }

            //TODO to base and notify switch flow to observer
            var stageTemp = AssetSystem.LoadAsset<GameObject>(STAGE_FOLDER_PATH + stageData.StagePath);
            GameObject stageGo = ObjectUtility.InstantiateWithoutClone(stageTemp);
        }
    }
}
