using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetModule
{
    public static partial class AssetPathUtility
    {
        private const string WeaponModelFolderPath = "Prototype/TestObject/Weapon";

        public static string GetWeaponModelPath(string modelName)
        {
            return Path.Combine(WeaponModelFolderPath, modelName);
        }
    }
}
