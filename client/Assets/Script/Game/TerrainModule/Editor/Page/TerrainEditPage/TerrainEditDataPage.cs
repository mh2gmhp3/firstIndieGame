using UnityEditor;
using UnityEngine;
using Extension;
using System.Collections.Generic;
using Framework.Editor;
using Framework.Editor.Utility;
using System;
using Utility;

namespace TerrainModule.Editor
{
    public class TerrainEditDataPage : TerrainEditBasePage
    {
        private struct BrushAddBlockInfo
        {
            public Vector3Int Coord;
            public Vector4 YValue;
            public int TemplateId;
            public int Direction;

            public BrushAddBlockInfo(Vector3Int coord, Vector4 yValue, int templateId, int dirextion)
            {
                Coord = coord;
                YValue = yValue;
                TemplateId = templateId;
                Direction = dirextion;
            }
        }

        private enum MouseFunction
        {
            AddBlock,
            DeleteBlock,
            SelectBlock,
        }

        private enum BlockOperateMode
        {
            Single,
            Area,
            Brush,
        }

        private TerrainEditRuntimeData CurEditRuntimeData => _editorData.CurTerrainEditRuntimeData;
        private int _editChunkFlat = 0;
        private int _editDistance = 0;

        private HashSet<int> _notifyChunkSet = new HashSet<int>();

        private MouseFunction _curMouseFunction = MouseFunction.AddBlock;
        private BlockOperateMode _blockOperateMode = BlockOperateMode.Single;

        #region AddBlock

        //Area Mode
        private List<Vector3Int> _areaBlockCoord = new List<Vector3Int>();
        private List<Vector3Int> _areaPreviewBlockCoord = new List<Vector3Int>();
        private List<Vector3Int> _inAreaBlockCoord = new List<Vector3Int>();

        //Brush Mode
        private Color _brushHandlesCircleColor = new Color(0, 0, 1, 0.3f);
        private Color _brushHandlesBlockColor = new Color(0, 0, 1, 0.8f);
        private float _brushSize = 10f;
        private float _brushWeight = 1.0f;
        private bool _brushOnlySetYValue = false;
        private List<Vector2Int> _intersectsXZBrushBlockCoord = new List<Vector2Int>();
        private List<BrushAddBlockInfo> _brushNeedAddBlockCoord = new List<BrushAddBlockInfo>();

        private Vector2 _blockTemplateScrollPos = Vector2.zero;
        private int _addBlockTemplateId = 0;
        private bool _autoYValueFit = true;
        private bool _replaceBlock = false;

        #endregion

        private int _curSelectedChunkId = -1;
        private BlockEditRuntimeData _curSelectedBlockData = null;

        private BlockTemplatePreviewSetting _blockPreviewSetting = new BlockTemplatePreviewSetting();

        public TerrainEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.Edit];

        public override void OnEnable()
        {
            if (CurEditRuntimeData != null)
            {
                _blockPreviewSetting.BlockSize = CurEditRuntimeData.BlockSize;
            }
        }

        public override void OnDisable()
        {
            EditorPreviewUtility.Cleanup();
            _areaBlockCoord.Clear();
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
                //Draw SelectBlcok in Chunk
                Handles.color = Color.red;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetWorldBlockCenterPositionWithId(_curSelectedChunkId, _curSelectedBlockData.Id),
                    CurEditRuntimeData.BlockSize);
                var blockWorldPivotPos = CurEditRuntimeData.GetWorldBlockPivotPositionWithId(_curSelectedChunkId, _curSelectedBlockData.Id);

                //Draw YValue SphereSlider Handler
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

