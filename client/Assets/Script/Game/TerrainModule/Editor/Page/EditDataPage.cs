using UnityEditor;
using UnityEngine;
using Extension;
using System.Collections.Generic;

namespace TerrainModule.Editor
{
    public class EditDataPage : TerrainEditorPage
    {
        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurEditRuntimeData;
        private int _editChunkFlat = 0;
        private int _editDistance = 0;

        private int _targetChunkId = -1;

        public EditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.PageToName[(int)TerrainEditorPageType.Edit];

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;
            _editChunkFlat = EditorGUILayout.IntSlider(_editChunkFlat, 0, CurEditRuntimeData.ChunkNum.y - 1);
            _editDistance = EditorGUILayout.IntSlider(_editDistance, 0, (int)CurEditRuntimeData.TerrainSize().y);
        }

        public override void OnSceneGUI()
        {
            if (CurEditRuntimeData == null)
                return;
            TerrainEditorUtility.HandleDrawChunk(
                CurEditRuntimeData.BlockSize,
                CurEditRuntimeData.ChunkBlockNum,
                CurEditRuntimeData.ChunkNum);

            var chunkSize = CurEditRuntimeData.GetChunkSize();
            Event currentEvent = Event.current;
            Vector3 mousePosition = currentEvent.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (CurEditRuntimeData.RaycastBlock(ray, _editDistance, _editChunkFlat, out var hitResult))
            {
                var chunkCenterRedress = new Vector3(
                    chunkSize.x / 2f,
                    chunkSize.y / 2f,
                    chunkSize.z / 2f);
                var chunkPivotPosition = CurEditRuntimeData.GetChunkPivotPositionWithId(hitResult.ChunkId);
                Handles.color = Color.red;
                Handles.DrawWireCube(chunkPivotPosition + chunkCenterRedress, chunkSize);
                var blockPivotPosition = CurEditRuntimeData.GetBlockInChunkCenterPositionWithId(hitResult.BlockId);
                Handles.color = Color.green;
                Handles.DrawWireCube(blockPivotPosition + chunkPivotPosition, CurEditRuntimeData.BlockSize);
                Handles.color = Color.blue;
                Handles.DrawLine(blockPivotPosition + chunkPivotPosition, blockPivotPosition + chunkPivotPosition + hitResult.HitFaceNormal * 8 * -1);

                if (currentEvent.type == EventType.MouseMove)
                {
                    SceneView.RepaintAll();
                    Repaint();
                }
            }
        }
    }
}
