using System.Collections.Generic;
using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public sealed class WarehouseEntityRegistry
    {
        readonly Dictionary<string, Transform> entities = new();
        readonly HashSet<string> duplicates = new();

        public int Count => entities.Count;
        public IReadOnlyCollection<string> DuplicateNames => duplicates;

        public WarehouseEntityRegistry(Transform root)
        {
            foreach (var item in root.GetComponentsInChildren<Transform>(true))
                if (!entities.TryAdd(item.name, item)) duplicates.Add(item.name);
        }

        public bool TryResolveUnique(string id, out Transform entity)
        {
            if (duplicates.Contains(id)) { entity = null; return false; }
            return entities.TryGetValue(id, out entity);
        }

        public bool ContainsDuplicate(string id) => duplicates.Contains(id);

        public static bool TryGetRendererBounds(Transform entity, out Bounds bounds)
        {
            var renderers = entity.GetComponentsInChildren<Renderer>(true);
            bounds = default;
            if (renderers.Length == 0) return false;
            bounds = renderers[0].bounds;
            for (int i=1; i<renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return true;
        }
    }
}
