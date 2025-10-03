using DataModule;
using Logging;
using System.Collections.Generic;

namespace UIModule.Game
{
    public partial class Window_AttackBehaviorEdit : UIWindow
    {
        public class UIAttackBehaviorData : IUIData
        {
            public AttackBehaviorData RawData;

            public UIAttackBehaviorData(AttackBehaviorData rawData)
            {
                RawData = rawData;
            }
        }

        public class UIAttackBehaviorDataContainer : IUIData
        {
            public List<UIAttackBehaviorData> AttackBehaviorDataList = new List<UIAttackBehaviorData>();

            public UIAttackBehaviorDataContainer(List<AttackBehaviorData> repoDataList)
            {
                for (int i = 0; i < repoDataList.Count; i++)
                {
                    AttackBehaviorDataList.Add(new UIAttackBehaviorData(repoDataList[i]));
                }
            }
        }

        private UIAttackBehaviorDataContainer _dataContainer;

        protected override void DoOpen(IUIData uiData)
        {
            if (uiData is UIAttackBehaviorDataContainer dataContainer)
            {
                _dataContainer = dataContainer;
            }
            else
            {
                Log.LogError("Window_AttackBehaviorEdit Error, Data invalid");
                SetVisible(false);
                return;
            }
            SetScrollerData();
        }

        protected override void DoClose()
        {

        }

        protected override void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo)
        {

        }

        private void SetScrollerData()
        {
            //TODO 先直接清掉 應該要用Pool的方式處理Scroller
            var childCount = RectTransform_Scroller_Content.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                var child = RectTransform_Scroller_Content.GetChild(i);
                if (child.gameObject == GameObject_AttackBehavior_Cell_Template)
                    continue;
                Destroy(RectTransform_Scroller_Content.GetChild(i).gameObject);
            }

            for (int i = 0; i < _dataContainer.AttackBehaviorDataList.Count; i++)
            {
                var newCell = Instantiate(GameObject_AttackBehavior_Cell_Template);
                newCell.transform.SetParent(RectTransform_Scroller_Content);
                var widget = newCell.GetComponent<Widget_AttackBehavior_Cell>();
                widget.SetData(_dataContainer.AttackBehaviorDataList[i]);
            }
        }
    }
}
