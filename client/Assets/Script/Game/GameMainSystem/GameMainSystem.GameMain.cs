using DataModule;
using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UIModule.Game;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        /// <summary>
        /// 初始化進入遊戲流程
        /// </summary>
        public void InitGameMain()
        {
            SetCurGameState(GameState.UI);
            UISystem.OpenUIWindow(WindowId.Window_Main, null);
        }

        public static void StartNewGame()
        {
            Log.LogInfo("開始新遊戲");
            _instance.CreateTestData(1);
            _instance.InitGameTest();
        }

        public static void ContinueGame()
        {
            Log.LogInfo("繼續遊戲");
            if (!_instance._dataManager.Exist(1))
                return;
            _instance._dataManager.Load(1);
            _instance.InitGameTest();
        }

        public static void LoadGame()
        {
            Log.LogInfo("讀取遊戲存檔");
            if (!_instance._dataManager.Exist(1))
                return;
            _instance._dataManager.Load(1);
            _instance.InitGameTest();
        }

        public static void Setting()
        {
            Log.LogInfo("開啟遊戲設定頁面(共用)");
        }

        public static void Info()
        {
            Log.LogInfo("開啟遊戲資訊頁面");
        }

        public static void OpenCharacterMain()
        {
            UISystem.OpenUIWindow(WindowId.Window_CharacterMain, GetUICharacterData());
            SetCurGameState(GameState.UI);
        }

        public static void CloseCharacterMain()
        {
            UISystem.CloseUIWindow(WindowId.Window_CharacterMain);
            SetCurGameState(GameState.Normal);
        }

        public static void SaveCurData()
        {
            if (DataManager.CurDataId != -1)
            {
                DataManager.Save(DataManager.CurDataId);
            }
        }
    }
}