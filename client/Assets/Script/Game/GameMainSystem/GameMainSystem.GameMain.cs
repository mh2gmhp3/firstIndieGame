using Logging;
using System.Collections;
using System.Collections.Generic;
using UIModule;
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
            UISystem.OpenUIWindow("Window_Main", null);
        }

        public static void StartNewGame()
        {
            Log.LogInfo("開始新遊戲");
            _instance.InitGameTest();
        }

        public static void ContinueGame()
        {
            //load with playerprefs
            Log.LogInfo("繼續遊戲");
        }

        public static void LoadGame()
        {
            Log.LogInfo("讀取遊戲存檔");
        }

        public static void Setting()
        {
            Log.LogInfo("開啟遊戲設定頁面(共用)");
        }

        public static void Info()
        {
            Log.LogInfo("開啟遊戲資訊頁面");
        }
    }
}