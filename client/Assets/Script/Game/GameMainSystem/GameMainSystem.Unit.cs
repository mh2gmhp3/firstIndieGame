using AssetModule;
using UnitModule;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private const string UnitRootPath = "Prototype/TestObject/UnitRoot";
        private UnitManager _unitManager = new UnitManager();

        public UnitManager UnitManager => _unitManager;

        public void InitUnitManager()
        {
            var unitRootAssets = AssetSystem.LoadAsset<GameObject>(UnitRootPath);
            var unitRoot = unitRootAssets.GetComponent<UnitSetting>();
            _unitManager.Init(unitRoot, _transform);
        }
    }
}
