using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpatialGrid.WarehouseGeometry
{
    static class WarehouseGeometryTestBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register()
        {
            // AfterSceneLoad fires exactly once, at the first scene loaded this Play
            // session. When Play Mode is entered directly with this scene loaded
            // (the real product path), that is enough. When a Play Mode test loads
            // this scene *after* the Unity Test Framework's own bootstrap scene has
            // already loaded (RT-003), the one-shot callback fires on the wrong
            // scene and never re-checks. Subscribing to sceneLoaded covers both.
            StartTest(SceneManager.GetActiveScene());
            SceneManager.sceneLoaded += (scene, mode) => StartTest(scene);
        }

        static void StartTest(Scene scene)
        {
            if (scene.name != "PlatformVR_GeometryTest") return;
            foreach (var root in scene.GetRootGameObjects())
                if (root.name != "Main Camera" && root.name != "Directional Light") Object.Destroy(root);
            var controller = new GameObject("WarehouseGeometryTest").AddComponent<WarehouseGeometryTestController>();
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), value => value == "--warehouse-controlled"))
                controller.Source = "WarehouseGeometry/warehouse_controlled_0_940m.glb";
        }
    }
}
