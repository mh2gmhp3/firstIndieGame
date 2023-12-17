using Framework.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entry
{
    public class GameEntry : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            SystemManager.InitInstance();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
