using UnityEditor;
using UnityEngine;
using Extension;
using System.Collections.Generic;
using Framework.Editor;

namespace TerrainModule.Editor
{
    public class EditDataPage : TerrainEditorPage
    {
        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurEditRuntimeData;
        private int _editChunkFlat = 0;
        private int _editDistance = 0;

        private List<int> _notifyChunkList = new List<int>();

        public EditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.PageToName[(int)TerrainEditorPageType.Edit];

        public override void OnGUI()
        {
            if (CurEditRuntimeData == null)
                return;

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.TextField("名稱", CurEditRuntimeData.Name);
                EditorGUILayout.Vector3IntField("BlockSize", CurEditRuntimeData.BlockSize);
                EditorGUILayout.Vector3IntField("ChunkBlockNum", CurEditRuntimeData.ChunkBlockNum);
                EditorGUILayout.Vector3IntField("ChunkNum", CurEditRuntimeData.ChunkNum);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUI.BeginChangeCheck();
                _editChunkFlat = EditorGUILayout.IntSlider("編輯平面Y:", _editChunkFlat, 0, CurEditRuntimeData.ChunkNum.y - 1);
                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
                _editDistance = EditorGUILayout.IntSlider("射線距離:", _editDistance, 0, (int)CurEditRuntimeData.TerrainSize().y);
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("存檔"))
            {
                _editorData.SaveCurData();
            }
        }

        public override void OnSceneGUI()
        {
            if (CurEditRuntimeData == null)
                return;
            TerrainEditorUtility.HandleDrawChunk(
                CurEditRuntimeData.BlockSize,
                CurEditRuntimeData.ChunkBlockNum,
                CurEditRuntimeData.ChunkNum,
                new Color(1, 1, 1, 0.1f),
                _editChunkFlat);
            TerrainEditorUtility.HandleDrawChunkPlane(
                CurEditRuntimeData.BlockSize,
                CurEditRuntimeData.ChunkBlockNum,
                CurEditRuntimeData.ChunkNum,
                new Color(1, 1, 1, 0.5f),
                _editChunkFlat);

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
                if (hitResult.HaveData)
                {
                    var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates - hitResult.HitFaceNormal;
                    if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                    {
                        chunkPivotPosition = CurEditRuntimeData.GetChunkPivotPositionWithId(chunkId);
                        blockPivotPosition = CurEditRuntimeData.GetBlockInChunkCenterPositionWithId(blockInChunkId);
                        Handles.color = Color.blue;
                        Handles.DrawWireCube(blockPivotPosition + chunkPivotPosition, CurEditRuntimeData.BlockSize);
                    }
                }

                if (currentEvent.type == EventType.MouseMove)
                {
                    SceneView.RepaintAll();
                    Repaint();
                }

                _notifyChunkList.Clear();
                if (currentEvent.type == EventType.MouseDown)
                {
                    if (currentEvent.button == 0)
                    {
                        if (hitResult.HaveData)
                        {
                            var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates - hitResult.HitFaceNormal;
                            if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                            {
                                CurEditRuntimeData.AddBlockData(chunkId, blockInChunkId);
                                _notifyChunkList.Add(hitResult.ChunkId);
                                if (hitResult.ChunkId != chunkId)
                                    _notifyChunkList.Add(chunkId);
                            }
                        }
                        else
                        {
                            CurEditRuntimeData.AddBlockData(hitResult.ChunkId, hitResult.BlockId);
                            _notifyChunkList.Add(hitResult.ChunkId);
                        }
                        currentEvent.Use();
                    }
                }

                if (currentEvent.type == EventType.KeyDown && currentEvent.isKey)
                {
                    if (currentEvent.keyCode == KeyCode.D)
                    {
                        CurEditRuntimeData.RemoveBlockData(hitResult.ChunkId, hitResult.BlockId);
                        _notifyChunkList.Add(hitResult.ChunkId);
                        currentEvent.Use();
                    }
                }

                for (int i = 0; i < _notifyChunkList.Count; i++)
                {
                    _editorData.TerrainEditorMgr.RefreshChunkMesh(_notifyChunkList[i]);
                }
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }
}
