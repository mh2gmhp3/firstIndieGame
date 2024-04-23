using SceneModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    public partial class GameMainSystem
    {
        private void InitGameScene()
        {
            SceneSystem.SetSceneBehavior(new GameSceneBehavior());
        }
    }
}
