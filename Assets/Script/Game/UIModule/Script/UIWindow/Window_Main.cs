namespace UIModule.Game
{
    public partial class Window_Main : UIWindow
    {
        protected override void DoOpen(UIData uiData)
        {
            Widget_Button_StartNew.SetContent("開始新遊戲");
            Widget_Button_Continue.SetContent("繼續遊戲");
            Widget_Button_Load.SetContent("讀取遊戲");
            Widget_Button_Setting.SetContent("設定");
            Widget_Button_Info.SetContent("遊戲資訊");
        }
    }
}
