using System.Drawing.Drawing2D;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public static class EditorPreviewUtility
    {
        private static PreviewRenderUtility _utility;

        private static void CheckUtility()
        {
            if (_utility == null)
            {
                _utility = new PreviewRenderUtility();
                _utility.camera.transform.rotation = Quaternion.identity;

                _utility.lights[0].intensity = 1f;
                _utility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
                _utility.ambientColor = Color.gray;

                _utility.camera.clearFlags = CameraClearFlags.SolidColor;
                _utility.camera.fieldOfView = 60;
                _utility.camera.nearClipPlane = 0.3f;
                _utility.camera.farClipPlane = 100f;
            }
        }

        public static void Cleanup()
        {
            if (_utility != null)
            {
                _utility.Cleanup();
                _utility = null;
            }
        }

        public static Texture GenPreviewTexture(
            Mesh mesh,
            Material material,
            Vector2 textureSize,
            Vector3 cameraPos,
            Quaternion cameraRotation,
            Matrix4x4 meshMatrix)
        {
            CheckUtility();
            _utility.camera.transform.position = cameraPos;
            _utility.camera.transform.rotation = cameraRotation;
            _utility.BeginPreview(new Rect(0, 0, textureSize.x, textureSize.y), GUIStyle.none);
            _utility.DrawMesh(mesh, meshMatrix, material, 0);
            _utility.Render(allowScriptableRenderPipeline: true);
            return _utility.EndPreview();
        }
    }
}
