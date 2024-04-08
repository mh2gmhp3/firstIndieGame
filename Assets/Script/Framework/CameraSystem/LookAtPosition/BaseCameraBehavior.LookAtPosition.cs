using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public interface ILookAtPosition : ICameraCommand
    {
        public Vector3 Position { get; set; }
    }

    public class LookAtPosition : ILookAtPosition
    {
        public int CommandId { get; set; }
        public Vector3 Position { get; set; }
    }

    public partial class BaseCameraBehavior
    {
        [CameraCommand((int)CameraCommandDefine.BaseCommand.LookAtPosition)]
        protected virtual void LookAtPosition(ICameraCommand command)
        {
            if (!(command is ILookAtPosition lookAtPosition))
                return;

            _cameraTrans.LookAt(lookAtPosition.Position);
        }
    }
}
