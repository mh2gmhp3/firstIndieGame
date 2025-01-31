using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Collision
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollisionAreaAttribute : Attribute
    {
        public int AreaType;

        public CollisionAreaAttribute(int areaType)
        {
            AreaType = areaType;
        }
    }
}