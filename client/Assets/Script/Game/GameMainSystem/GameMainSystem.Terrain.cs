using AssetModule;
using System.Collections;
using System.Collections.Generic;
using TerrainModule;
using UnitModule;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        public TerrainManager _terrainManager = new TerrainManager();

        public void InitTerrainManager()
        {
            _terrainManager.Init(_transform);
            RegisterUpdateTarget(_terrainManager);
        }

        public static void EnterTerrain(string name)
        {
            _instance._terrainManager.Enter(name);
        }
    }
}
