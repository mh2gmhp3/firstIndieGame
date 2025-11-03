using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule
{
    public class UnitSetting : MonoBehaviour
    {
        /// <summary>
        /// 移動用的Transform Root
        /// </summary>
        public Transform RootTransform;
        /// <summary>
        /// 旋轉用的Transform
        /// </summary>
        public Transform RotateTransform;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody;
    }
}
