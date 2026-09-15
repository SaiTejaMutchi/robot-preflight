using System.Collections.Generic;
using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public sealed class WarehouseHighlightController
    {
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        readonly Dictionary<Renderer, MaterialPropertyBlock> originals = new();
        public bool IsHighlighted => originals.Count > 0;

        public void Highlight(Transform target, Color color)
        {
            Clear();
            foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
            {
                var original = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original);
                originals[renderer] = original;
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor(BaseColor, color);
                block.SetColor(ColorId, color);
                renderer.SetPropertyBlock(block);
            }
        }

        public void Clear()
        {
            foreach (var item in originals)
                if (item.Key != null) item.Key.SetPropertyBlock(item.Value);
            originals.Clear();
        }
    }
}
