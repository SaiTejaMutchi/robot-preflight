using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SpatialGrid.WarehouseGeometry.PlayModeTests
{
    public class WarehouseGeometryPlayModeTests
    {
        const float TimeoutSeconds = 30f;

        [UnityTest]
        public IEnumerator PublicRackAssembly_RuntimeMeasuresBlockedAtOnePointSevenFiveMeters()
        {
            yield return SceneManager.LoadSceneAsync("PlatformVR_GeometryTest", LoadSceneMode.Single);
            yield return null;

            var scene = SceneManager.GetActiveScene();
            var rootNames = new System.Collections.Generic.List<string>();
            foreach (var root in scene.GetRootGameObjects()) rootNames.Add(root.name);
            Debug.Log($"PLAYMODE_SCENE_ROOTS {string.Join(",", rootNames)}");
            Assert.IsFalse(rootNames.Contains("LoginManager"), "Legacy LoginManager present at scene root");
            Assert.IsFalse(rootNames.Contains("DashBoardManager"), "Legacy DashBoardManager present at scene root");
            Assert.IsFalse(rootNames.Contains("ContentManager"), "Legacy ContentManager present at scene root");

            WarehouseGeometryTestController controller = null;
            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (controller == null && Time.realtimeSinceStartup < deadline)
            {
                controller = Object.FindFirstObjectByType<WarehouseGeometryTestController>();
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller, "WarehouseGeometryTestBootstrap did not create WarehouseGeometryTestController within timeout");

            while (!controller.LastResult.HasValue && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue(controller.LastResult.HasValue, "Controller.Run() did not produce a result within timeout");

            var result = controller.LastResult.Value;
            Debug.Log($"PLAYMODE_RESULT required={result.requiredMeters:F3} available={result.availableMeters:F3} difference={result.differenceMeters:F3} status={result.status}");

            Assert.AreEqual(1.915f, result.requiredMeters, 0.0001f, "required width mismatch");
            Assert.AreEqual(1.750f, result.availableMeters, 0.005f, "available width mismatch");
            Assert.AreEqual(-0.165f, result.differenceMeters, 0.005f, "difference mismatch");
            Assert.AreEqual(ClearanceStatus.Blocked, result.status, "status mismatch");
            Assert.IsTrue(controller.IsHighlighted, "Blocker was not highlighted for a BLOCKED result");
            Assert.IsNotNull(controller.LoadedRoot, "Loaded root transform is null");

            yield return null;
            var cam = Camera.main;
            string shotDir = Path.Combine(Application.dataPath, "..", "evidence", "runtime");
            Directory.CreateDirectory(shotDir);
            string shotPath = Path.Combine(shotDir, "playmode_public_rack_screenshot.png");
            if (cam != null)
            {
                var rt = new RenderTexture(1280, 720, 24);
                var previousTarget = cam.targetTexture;
                var previousActive = RenderTexture.active;
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(shotPath, tex.EncodeToPNG());
                cam.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.Destroy(tex);
                Object.Destroy(rt);
            }
            Debug.Log($"PLAYMODE_SCREENSHOT path={shotPath} exists={File.Exists(shotPath)} bytes={(File.Exists(shotPath) ? new FileInfo(shotPath).Length : 0)}");
        }
    }
}
