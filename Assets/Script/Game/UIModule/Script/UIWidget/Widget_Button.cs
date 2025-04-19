namespace UIModule.Game
{
    public partial class Widget_Button : UIWidget
    {
        /// <summary>
        /// 按鈕使用資料
        /// </summary>
        public class ButtonData : IUIData
        {
            /// <summary>
            /// 顯示文字
            /// </summary>
            public string Content;

            public ButtonData(string content)
            {
                Content = content;
            }
        }

        protected override void DoSetData()
        {
            if (_uiData is ButtonData buttonData)
            {
                Text_Content.text = buttonData.Content;
            }
        }
    }
}
