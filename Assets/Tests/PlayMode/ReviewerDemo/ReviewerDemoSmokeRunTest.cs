using System.Collections;
using System.IO;
using NUnit.Framework;
using SpatialGrid.ReviewerDemo;
using SpatialGrid.WarehouseGeometry;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SpatialGrid.ReviewerDemo.PlayModeTests
{
    // RT-SMOKE: one clean Play Mode launch of the actual reviewer demo.
    // Invoked three independent times (separate Unity processes) so each
    // run has its own launch evidence. Does not replace UI-002's reset/reload
    // proof and does not expand into RT-004/RT-005.
    public class ReviewerDemoSmokeRunTest
    {
        [UnityTest]
        public IEnumerator OneCleanLaunch_ProducesFrozenPass()
        {
            yield return SceneManager.LoadSceneAsync("ReviewerDemo", LoadSceneMode.Single);
            yield return null;

            ReviewerDemoController controller = null;
            float deadline = Time.realtimeSinceStartup + 30f;
            while (controller == null && Time.realtimeSinceStartup < deadline)
            {
                controller = Object.FindFirstObjectByType<ReviewerDemoController>();
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller);

            while (!controller.OverviewReady && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue(controller.OverviewReady, "warehouse did not load");
            Assert.AreEqual(1, ReviewerDemoController.CountControllers());
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots());
            Assert.IsFalse(controller.Provider.HasResult);

            string roots = "";
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                roots += go.name + ",";
            StringAssert.DoesNotContain("ContentManager", roots);
            StringAssert.DoesNotContain("Login", roots);
            Debug.Log($"REVIEWER_DEMO_SMOKE_LAUNCH roots={roots} legacyStartup=false providerValid=true");

            yield return controller.RunCheckSequence();

            Assert.IsTrue(controller.SequenceComplete);
            var result = controller.LastCompatibilityResult.Value;
            Assert.IsTrue(IndependentCompatibilityResultProvider.MatchesFrozenIndependentMeasurement(result));
            Debug.Log(
                $"REVIEWER_DEMO_SMOKE_RESULT required={result.requiredValue:F6}m available={result.measuredValue:F6}m difference={result.signedDifference:F6}m status={result.status} providerValid=true");

            string shotDir = Path.Combine(Application.dataPath, "..", "evidence", "runtime");
            Directory.CreateDirectory(shotDir);
            controller.CaptureEvidencePng(Path.Combine(shotDir, "rtsmoke_playmode_current.png"));
        }
    }
}
