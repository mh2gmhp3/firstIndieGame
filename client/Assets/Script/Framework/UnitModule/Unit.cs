using Extension;
using UnitModule.Movement;

namespace UnitModule
{
    public abstract class Unit
    {
        private int _id;
        private int _colliderGroupId;
        private UnitSetting _unitSetting;
        private string _avatarPath;
        private UnitAvatarSetting _unitAvatarSetting;

        private UnitMovementSetting _unitMovementSetting;

        public int Id => _id;
        public int ColliderGroupId => _colliderGroupId;
        public UnitSetting UnitSetting => _unitSetting;
        public string AvatarPath => _avatarPath;
        public UnitAvatarSetting UnitAvatarSetting => _unitAvatarSetting;

        public UnitMovementSetting UnitMovementSetting => _unitMovementSetting;

        public Unit()
        {
            _unitMovementSetting = new UnitMovementSetting(this);
        }

        public void Init(UnitSetting unitRootSetting)
        {
            _unitSetting = unitRootSetting;
        }

        public void Setup(int id, int colliderGroupId, string avatarPath, UnitAvatarSetting unitAvatarSetting)
        {
            _id = id;
            _colliderGroupId = colliderGroupId;
            _avatarPath = avatarPath;
            _unitAvatarSetting = unitAvatarSetting;
            _unitAvatarSetting.RootTransform.SetParent(_unitSetting.RotateTransform);
            _unitAvatarSetting.RootTransform.Reset();
            DoSetup();
        }

        public void Reset()
        {
            DoReset();
        }

        public void Clear()
        {
            _id = 0;
            _colliderGroupId = 0;
            _avatarPath = string.Empty;
            _unitAvatarSetting = null;
            DoClear();
        }

        protected abstract void DoSetup();
        protected abstract void DoReset();

        protected abstract void DoClear();
    }
}
