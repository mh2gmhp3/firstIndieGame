using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    [Serializable]
    public class BlockTemplateRuntimeData
    {
        public int Id;

        public Vector2 PXTiling;
        public Vector2 NXTiling;

        public Vector2 PYTiling;
        public Vector2 NYTiling;

        public Vector2 PZTiling;
        public Vector2 NZTiling;

        public TerrainPreviewInfo PreviewInfo = new TerrainPreviewInfo();

        public BlockTemplateRuntimeData(BlockTemplateData data)
        {
            Id = data.Id;
            PXTiling = data.PXTiling;
            NXTiling = data.NXTiling;
            PYTiling = data.PYTiling;
            NYTiling = data.NYTiling;
            PZTiling = data.PZTiling;
            NZTiling = data.NZTiling;
        }

        public BlockTemplateRuntimeData(int id)
        {
            Id = id;

            PXTiling = Vector2.zero;
            NXTiling = Vector2.zero;

            PYTiling = Vector2.zero;
            NYTiling = Vector2.zero;

            PZTiling = Vector2.zero;
            NZTiling = Vector2.zero;
        }

        public void SetTiling(Vector3Int face, Vector2 tiling)
        {
            tiling = new Vector2(Mathf.Clamp01(tiling.x), Mathf.Clamp01(tiling.y));
            //+x
            if (face == Vector3Int.right)
            {
                PXTiling = tiling;
            }
            //-x
            else if (face == Vector3Int.left)
            {
                NXTiling = tiling;
            }
            //+y
            else if (face == Vector3Int.up)
            {
                PYTiling = tiling;
            }
            //-y
            else if (face == Vector3Int.down)
            {
                NYTiling = tiling;
            }
            //+z
            else if (face == Vector3Int.forward)
            {
                PZTiling = tiling;
            }
            //-z
            else if (face == Vector3Int.back)
            {
                NZTiling= tiling;
            }
        }
    }

    public class BlockTemplateEditRuntimeData
    {
        public string Name;

        public Texture2D TileMap;
        public Vector2Int Tiling;
        public Shader Shader;

        public List<BlockTemplateRuntimeData> BlockTemplateDataList = new List<BlockTemplateRuntimeData>();

        public Material Material { get; private set; }

        public BlockTemplateEditRuntimeData(BlockTemplateEditData editData)
        {
            Name = editData.name;
            BlockTemplateDataList.Clear();
            for (int i = 0; i< editData.BlockTemplateDataList.Count; i++)
            {
                BlockTemplateDataList.Add(new BlockTemplateRuntimeData(editData.BlockTemplateDataList[i]));
            }

            TileMap = editData.TileMap;
            Tiling = editData.Tiling;
            Shader = editData.Shader;

            RefreshMaterial();
        }

        public void RefreshMaterial()
        {
            Material = TerrainEditorUtility.GenTerrainMaterial(Shader, TileMap, Tiling);
        }

        public void MarkRefreshAllPreviewTexture()
        {
            for (int i = 0; i < BlockTemplateDataList.Count; i++)
            {
                BlockTemplateDataList[i].PreviewInfo.MarkRefreshTexture();
            }
        }

        public bool TryGetBlockData(int id, out BlockTemplateRuntimeData result)
        {
            for (int i = 0; i < BlockTemplateDataList.Count; i++)
            {
                if (BlockTemplateDataList[i].Id == id)
                {
                    result = BlockTemplateDataList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }

        public int AddBlockData()
        {
            var nextId = GetNextId();
            BlockTemplateDataList.Add(new BlockTemplateRuntimeData(nextId));
            return nextId;
        }

        public void RemoveBlockData(int id)
        {
            for (int i = 0; i < BlockTemplateDataList.Count; i++)
            {
                if (BlockTemplateDataList[i].Id == id)
                {
                    BlockTemplateDataList.RemoveAt(i);
                    break;
                }
            }
        }

        private int GetNextId()
        {
            var maxId = 0;
            for (int i = 0; i < BlockTemplateDataList.Count; i++)
            {
                if (BlockTemplateDataList[i].Id > maxId)
                {
                    maxId = BlockTemplateDataList[i].Id;
                }
            }
            return maxId + 1;
        }
    }
}