            if (CurEditRuntimeData.RaycastBlock(ray, _editDistance, out var hitResult, RaycastFilter))
            {
                //Draw Current Chunk
                Handles.color = Color.red;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetChunkCenterPositionWithId(hitResult.ChunkId),
                    chunkSize);
                //要Add才會顯示
                if (_curMouseFunction == MouseFunction.AddBlock)
                {
                    if (_blockOperateMode == BlockOperateMode.Single)
                    {
                        DrawSinglePreviewBlock(hitResult, !_replaceBlock);
                    }
                    else if (_blockOperateMode == BlockOperateMode.Area)
                    {
                        DrawSinglePreviewBlock(hitResult, false);
                        if (hitResult.HaveData && !_replaceBlock)
                            DrawAreaPreviewBlock(hitResult.WorldBlockCoordinates + hitResult.HitFaceNormal);
                        else
                            DrawAreaPreviewBlock(hitResult.WorldBlockCoordinates);
                    }
                    else if (_blockOperateMode == BlockOperateMode.Brush)
                    {
                        DrawBushPreviewBlock(hitResult);
                    }
                }
                else if (_curMouseFunction == MouseFunction.DeleteBlock)
                {
                    if (_blockOperateMode == BlockOperateMode.Single)
                    {
                        DrawSinglePreviewBlock(hitResult, false);
                    }
                    else if (_blockOperateMode == BlockOperateMode.Area)
                    {
                        DrawSinglePreviewBlock(hitResult, false);
                        DrawAreaPreviewBlock(hitResult.WorldBlockCoordinates);
                    }
                    else if (_blockOperateMode == BlockOperateMode.Brush)
                    {
                        DrawBushPreviewBlock(hitResult);
                    }
                }
                else if (_curMouseFunction == MouseFunction.SelectBlock)
                {
                    DrawSinglePreviewBlock(hitResult, false);
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

        private void DrawSinglePreviewBlock(RaycastBlockResult hitResult, bool drawFaceNormalBlock)
        {
            //Draw Current Block
            Handles.color = Color.green;
            Handles.DrawWireCube(
                CurEditRuntimeData.GetWorldBlockCenterPositionWithId(hitResult.ChunkId, hitResult.BlockId),
                CurEditRuntimeData.BlockSize);

            //Draw FaceNormal Block
            if (hitResult.HaveData && drawFaceNormalBlock)
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

        private void DrawAreaPreviewBlock(Vector3Int curWorldBlockCoord)
        {
            if (_areaBlockCoord.Count > 0)
            {
                _areaPreviewBlockCoord.Clear();
                for (int i = 0; i < 3; i++)
                {
                    if (i < _areaBlockCoord.Count)
                    {
                        _areaPreviewBlockCoord.Add(_areaBlockCoord[i]);
                        continue;
                    }

                    _areaPreviewBlockCoord.Add(curWorldBlockCoord);

                }
                var diff = new Vector3Int(
                    _areaPreviewBlockCoord[1].x - _areaPreviewBlockCoord[0].x,
                    _areaPreviewBlockCoord[2].y - _areaPreviewBlockCoord[0].y,
                    _areaPreviewBlockCoord[1].z - _areaPreviewBlockCoord[0].z);
                var absDiff = new Vector3Int(
                    Math.Abs(diff.x),
                    Math.Abs(diff.y),
                    Math.Abs(diff.z));
                var blockSize = CurEditRuntimeData.BlockSize;
                Handles.color = Color.yellow;
                var xzPlaneCenterCoordDiff = ((Vector3)_areaPreviewBlockCoord[1] - _areaPreviewBlockCoord[0]) / 2f;
                var yCenterCoordDiff = (_areaPreviewBlockCoord[2].y - _areaPreviewBlockCoord[1].y) / 2f;
                var centerCoord = _areaPreviewBlockCoord[0] + new Vector3(xzPlaneCenterCoordDiff.x, yCenterCoordDiff, xzPlaneCenterCoordDiff.z);
                Handles.DrawWireCube(
                    new Vector3(centerCoord.x * blockSize.x, centerCoord.y * blockSize.y, centerCoord.z * blockSize.z) + (Vector3)blockSize / 2f,
                    new Vector3((absDiff.x + 1) * blockSize.x, (absDiff.y + 1) * blockSize.y, (absDiff.z + 1) * blockSize.z));
            }
        }

        private void DrawBushPreviewBlock(RaycastBlockResult hitResult)
        {
            float radius = _brushSize / 2f;
            Handles.color = _brushHandlesCircleColor;
            Handles.DrawSolidDisc(
                new Vector3(
                    hitResult.HitWorldPosition.x,
                    (hitResult.HaveData ?
                        hitResult.WorldBlockCoordinates.y + 1 :
                        hitResult.WorldBlockCoordinates.y) * CurEditRuntimeData.BlockSize.y,
                    hitResult.HitWorldPosition.z),
                Vector3.up,
                radius);
            VoxelUtility.GetCircleIntersectsVoxel(
                new Vector2(hitResult.HitWorldPosition.x, hitResult.HitWorldPosition.z),
                radius,
                new Vector2(CurEditRuntimeData.BlockSize.x, CurEditRuntimeData.BlockSize.z),
                _intersectsXZBrushBlockCoord);
            Handles.color = _brushHandlesBlockColor;
            for (int i = 0; i < _intersectsXZBrushBlockCoord.Count; i++)
            {
                var xzCoord = _intersectsXZBrushBlockCoord[i];
                var blockCoord = new Vector3Int(xzCoord.x, hitResult.WorldBlockCoordinates.y, xzCoord.y);
                if (!CurEditRuntimeData.TryGetId(blockCoord, out var chunkId, out var blockInChunkId))
                    continue;
                Handles.DrawWireCube(
                    CurEditRuntimeData.GetWorldBlockCenterPositionWithId(chunkId, blockInChunkId),
                    CurEditRuntimeData.BlockSize);
            }
        }

        private RaycastBlockFilterType RaycastFilter(Vector3Int blockWorldCoord)
        {
            var chunkCoord = CurEditRuntimeData.GetChunkCoordWithWorldBlockCoord(blockWorldCoord);
            if (chunkCoord.y < _editChunkFlat)
                return RaycastBlockFilterType.Break;

            if (_curMouseFunction == MouseFunction.AddBlock || _curMouseFunction == MouseFunction.DeleteBlock)
            {
                if (_blockOperateMode == BlockOperateMode.Area)
                {
                    if (_areaBlockCoord.Count == 1)
                    {
                        if (blockWorldCoord.y == _areaBlockCoord[0].y)
                            return RaycastBlockFilterType.Ok;
                    }
                    else if (_areaBlockCoord.Count == 2)
                    {
                        if (blockWorldCoord.x == _areaBlockCoord[1].x && blockWorldCoord.z == _areaBlockCoord[1].z)
                            return RaycastBlockFilterType.Ok;
                    }

                    return _areaBlockCoord.Count == 0 ? RaycastBlockFilterType.Ok : RaycastBlockFilterType.Continue;
                }
            }

            return RaycastBlockFilterType.Ok;
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

            if (_curMouseFunction == MouseFunction.AddBlock)
                _areaBlockCoord.Clear();
            if (_curMouseFunction == MouseFunction.DeleteBlock)
                _areaBlockCoord.Clear();
            if (_curMouseFunction == MouseFunction.SelectBlock)
                ClearSelected();

            _curMouseFunction = function;

            SceneView.RepaintAll();
            Repaint();
        }

        private void InvokeMouseFunction(RaycastBlockResult hitResult)
        {
            _notifyChunkSet.Clear();
            if (_curMouseFunction == MouseFunction.AddBlock)
            {
                if (_blockOperateMode == BlockOperateMode.Single)
                {
                    if (!CheckBlockTemplateId())
                        return;

                    if (hitResult.HaveData)
                    {
                        if (_replaceBlock)
                        {
                            CurEditRuntimeData.UpdateBlockData(hitResult.ChunkId, hitResult.BlockId, _addBlockTemplateId);
                            _notifyChunkSet.Add(hitResult.ChunkId);
                        }
                        else
                        {
                            var newAddWorldBlockCoord = hitResult.WorldBlockCoordinates + hitResult.HitFaceNormal;
                            if (CurEditRuntimeData.TryGetId(newAddWorldBlockCoord, out var chunkId, out var blockInChunkId))
                            {
                                var yTopValue = Vector4.one;
                                var yBottomValue = Vector4.zero;
                                if (_autoYValueFit && CurEditRuntimeData.TryGetBlock(hitResult.ChunkId, hitResult.BlockId, out var blockData, out _))
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

                                CurEditRuntimeData.AddBlockData(chunkId, blockInChunkId, _addBlockTemplateId, yTopValue, yBottomValue);
                                _notifyChunkSet.Add(hitResult.ChunkId);
                                if (hitResult.ChunkId != chunkId)
                                    _notifyChunkSet.Add(chunkId);
                            }
                        }
                    }
                    else
                    {
                        CurEditRuntimeData.AddBlockData(hitResult.ChunkId, hitResult.BlockId, _addBlockTemplateId);
                        _notifyChunkSet.Add(hitResult.ChunkId);
                    }
                }
                else if (_blockOperateMode == BlockOperateMode.Area)
                {
                    if (!CheckBlockTemplateId())
                        return;

                    if (_replaceBlock)
                    {
                        _areaBlockCoord.Add(hitResult.WorldBlockCoordinates);
                    }
                    else
                    {
                        if (hitResult.HaveData)
                            _areaBlockCoord.Add(hitResult.WorldBlockCoordinates + hitResult.HitFaceNormal);
                        else
                            _areaBlockCoord.Add(hitResult.WorldBlockCoordinates);
                    }
                    var inAreaWorldBlockCoordList = GetInAreaWorldBlockCoordList();
                    for (int i = 0; i < inAreaWorldBlockCoordList.Count; i++)
                    {
                        if (CurEditRuntimeData.TryGetId(inAreaWorldBlockCoordList[i], out var chunkId, out var blockId))
                        {
                            if (_replaceBlock)
                                CurEditRuntimeData.UpdateBlockData(chunkId, blockId, _addBlockTemplateId);
                            else
                                CurEditRuntimeData.AddBlockData(chunkId, blockId, _addBlockTemplateId);
                            _notifyChunkSet.Add(chunkId);
                        }
                    }
                }
                else if (_blockOperateMode == BlockOperateMode.Brush)
                {
                    BrushDraw(hitResult, true);
                }
            }
            else if (_curMouseFunction == MouseFunction.DeleteBlock)
            {
                if (_blockOperateMode == BlockOperateMode.Single)
                {
                    CurEditRuntimeData.RemoveBlockData(hitResult.ChunkId, hitResult.BlockId);
                    _notifyChunkSet.Add(hitResult.ChunkId);
                }
                else if (_blockOperateMode == BlockOperateMode.Area)
                {
                    _areaBlockCoord.Add(hitResult.WorldBlockCoordinates);
                    var inAreaWorldBlockCoordList = GetInAreaWorldBlockCoordList();
                    for (int i = 0; i < inAreaWorldBlockCoordList.Count; i++)
                    {
                        if (CurEditRuntimeData.TryGetId(inAreaWorldBlockCoordList[i], out var chunkId, out var blockId))
                        {
                            CurEditRuntimeData.RemoveBlockData(chunkId, blockId);
                            _notifyChunkSet.Add(chunkId);
                        }
                    }
                }
                else if (_blockOperateMode == BlockOperateMode.Brush)
                {
                    BrushDraw(hitResult, false);
                }
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

            foreach (var notifyChunk in _notifyChunkSet)
            {
                _editorData.TerrainEditorMgr.RefreshChunkMesh(notifyChunk);
            }
        }

        private bool CheckBlockTemplateId()
        {
            if (!CurEditRuntimeData.BlockTemplateEditRuntimeData.TryGetBlockData(_addBlockTemplateId, out _))
            {
                EditorUtility.DisplayDialog(
                    TerrainEditorDefine.Dialog_Title_Error,
                    "找不到要新增的方塊範本，請重新選擇",
                    TerrainEditorDefine.Dialog_Ok_Confirm);
                return false;
            }
            return true;
        }

        private List<Vector3Int> GetInAreaWorldBlockCoordList()
        {
            _inAreaBlockCoord.Clear();
            if (_areaBlockCoord.Count == 3)
            {
                var startCoord = _areaBlockCoord[0];
                var diff = new Vector3Int(
                    _areaBlockCoord[1].x - _areaBlockCoord[0].x,
                    _areaBlockCoord[2].y - _areaBlockCoord[0].y,
                    _areaBlockCoord[1].z - _areaBlockCoord[0].z);
                var absDiff = new Vector3Int(
                    Math.Abs(diff.x),
                    Math.Abs(diff.y),
                    Math.Abs(diff.z));
                var sign = new Vector3Int(
                    Math.Sign(diff.x),
                    Math.Sign(diff.y),
                    Math.Sign(diff.z));
                for (int y = 0; y < absDiff.y + 1; y++)
                {
                    for (int z = 0; z < absDiff.z + 1; z++)
                    {
                        for (int x = 0; x < absDiff.x + 1; x++)
                        {
                            var worldCoord = startCoord + new Vector3Int(x * sign.x, y * sign.y, z * sign.z);
                            _inAreaBlockCoord.Add(worldCoord);
                        }
                    }
                }
                _areaBlockCoord.Clear();
            }
            return _inAreaBlockCoord;
        }

        private void BrushDraw(RaycastBlockResult hitResult, bool add)
        {
            if (_replaceBlock)
            {
                //覆蓋不處理刪除
                if (!add)
                    return;
                //覆蓋必須要有選擇範本
                if (!CheckBlockTemplateId())
                    return;
            }

            _brushNeedAddBlockCoord.Clear();
            float radius = _brushSize / 2f;
            var hitXZWorldPos = new Vector2(hitResult.HitWorldPosition.x, hitResult.HitWorldPosition.z);
            VoxelUtility.GetCircleIntersectsVoxel(
                hitXZWorldPos,
                radius,
                new Vector2(CurEditRuntimeData.BlockSize.x, CurEditRuntimeData.BlockSize.z),
                _intersectsXZBrushBlockCoord);
            Handles.color = _brushHandlesBlockColor;
            var direction = 0;
            if (add)
            {
                if (hitResult.HitFaceNormal == Vector3Int.down)
                    direction = -1;
                else
                    direction = 1;
            }
            else
            {
                if (hitResult.HitFaceNormal == Vector3Int.down)
                    direction = 1;
                else
                    direction = -1;
            }
            var weight = _brushWeight * direction;
            bool allMax = true;
            for (int i = 0; i < _intersectsXZBrushBlockCoord.Count; i++)
            {
                var xzCoord = _intersectsXZBrushBlockCoord[i];
                var blockCoord = new Vector3Int(xzCoord.x, hitResult.WorldBlockCoordinates.y, xzCoord.y);
                if (!CurEditRuntimeData.TryGetId(blockCoord, out var chunkId, out var blockInChunkId))
                    continue;
                var worldPos = CurEditRuntimeData.GetWorldBlockPivotPositionWithId(chunkId, blockInChunkId);
                var xzPos = new Vector2(worldPos.x, worldPos.z);
                var inBrushYPoint = GetInBrushYPoint(hitXZWorldPos, xzPos, radius);
                if (!_replaceBlock)
                {
                    //都沒有碰到任何Y點
                    if (inBrushYPoint == Vector4.zero)
                        continue;
                }
                var addValue = inBrushYPoint * weight;
                if (add)
                {
                    if (CurEditRuntimeData.TryGetBlock(chunkId, blockInChunkId, out var blockData, out _))
                    {
                        if (_replaceBlock)
                        {
                            //只更新類型
                            CurEditRuntimeData.UpdateBlockData(chunkId, blockInChunkId, _addBlockTemplateId);
                            _notifyChunkSet.Add(chunkId);
                            continue;
                        }
                        if (direction > 0)
                        {
                            //由上而下新增
                            if (blockData.YTopValue == Vector4.one)
                            {
                                _brushNeedAddBlockCoord.Add(new BrushAddBlockInfo(blockCoord, addValue, blockData.TemplateId, direction));
                            }
                            else
                            {
                                var oriYTop = blockData.YTopValue;
                                blockData.SetYValue(blockData.YTopValue + addValue, blockData.YBottomValue);
                                if (oriYTop != blockData.YTopValue)
                                {
                                    _notifyChunkSet.Add(chunkId);
                                    allMax = false;
                                }
                            }
                        }
                        else
                        {
                            //由下而上新增
                            if (blockData.YBottomValue == Vector4.zero)
                            {
                                _brushNeedAddBlockCoord.Add(new BrushAddBlockInfo(blockCoord, addValue, blockData.TemplateId, direction));
                            }
                            else
                            {
                                var oriYBottom = blockData.YBottomValue;
                                blockData.SetYValue(blockData.YTopValue, blockData.YBottomValue + addValue);
                                if (oriYBottom != blockData.YBottomValue)
                                {
                                    _notifyChunkSet.Add(chunkId);
                                    allMax = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (CurEditRuntimeData.TryGetBlock(chunkId, blockInChunkId, out var blockData, out _))
                    {
                        if (direction > 0)
                        {
                            //由下而上刪除
                            var oriYBottom = blockData.YBottomValue;
                            blockData.SetYValue(blockData.YTopValue, blockData.YBottomValue + addValue);
                            if (blockData.YBottomValue == Vector4.one)
                            {
                                _brushNeedAddBlockCoord.Add(new BrushAddBlockInfo(blockCoord, addValue, blockData.TemplateId, direction));
                            }
                            else if (oriYBottom != blockData.YBottomValue)
                            {
                                _notifyChunkSet.Add(chunkId);
                                allMax = false;
                            }
                        }
                        else
                        {
                            //由上而下刪除
                            var oriYTop = blockData.YTopValue;
                            blockData.SetYValue(blockData.YTopValue + addValue, blockData.YBottomValue);
                            if (blockData.YTopValue == Vector4.zero)
                            {
                                _brushNeedAddBlockCoord.Add(new BrushAddBlockInfo(blockCoord, addValue, blockData.TemplateId, direction));
                            }
                            else if (oriYTop != blockData.YTopValue)
                            {
                                _notifyChunkSet.Add(chunkId);
                                allMax = false;
                            }
                        }
                    }
                }
            }
            if (allMax)
            {
                if (add)
                {
                    for (int i = 0; i < _brushNeedAddBlockCoord.Count; i++)
                    {
                        var addBlockInfo = _brushNeedAddBlockCoord[i];
                        var addBlockCoord = addBlockInfo.Coord + new Vector3Int(0, addBlockInfo.Direction, 0);
                        if (!CurEditRuntimeData.TryGetId(addBlockCoord, out var chunkId, out var blockId))
                            continue;
                        if (CurEditRuntimeData.TryGetBlock(addBlockCoord, out var blockData, out _))
                        {
                            if (addBlockInfo.Direction > 0)
                            {
                                blockData.SetYValue(blockData.YTopValue + addBlockInfo.YValue, blockData.YBottomValue);
                            }
                            else
                            {
                                blockData.SetYValue(blockData.YTopValue, blockData.YBottomValue + addBlockInfo.YValue);
                            }
                        }
                        else
                        {
                            if (!_brushOnlySetYValue)
                            {
                                if (addBlockInfo.Direction > 0)
                                {
                                    CurEditRuntimeData.AddBlockData(chunkId, blockId, addBlockInfo.TemplateId, addBlockInfo.YValue, Vector4.zero);
                                }
                                else
                                {
                                    CurEditRuntimeData.AddBlockData(chunkId, blockId, addBlockInfo.TemplateId, Vector4.one, Vector4.one + addBlockInfo.YValue);
                                }
                            }
                        }

                        _notifyChunkSet.Add(chunkId);
                    }
                }
                else
                {
                    if (!_brushOnlySetYValue)
                    {
                        for (int i = 0; i < _brushNeedAddBlockCoord.Count; i++)
                        {
                            var addBlockInfo = _brushNeedAddBlockCoord[i];
                            if (!CurEditRuntimeData.TryGetId(addBlockInfo.Coord, out var chunkId, out var blockId))
                                continue;
                            CurEditRuntimeData.RemoveBlockData(chunkId, blockId);
                            _notifyChunkSet.Add(chunkId);
                        }
                    }
                }
            }
        }

        private Vector4 GetInBrushYPoint(Vector2 xzWorldPos, Vector2 yXZPoint, float radius)
        {
            Vector4 inBrushYPoint = Vector4.zero;
            if (VoxelUtility.PointInCircle(xzWorldPos, yXZPoint, radius))
            {
                inBrushYPoint.x = 1;
            }
            if (VoxelUtility.PointInCircle(xzWorldPos, yXZPoint + new Vector2(CurEditRuntimeData.BlockSize.x, 0), radius))
            {
                inBrushYPoint.y = 1;
            }
            if (VoxelUtility.PointInCircle(xzWorldPos, yXZPoint + new Vector2(0, CurEditRuntimeData.BlockSize.z), radius))
            {
                inBrushYPoint.z = 1;
            }
            if (VoxelUtility.PointInCircle(xzWorldPos, yXZPoint + new Vector2(CurEditRuntimeData.BlockSize.x, CurEditRuntimeData.BlockSize.z), radius))
            {
                inBrushYPoint.w = 1;
            }
            return inBrushYPoint;
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
                    if (GUILayout.Button("存檔"))
                    {
                        _editorData.SaveCurTerrainData();
                    }
                    if (GUILayout.Button("輸出"))
                    {
                        _editorData.ExportCurTerrainData();
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
                    _replaceBlock = EditorGUILayout.Toggle("覆蓋既有方塊類型", _replaceBlock);
                    _blockOperateMode = (BlockOperateMode)EditorGUILayout.EnumPopup("模式:", _blockOperateMode);
                    if (_blockOperateMode == BlockOperateMode.Single)
                    {
                        _autoYValueFit = EditorGUILayout.Toggle("自動貼合Y高低值:", _autoYValueFit);
                    }
                    else if (_blockOperateMode == BlockOperateMode.Brush)
                    {
                        DrawBrushSetting();
                    }
                    EditorGUILayout.LabelField("使用方塊類型");
                    if (CurEditRuntimeData.BlockTemplateEditRuntimeData == null)
                    {
                        EditorGUILayout.LabelField("沒有設定方塊範本");
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"當前選擇Id:{_addBlockTemplateId}");
                        var columnCount = 8;
                        var dataCount = CurEditRuntimeData.BlockTemplateEditRuntimeData.BlockTemplateDataList.Count;
                        var rowCount = Mathf.CeilToInt(dataCount / (float)columnCount);
                        var maxWidth = (_editorData.EditorWindow.position.width - 100) / columnCount;
                        _blockTemplateScrollPos = EditorGUILayout.BeginScrollView(_blockTemplateScrollPos, CommonGUIStyle.Default_Box);
                        {
                            for (int i = 0; i < rowCount; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    for (int j = 0; j < columnCount; j++)
                                    {
                                        int index = i * columnCount + j;
                                        if (index >= dataCount)
                                            break;
                                        DrawBlockTemplateScrollCell(CurEditRuntimeData.BlockTemplateEditRuntimeData.BlockTemplateDataList[index], maxWidth);
                                        GUILayout.Space(2f);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                GUILayout.Space(2f);
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else if (_curMouseFunction == MouseFunction.DeleteBlock)
            {
                _blockOperateMode = (BlockOperateMode)EditorGUILayout.EnumPopup("模式:", _blockOperateMode);
                if (_blockOperateMode == BlockOperateMode.Brush)
                {
                    DrawBrushSetting();
                }
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

        private void DrawBrushSetting()
        {
            _brushSize = EditorGUILayout.Slider("筆刷大小:", _brushSize, 1f, 200f);
            _brushWeight = EditorGUILayout.Slider("筆刷強度:", _brushWeight, 0.1f, 1f);
            _brushOnlySetYValue = EditorGUILayout.Toggle("筆刷只設定Y高度:", _brushOnlySetYValue);
        }

        private void DrawBlockTemplateScrollCell(BlockTemplateRuntimeData data, float maxWidth)
        {
            EditorGUILayout.BeginVertical(CommonGUIStyle.SelectableBlueBox(_addBlockTemplateId == data.Id), GUILayout.Width(maxWidth), GUILayout.MaxWidth(maxWidth));
            {
                EditorGUILayout.LabelField($"Id:{data.Id}", GUILayout.MaxWidth(maxWidth));
                EditorGUILayout.BeginHorizontal(CommonGUIStyle.Default_Box);
                {
                    var width = maxWidth - 10;
                    var rect = GUILayoutUtility.GetRect(width, width, GUILayout.Width(width), GUILayout.Height(width));
                    TerrainEditorUtility.DrawBlockTemplatePreview(
                        data,
                        _blockPreviewSetting,
                        CurEditRuntimeData.TerrainMaterial,
                        rect);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            var lastRect = GUILayoutUtility.GetLastRect();
            Event current = Event.current;
            if (lastRect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown && current.button == 0)
                {
                    _addBlockTemplateId = data.Id;
                    Repaint();
                }
            }
        }

        #endregion
    }
}
