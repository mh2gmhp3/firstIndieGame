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

        public class UIAttackBehaviorDataContainer : IUIData, IScrollerControllerDataGetter
        {
            public List<UIAttackBehaviorData> AttackBehaviorDataList = new List<UIAttackBehaviorData>();

            public UIAttackBehaviorDataContainer(List<AttackBehaviorData> repoDataList)
            {
                for (int i = 0; i < repoDataList.Count; i++)
                {
                    AttackBehaviorDataList.Add(new UIAttackBehaviorData(repoDataList[i]));
                }
            }

            int IScrollerControllerDataGetter.GetCellCount()
            {
                return AttackBehaviorDataList.Count;
            }

            string IScrollerControllerDataGetter.GetCellIdentity(int cellIndex)
            {
                return string.Empty;
            }

            IUIData IScrollerControllerDataGetter.GetUIData(int cellIndex, int widgetIndex)
            {
                if (cellIndex < 0 || cellIndex >= AttackBehaviorDataList.Count)
                {
                    return null;
                }

                return AttackBehaviorDataList[cellIndex];
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
            SimpleScrollerController_AttackBehavior.SetDataGetter(_dataContainer);
        }
    }
}
