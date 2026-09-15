using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public static class WarehouseCameraFocus
    {
        public static void Focus(Camera camera, Bounds bounds)
        {
            float radius = Mathf.Max(bounds.extents.magnitude, .5f);
            var direction = new Vector3(1f, .65f, -1f).normalized;
            camera.transform.position = bounds.center + direction * (radius / Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad) + radius);
            camera.transform.LookAt(bounds.center);
            camera.nearClipPlane = Mathf.Max(.03f, radius / 100f);
        }
    }
}
