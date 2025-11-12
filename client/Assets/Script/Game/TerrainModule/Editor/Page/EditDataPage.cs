using UnityEditor;
using UnityEngine;
using Extension;
using System.Collections.Generic;
using Framework.Editor;
using Framework.Editor.Utility;

namespace TerrainModule.Editor
{
    public class EditDataPage : TerrainEditorPage
    {
        private enum MouseFunction
        {
            AddBlock,
            DeleteBlock,
            SelectBlock,
        }

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurEditRuntimeData;
        private int _editChunkFlat = 0;
        private int _editDistance = 0;

        private List<int> _notifyChunkList = new List<int>();

        private MouseFunction _curMouseFunction = MouseFunction.AddBlock;

        private int _curSelectedChunkId = -1;
        private BlockEditRuntimeData _curSelectedBlockData = null;

        public EditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.PageToName[(int)TerrainEditorPageType.Edit];

        public override void OnEnable()
        {
            _editorData.TerrainEditorMgrObj.SetActive(true);
        }

        public override void OnGUI()
        {
            ReceiveInputForMouseFunction();

            if (CurEditRuntimeData == null)
                return;

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.TextField("名稱", CurEditRuntimeData.Name);
                EditorGUILayout.Vector3IntField("BlockSize", CurEditRuntimeData.BlockSize);
                EditorGUILayout.Vector3IntField("ChunkBlockNum", CurEditRuntimeData.ChunkBlockNum);
                EditorGUILayout.Vector3IntField("ChunkNum", CurEditRuntimeData.ChunkNum);
                EditorGUI.BeginChangeCheck();
                CurEditRuntimeData.TerrainMaterial =
                    (Material)EditorGUILayout.ObjectField(
                        "材質:",
                        CurEditRuntimeData.TerrainMaterial,
                        typeof(Material),
                        false);
                if (EditorGUI.EndChangeCheck())
                    _editorData.TerrainEditorMgr.RebuildAllPreviewMesh();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUI.BeginChangeCheck();
                _editChunkFlat = EditorGUILayout.IntSlider("EditChunkPlaneY:", _editChunkFlat, 0, CurEditRuntimeData.ChunkNum.y - 1);
                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
                _editDistance = EditorGUILayout.IntSlider(
                    "RayDistance:",
                    _editDistance,
                    128,
                    Mathf.Max(128, Mathf.Min((int)CurEditRuntimeData.TerrainSize().y, 512)));
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField("當前編輯方塊");
                if (_curSelectedBlockData == null)
                {
                    EditorGUILayout.LabelField("未選擇方塊");
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        EditorGUIUtil.MinMaxSlider("-z-x", ref _curSelectedBlockData.YBottomValue.x, ref _curSelectedBlockData.YTopValue.x, 0, 1);
                        GUILayout.Space(5f);
                        EditorGUIUtil.MinMaxSlider("-z+x", ref _curSelectedBlockData.YBottomValue.y, ref _curSelectedBlockData.YTopValue.y, 0, 1);
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);
                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        EditorGUIUtil.MinMaxSlider("+z-x", ref _curSelectedBlockData.YBottomValue.z, ref _curSelectedBlockData.YTopValue.z, 0, 1);
                        GUILayout.Space(5f);
                        EditorGUIUtil.MinMaxSlider("+z+x", ref _curSelectedBlockData.YBottomValue.w, ref _curSelectedBlockData.YTopValue.w, 0, 1);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("填滿方塊"))
                        {
                            _curSelectedBlockData.YTopValue = Vector4.one;
                            _curSelectedBlockData.YBottomValue = Vector4.zero;
                        }
                        if (EditorGUI.EndChangeCheck())
                            _editorData.TerrainEditorMgr.RefreshChunkMesh(_curSelectedChunkId);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("存檔"))
            {
                _editorData.SaveCurData();
            }
        }

        public override void OnSceneGUI()
        {
            ReceiveInputForMouseFunction();

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

            if (_curSelectedBlockData != null)
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetWorldBlockCenterPositionWithId(_curSelectedChunkId, _curSelectedBlockData.Id),
                    CurEditRuntimeData.BlockSize);
            }

            var chunkSize = CurEditRuntimeData.GetChunkSize();
            Event currentEvent = Event.current;
            Vector3 mousePosition = currentEvent.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (CurEditRuntimeData.RaycastBlock(ray, _editDistance, _editChunkFlat, out var hitResult))
            {
                Handles.color = Color.red;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetChunkCenterPositionWithId(hitResult.ChunkId),
                    chunkSize);
                Handles.color = Color.green;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetWorldBlockCenterPositionWithId(hitResult.ChunkId, hitResult.BlockId),
                    CurEditRuntimeData.BlockSize);
                if (hitResult.HaveData)
                {
                    var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates - hitResult.HitFaceNormal;
                    if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                    {
                        Handles.color = Color.blue;
                        Handles.DrawWireCube(
                            CurEditRuntimeData.GetWorldBlockCenterPositionWithId(chunkId, blockInChunkId),
                            CurEditRuntimeData.BlockSize);
                    }
                }

                if (currentEvent.type == EventType.MouseMove)
                {
                    SceneView.RepaintAll();
                    Repaint();
                }

                if (currentEvent.type == EventType.MouseDown)
                {
                    if (currentEvent.button == 0)
                    {
                        _notifyChunkList.Clear();
                        if (_curMouseFunction == MouseFunction.AddBlock)
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
                        }
                        else if (_curMouseFunction == MouseFunction.DeleteBlock)
                        {
                            CurEditRuntimeData.RemoveBlockData(hitResult.ChunkId, hitResult.BlockId);
                            _notifyChunkList.Add(hitResult.ChunkId);
                        }
                        else if (_curMouseFunction == MouseFunction.SelectBlock)
                        {
                            if (hitResult.HaveData)
                            {
                                if (CurEditRuntimeData.TryGetBlock(hitResult.ChunkId, hitResult.BlockId, out var blockData, out _))
                                {
                                    _curSelectedChunkId = hitResult.ChunkId;
                                    _curSelectedBlockData = blockData;
                                    Repaint();
                                }
                                else
                                {
                                    ClearSelected();
                                }
                            }
                            else
                            {
                                ClearSelected();
                            }
                        }

                        currentEvent.Use();

                        for (int i = 0; i < _notifyChunkList.Count; i++)
                        {
                            _editorData.TerrainEditorMgr.RefreshChunkMesh(_notifyChunkList[i]);
                        }
                    }
                }
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        private void ClearSelected()
        {
            _curSelectedChunkId = -1;
            _curSelectedBlockData = null;
            Repaint();
        }

        private void ReceiveInputForMouseFunction()
        {
            Event currentEvent = Event.current;
            if (!currentEvent.isKey && currentEvent.type != EventType.KeyDown)
                return;

            if (currentEvent.keyCode == KeyCode.A)
            {
                SetMouseFunction(MouseFunction.AddBlock);
            }
            else if (currentEvent.keyCode == KeyCode.S)
            {
                SetMouseFunction(MouseFunction.SelectBlock);
            }
            else if (currentEvent.keyCode == KeyCode.D)
            {
                SetMouseFunction(MouseFunction.DeleteBlock);
            }
        }

        private void SetMouseFunction(MouseFunction function)
        {
            if (_curMouseFunction == function)
                return;

            if (_curMouseFunction == MouseFunction.SelectBlock)
                ClearSelected();

            _curMouseFunction = function;
        }
    }
}
