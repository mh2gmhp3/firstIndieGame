using UnityEditor;
using UnityEngine;
using Extension;
using System.Collections.Generic;
using Framework.Editor;
using Framework.Editor.Utility;

namespace TerrainModule.Editor
{
    public class TerrainEditDataPage : TerrainEditBasePage
    {
        private enum MouseFunction
        {
            AddBlock,
            DeleteBlock,
            SelectBlock,
        }

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurTerrainEditRuntimeData;
        private int _editChunkFlat = 0;
        private int _editDistance = 0;
        private bool _autoYValueFit = true;

        private List<int> _notifyChunkList = new List<int>();

        private MouseFunction _curMouseFunction = MouseFunction.AddBlock;

        private Vector2 _blockTemplateScrollPos = Vector2.zero;
        private int _addBlockTemplateId = 0;

        private int _curSelectedChunkId = -1;
        private BlockEditRuntimeData _curSelectedBlockData = null;

        public TerrainEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.Edit];

        public override void OnEnable()
        {

        }

        public override void OnGUI()
        {
            ReceiveInputForMouseFunction();

            if (CurEditRuntimeData == null)
                return;

            DrawCurEditData();
            EditorGUILayout.Space(5f);
            DrawEditorSetting();
            EditorGUILayout.Space(5f);
            DrawMouseFunction();
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
                var blockWorldPivotPos = CurEditRuntimeData.GetWorldBlockPivotPositionWithId(_curSelectedChunkId, _curSelectedBlockData.Id);

                Vector4 newYTopValue = Vector4.one;
                Vector4 newYBottomValue = Vector4.zero;
                EditorGUI.BeginChangeCheck();
                {
                    var size = CurEditRuntimeData.BlockSize.y / 8f;
                    Handles.color = new Color(0, 1, 0, 0.5f);
                    var newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(0, _curSelectedBlockData.YTopValue.x * CurEditRuntimeData.BlockSize.y, 0), Vector3.up, size);
                    var y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYTopValue.x = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(CurEditRuntimeData.BlockSize.x, _curSelectedBlockData.YTopValue.y * CurEditRuntimeData.BlockSize.y, 0), Vector3.up, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYTopValue.y = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(0, _curSelectedBlockData.YTopValue.z * CurEditRuntimeData.BlockSize.y, CurEditRuntimeData.BlockSize.z), Vector3.up, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYTopValue.z = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(CurEditRuntimeData.BlockSize.x, _curSelectedBlockData.YTopValue.w * CurEditRuntimeData.BlockSize.y, CurEditRuntimeData.BlockSize.z), Vector3.up, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYTopValue.w = y;

                    Handles.color = new Color(0, 0, 1, 0.5f);
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(0, _curSelectedBlockData.YBottomValue.x * CurEditRuntimeData.BlockSize.y, 0), Vector3.down, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYBottomValue.x = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(CurEditRuntimeData.BlockSize.x, _curSelectedBlockData.YBottomValue.y * CurEditRuntimeData.BlockSize.y, 0), Vector3.down, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYBottomValue.y = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(0, _curSelectedBlockData.YBottomValue.z * CurEditRuntimeData.BlockSize.y, CurEditRuntimeData.BlockSize.z), Vector3.down, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYBottomValue.z = y;
                    newPos = HandlesUtil.SphereSlider(blockWorldPivotPos + new Vector3(CurEditRuntimeData.BlockSize.x, _curSelectedBlockData.YBottomValue.w * CurEditRuntimeData.BlockSize.y, CurEditRuntimeData.BlockSize.z), Vector3.down, size);
                    y = (newPos.y - blockWorldPivotPos.y) / CurEditRuntimeData.BlockSize.y;
                    newYBottomValue.w = y;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _curSelectedBlockData.SetYValue(newYTopValue, newYBottomValue);
                    _editorData.TerrainEditorMgr.RefreshChunkMesh(_curSelectedChunkId);
                }
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
                //要Add才會顯示
                if (_curMouseFunction == MouseFunction.AddBlock)
                {
                    if (hitResult.HaveData)
                    {
                        var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates + hitResult.HitFaceNormal;
                        if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                        {
                            Handles.color = Color.blue;
                            Handles.DrawWireCube(
                                CurEditRuntimeData.GetWorldBlockCenterPositionWithId(chunkId, blockInChunkId),
                                CurEditRuntimeData.BlockSize);
                        }
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
                        InvokeMouseFunction(hitResult);
                        currentEvent.Use();
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

        #region MouseFunction

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

            SceneView.RepaintAll();
            Repaint();
        }

        private void InvokeMouseFunction(RaycastBlockResult hitResult)
        {
            _notifyChunkList.Clear();
            if (_curMouseFunction == MouseFunction.AddBlock)
            {
                if (!CurEditRuntimeData.BlockTemplateEditRuntimeData.TryGetBlockData(_addBlockTemplateId, out var blockTemplateData))
                {
                    EditorUtility.DisplayDialog(
                        TerrainEditorDefine.Dialog_Title_Error,
                        "找不到要新增的方塊範本，請重新選擇",
                        TerrainEditorDefine.Dialog_Ok_Confirm);
                    return;
                }

                if (hitResult.HaveData)
                {
                    var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates + hitResult.HitFaceNormal;
                    if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                    {
                        var yTopValue = Vector4.one;
                        var yBottomValue = Vector4.zero;
                        if (_autoYValueFit &&CurEditRuntimeData.TryGetBlock(hitResult.ChunkId, hitResult.BlockId, out var blockData, out _))
                        {
                            if (hitResult.HitFaceNormal.x == 1)
                            {
                                //+x (-zb, -zt, +zb, +zt)
                                var zdiff = new Vector4(
                                    blockData.YBottomValue.y - blockData.YBottomValue.x,
                                    blockData.YTopValue.y - blockData.YTopValue.x,
                                    blockData.YBottomValue.w - blockData.YBottomValue.z,
                                    blockData.YTopValue.w - blockData.YTopValue.z);
                                yTopValue = new Vector4(
                                    blockData.YTopValue.y,
                                    Mathf.Clamp01(blockData.YTopValue.y + zdiff.y),
                                    blockData.YTopValue.w,
                                    Mathf.Clamp01(blockData.YTopValue.w + zdiff.w));
                                yBottomValue = new Vector4(
                                    blockData.YBottomValue.y,
                                    Mathf.Clamp01(blockData.YBottomValue.y + zdiff.x),
                                    blockData.YBottomValue.w,
                                    Mathf.Clamp01(blockData.YBottomValue.w + zdiff.z));
                            }
                            else if (hitResult.HitFaceNormal.x == -1)
                            {
                                //-x (-zb, -zt, +zb, +zt)
                                var zdiff = new Vector4(
                                    blockData.YBottomValue.y - blockData.YBottomValue.x,
                                    blockData.YTopValue.y - blockData.YTopValue.x,
                                    blockData.YBottomValue.w - blockData.YBottomValue.z,
                                    blockData.YTopValue.w - blockData.YTopValue.z)
                                    * -1;
                                yTopValue = new Vector4(
                                    Mathf.Clamp01(blockData.YTopValue.x + zdiff.y),
                                    blockData.YTopValue.x,
                                    Mathf.Clamp01(blockData.YTopValue.z + zdiff.w),
                                    blockData.YTopValue.z);
                                yBottomValue = new Vector4(
                                    Mathf.Clamp01(blockData.YBottomValue.x + zdiff.x),
                                    blockData.YBottomValue.x,
                                    Mathf.Clamp01(blockData.YBottomValue.z + zdiff.z),
                                    blockData.YBottomValue.z);
                            }
                            else if (hitResult.HitFaceNormal.z == 1)
                            {
                                //+z (+xb, +xt, -xb, -xt)
                                var zdiff = new Vector4(
                                    blockData.YBottomValue.w - blockData.YBottomValue.y,
                                    blockData.YTopValue.w - blockData.YTopValue.y,
                                    blockData.YBottomValue.z - blockData.YBottomValue.x,
                                    blockData.YTopValue.z - blockData.YTopValue.x);
                                yTopValue = new Vector4(
                                    blockData.YTopValue.z,
                                    blockData.YTopValue.w,
                                    Mathf.Clamp01(blockData.YTopValue.z + zdiff.w),
                                    Mathf.Clamp01(blockData.YTopValue.w + zdiff.y));
                                yBottomValue = new Vector4(
                                    blockData.YBottomValue.z,
                                    blockData.YBottomValue.w,
                                    Mathf.Clamp01(blockData.YBottomValue.z + zdiff.z),
                                    Mathf.Clamp01(blockData.YBottomValue.w + zdiff.x));
                            }
                            else if (hitResult.HitFaceNormal.z == -1)
                            {
                                //-z (+xb, +xt, -xb, -xt)
                                var zdiff = new Vector4(
                                    blockData.YBottomValue.w - blockData.YBottomValue.y,
                                    blockData.YTopValue.w - blockData.YTopValue.y,
                                    blockData.YBottomValue.z - blockData.YBottomValue.x,
                                    blockData.YTopValue.z - blockData.YTopValue.x)
                                    * -1;
                                yTopValue = new Vector4(
                                    Mathf.Clamp01(blockData.YTopValue.x + zdiff.w),
                                    Mathf.Clamp01(blockData.YTopValue.y + zdiff.y),
                                    blockData.YTopValue.x,
                                    blockData.YTopValue.y);
                                yBottomValue = new Vector4(
                                    Mathf.Clamp01(blockData.YBottomValue.x + zdiff.z),
                                    Mathf.Clamp01(blockData.YBottomValue.y + zdiff.x),
                                    blockData.YBottomValue.x,
                                    blockData.YBottomValue.y);
                            }
                        }

                        CurEditRuntimeData.AddBlockData(chunkId, blockInChunkId, blockTemplateData.Id, yTopValue, yBottomValue);
                        _notifyChunkList.Add(hitResult.ChunkId);
                        if (hitResult.ChunkId != chunkId)
                            _notifyChunkList.Add(chunkId);
                    }
                }
                else
                {
                    CurEditRuntimeData.AddBlockData(hitResult.ChunkId, hitResult.BlockId, blockTemplateData.Id);
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

            for (int i = 0; i < _notifyChunkList.Count; i++)
            {
                _editorData.TerrainEditorMgr.RefreshChunkMesh(_notifyChunkList[i]);
            }
        }

        #endregion

        #region DrawGUI

        private void DrawCurEditData()
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.TextField("名稱", CurEditRuntimeData.Name);
                EditorGUILayout.Vector3IntField("BlockSize", CurEditRuntimeData.BlockSize);
                EditorGUILayout.Vector3IntField("ChunkBlockNum", CurEditRuntimeData.ChunkBlockNum);
                EditorGUILayout.Vector3IntField("ChunkNum", CurEditRuntimeData.ChunkNum);
                EditorGUI.BeginChangeCheck();
                CurEditRuntimeData.BlockTemplateEditData =
                    (BlockTemplateEditData)EditorGUILayout.ObjectField(
                        "地格範本:",
                        CurEditRuntimeData.BlockTemplateEditData,
                        typeof(BlockTemplateEditData),
                        false);
                if (EditorGUI.EndChangeCheck())
                {
                    CurEditRuntimeData.RefreshBlockTemplateRuntimeData();
                    _editorData.TerrainEditorMgr.RebuildAllTerrainPreviewMesh();
                }
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("刷新範本"))
                    {
                        CurEditRuntimeData.RefreshBlockTemplateRuntimeData();
                        _editorData.TerrainEditorMgr.RebuildAllTerrainPreviewMesh();
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("存檔"))
                    {
                        _editorData.SaveCurTerrainData();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawEditorSetting()
        {
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
        }

        private void DrawMouseFunction()
        {
            if (_curMouseFunction == MouseFunction.AddBlock)
            {
                EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
                {
                    _autoYValueFit = EditorGUILayout.Toggle("自動貼合Y高低值:", _autoYValueFit);
                    EditorGUILayout.LabelField("新增方塊類型");
                    if (CurEditRuntimeData.BlockTemplateEditRuntimeData == null)
                    {
                        EditorGUILayout.LabelField("沒有設定方塊範本");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"當前選擇Id:{_addBlockTemplateId}");
                        _blockTemplateScrollPos = EditorGUILayout.BeginScrollView(_blockTemplateScrollPos, CommonGUIStyle.Default_Box);
                        {
                            for (int i = 0; i < CurEditRuntimeData.BlockTemplateEditRuntimeData.BlockTemplateDataList.Count; i++)
                            {
                                DrawBlockTemplateScrollCell(CurEditRuntimeData.BlockTemplateEditRuntimeData.BlockTemplateDataList[i]);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else if (_curMouseFunction == MouseFunction.SelectBlock)
            {
                EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
                {
                    EditorGUILayout.LabelField("當前選擇方塊");
                    if (_curSelectedBlockData == null)
                    {
                        EditorGUILayout.LabelField("未選擇方塊");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"選擇方塊: " +
                            $"ChunkId:{_curSelectedChunkId} " +
                            $"BlockId:{_curSelectedBlockData.Id} " +
                            $"TemplateId:{_curSelectedBlockData.TemplateId}");
                        EditorGUILayout.Space(5f);

                        Vector4 newYTopValue = _curSelectedBlockData.YTopValue;
                        Vector4 newYBottomValue = _curSelectedBlockData.YBottomValue;
                        EditorGUILayout.LabelField("Y高低值:");
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                            {
                                EditorGUIUtil.MinMaxSlider("-z-x", ref newYBottomValue.x, ref newYTopValue.x, 0, 1);
                                GUILayout.Space(5f);
                                EditorGUIUtil.MinMaxSlider("-z+x", ref newYBottomValue.y, ref newYTopValue.y, 0, 1);
                            }
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(5f);
                            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                            {
                                EditorGUIUtil.MinMaxSlider("+z-x", ref newYBottomValue.z, ref newYTopValue.z, 0, 1);
                                GUILayout.Space(5f);
                                EditorGUIUtil.MinMaxSlider("+z+x", ref newYBottomValue.w, ref newYTopValue.w, 0, 1);
                            }
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button("填滿方塊"))
                                {
                                    newYTopValue = Vector4.one;
                                    newYBottomValue = Vector4.zero;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            _curSelectedBlockData.SetYValue(newYTopValue, newYBottomValue);
                            _editorData.TerrainEditorMgr.RefreshChunkMesh(_curSelectedChunkId);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawBlockTemplateScrollCell(BlockTemplateRuntimeData data)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.Default_Box);
            {
                EditorGUILayout.LabelField($"Id:{data.Id}");
                TerrainEditorUtility.DrawTiling(data, false);
                if (GUILayout.Button("選擇"))
                {
                    _addBlockTemplateId = data.Id;
                }
            }
            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
