using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entry
{
    public class GameEntry : MonoBehaviour
    {
        void Start()
        {
            GameSystemManager.InitInstance();
        }
    }
}
