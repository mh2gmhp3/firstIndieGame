using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.GameSystem.Assets
{
    [GameSystem(GameSystemPriority.ASSETS_SYSTEM)]
    public partial class AssetsSystem : BaseGameSystem
    {
        public static T LoadAssets<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
    }
}
