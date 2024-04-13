using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneModule
{
    [GameSystem(GameSystemPriority.SCENE_SYSTEM)]
    public class SceneSystem : BaseGameSystem<SceneSystem>
    {
        public List<Type> a = new List<Type>();
    }
}
