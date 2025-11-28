using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public struct PreviewMeshData
    {
        public Mesh Mesh;
        public Matrix4x4 Matrix;
        public Material Material;

        public PreviewMeshData(Mesh mesh, Matrix4x4 matrix, Material material)
        {
            Mesh = mesh;
            Matrix = matrix;
            Material = material;
        }
    }

    public static class EditorPreviewUtility
    {
        private static PreviewRenderUtility _utility;
        private static List<PreviewMeshData> _previewMeshDataListCache = new List<PreviewMeshData>();

        public static List<PreviewMeshData> GetPreviewMeshDataListCache()
        {
            _previewMeshDataListCache.Clear();
            return _previewMeshDataListCache;
        }

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
            PreviewMeshData previewData,
            Vector2 textureSize,
            Vector3 cameraPos,
            Quaternion cameraRotation)
        {
            CheckUtility();
            _utility.camera.transform.position = cameraPos;
            _utility.camera.transform.rotation = cameraRotation;
            _utility.BeginPreview(new Rect(0, 0, textureSize.x, textureSize.y), GUIStyle.none);
            _utility.DrawMesh(previewData.Mesh, previewData.Matrix, previewData.Material, 0);
            _utility.Render(allowScriptableRenderPipeline: true);
            return _utility.EndPreview();
        }

        public static Texture GenPreviewTexture(
            List<PreviewMeshData> previewDataList,
            Vector2 textureSize,
            Vector3 cameraPos,
            Quaternion cameraRotation)
        {
            if (previewDataList == null)
                return null;

            CheckUtility();
            _utility.camera.transform.position = cameraPos;
            _utility.camera.transform.rotation = cameraRotation;
            _utility.BeginPreview(new Rect(0, 0, textureSize.x, textureSize.y), GUIStyle.none);
            for (int i = 0; i < previewDataList.Count; i++)
            {
                var previewData = previewDataList[i];
                _utility.DrawMesh(previewData.Mesh, previewData.Matrix, previewData.Material, 0);
            }
            _utility.Render(allowScriptableRenderPipeline: true);
            return _utility.EndPreview();
        }
    }
}
