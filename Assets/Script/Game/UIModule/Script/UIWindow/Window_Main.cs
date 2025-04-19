namespace UIModule.Game
{
    public partial class Window_Main : UIWindow
    {
        protected override void DoOpen(IUIData uiData)
        {
            Widget_Button_StartNew.SetData(new Widget_Button.ButtonData("開始新遊戲"));
            Widget_Button_Continue.SetData(new Widget_Button.ButtonData("繼續遊戲"));
            Widget_Button_Load.SetData(new Widget_Button.ButtonData("讀取遊戲"));
            Widget_Button_Setting.SetData(new Widget_Button.ButtonData("設定"));
            Widget_Button_Info.SetData(new Widget_Button.ButtonData("遊戲資訊"));
        }
    }
}
