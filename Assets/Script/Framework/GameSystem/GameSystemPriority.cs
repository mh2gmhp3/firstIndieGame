using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// 各系統修先權 TODO 如果很長改要抽成Setting避免每次Compiler都要跑
    /// </summary>
    public static partial class GameSystemPriority
    {
        public const int ASSET_SYSTEM = 0;
        public const int INPUT_SYSTEM = 1;
        public const int UI_SYSTEM = 2;
        public const int CAMERA_SYSTEM = 3;
        public const int SCENE_SYSTEM = 4;
        public const int FORM_SYSTEM = 5;
    }
}
