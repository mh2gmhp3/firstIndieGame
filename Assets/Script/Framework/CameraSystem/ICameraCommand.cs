using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public interface ICameraCommand
    {
        public int CommandId { get; set; }
        public Vector3 Target { get; set; }
    }

    public class CameraCommand : ICameraCommand
    {
        public int CommandId { get; set; }
        public Vector3 Target { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CameraCommandAttribute : Attribute
    {
        public int CommandId;

        public CameraCommandAttribute(int commandId)
        {
            CommandId = commandId;
        }
    }

    public delegate void CameraCommandMethod(ICameraCommand cameraCommand);
}
