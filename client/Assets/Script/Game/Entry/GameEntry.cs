using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entry
{
    public class GameEntry : MonoBehaviour
    {
        public bool TestMode = false;

        void Start()
        {
            GameSystemManager.InitInstance(TestMode);
        }
    }
}
