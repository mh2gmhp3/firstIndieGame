namespace UIModule.Game
{
    public partial class Widget_Button : UIWidget
    {
        public void SetContent(string content)
        {
            Init();
            Text_Content.text = content;
        }
    }
}
