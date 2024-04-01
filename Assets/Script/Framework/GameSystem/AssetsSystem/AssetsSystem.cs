using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Framework.Assets
{
    [GameSystem(GameSystemPriority.ASSETS_SYSTEM)]
    public partial class AssetsSystem : BaseGameSystem<AssetsSystem>
    {
        public static T LoadAssets<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
    }
}
