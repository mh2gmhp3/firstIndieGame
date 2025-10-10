using DataModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private DataManager _dataManager;
        private static DataManager DataManager => _instance._dataManager;

        private void InitGameData()
        {
            _dataManager = new DataManager();
            _dataManager.Init();
            _dataManager.LoadGlobal();
        }

        #region AttackBehavior

        public static List<AttackBehaviorData> GetAttackBehaviorDataList()
        {
            var repo = DataManager.GetDataRepository<AttackBehaviorDataRepository>();
            if (repo == null)
                return null;

            return repo.GetAttackBehaviorDataList();
        }

        #endregion
    }
}
