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
    // UI-002 acceptance: provider is the single UI source of truth, the
    // check can be reset and re-run, and a full scene reload does not leave
    // stale provider/GLB/measurement state. Visual capture is for the
    // UI-001F regression check, not a new polish pass.
    public class ReviewerDemoUITest
    {
        const float FrozenRequired = 1.915f;
        const float FrozenAvailable = 7.509663f;
        const float FrozenDifference = 5.594663f;

        [UnityTest]
        public IEnumerator Provider_Reset_Reload_PreserveFrozenPass()
        {
            yield return SceneManager.LoadSceneAsync("ReviewerDemo", LoadSceneMode.Single);
            yield return null;

            ReviewerDemoController controller = null;
            yield return WaitForController(c => controller = c);
            yield return WaitUntilReady(controller);

            Assert.AreEqual(1, ReviewerDemoController.CountControllers(), "exactly one ReviewerDemoController after first load");
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots(), "exactly one warehouse GLB root after first load");
            Assert.IsNotNull(controller.Provider, "provider must exist after scene load");
            Assert.IsFalse(controller.Provider.HasResult, "provider must start empty");

            Vector3 overviewPos = controller.sceneCamera.transform.position;
            Quaternion overviewRot = controller.sceneCamera.transform.rotation;

            string shotDir = Path.Combine(Application.dataPath, "..", "evidence", "runtime");
            Directory.CreateDirectory(shotDir);
            controller.CaptureEvidencePng(Path.Combine(shotDir, "ui002_reviewer_demo_precheck.png"));

            yield return controller.RunCheckSequence();
            AssertFrozenPresentation(controller, "first check");
            Assert.AreEqual(1, ReviewerDemoController.CountMeasurementOverlays(), "measurement overlay exists after first check");
            Assert.Greater(controller.resultPanel.alpha, 0.9f, "result panel visible after first check");
            Assert.IsNotNull(GameObject.Find("Measurement_Line_Cyan"), "cyan measurement line missing after first check");

            controller.CaptureEvidencePng(Path.Combine(shotDir, "ui002_reviewer_demo_final.png"));

            controller.ResetDemo();
            yield return null;
            yield return null;

            Assert.IsFalse(controller.SequenceComplete, "reset must clear SequenceComplete");
            Assert.IsFalse(controller.Provider.HasResult, "reset must clear the provider result");
            Assert.IsFalse(controller.LastResult.HasValue, "reset must clear LastResult");
            Assert.Less(controller.resultPanel.alpha, 0.05f, "result panel must be hidden after reset");
            Assert.AreEqual(0, ReviewerDemoController.CountMeasurementOverlays(), "measurement overlay must be destroyed on reset");
            Assert.IsNull(GameObject.Find("Measurement_Line_Cyan"), "measurement line survived reset");
            Assert.IsNull(GameObject.Find("Required_Envelope_Reference"), "required envelope survived reset");
            Assert.IsNull(GameObject.Find("Endpoint_A"), "endpoint marker survived reset");
            Assert.IsNull(GameObject.Find("Extension_Line"), "extension line survived reset");
            Assert.AreEqual("RUN COMPATIBILITY CHECK", controller.runCheckButtonLabel.text);
            Assert.IsTrue(controller.runCheckButton.interactable, "CTA must be interactive after reset");
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots(), "reset must not duplicate or destroy the GLB root");
            Assert.AreEqual(1, ReviewerDemoController.CountControllers(), "reset must not duplicate the controller");
            Assert.Less((controller.sceneCamera.transform.position - overviewPos).magnitude, 0.01f, "camera position not restored");
            Assert.Less(Quaternion.Angle(controller.sceneCamera.transform.rotation, overviewRot), 0.25f, "camera rotation not restored");

            yield return controller.RunCheckSequence();
            AssertFrozenPresentation(controller, "second check after reset");
            Assert.AreEqual(1, ReviewerDemoController.CountMeasurementOverlays(), "second check must create exactly one overlay");
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots());

            yield return SceneManager.LoadSceneAsync("ReviewerDemo", LoadSceneMode.Single);
            yield return null;
            controller = null;
            yield return WaitForController(c => controller = c);
            yield return WaitUntilReady(controller);

            Assert.AreEqual(1, ReviewerDemoController.CountControllers(), "reload left duplicate controllers");
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots(), "reload left duplicate GLB roots");
            Assert.AreEqual(0, ReviewerDemoController.CountMeasurementOverlays(), "reload left stale measurement overlay");
            Assert.IsNotNull(controller.Provider, "provider must be recreated after reload");
            Assert.IsFalse(controller.Provider.HasResult, "reloaded provider must start empty");

            yield return controller.RunCheckSequence();
            AssertFrozenPresentation(controller, "third check after full scene reload");
            Assert.AreEqual(1, ReviewerDemoController.CountControllers());
            Assert.AreEqual(1, ReviewerDemoController.CountLoadedWarehouseRoots());
            Assert.AreEqual(1, ReviewerDemoController.CountMeasurementOverlays());
        }

        static IEnumerator WaitForController(System.Action<ReviewerDemoController> assign)
        {
            ReviewerDemoController controller = null;
            float deadline = Time.realtimeSinceStartup + 30f;
            while (controller == null && Time.realtimeSinceStartup < deadline)
            {
                controller = Object.FindFirstObjectByType<ReviewerDemoController>();
                if (controller == null) yield return null;
            }
            Assert.IsNotNull(controller, "ReviewerDemoController not found in ReviewerDemo scene");
            assign(controller);
        }

        static IEnumerator WaitUntilReady(ReviewerDemoController controller)
        {
            float deadline = Time.realtimeSinceStartup + 30f;
            while (controller.runCheckButton != null && !controller.runCheckButton.interactable && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsTrue(controller.OverviewReady, "overview never became ready");
            Assert.IsTrue(controller.runCheckButton.interactable, "Run Compatibility Check button never became interactable (GLB import likely failed)");
        }

        static void AssertFrozenPresentation(ReviewerDemoController controller, string phase)
        {
            Assert.IsTrue(controller.SequenceComplete, $"{phase}: sequence did not complete");
            Assert.IsNotNull(controller.Provider, $"{phase}: provider missing");
            Assert.IsTrue(controller.Provider.HasResult, $"{phase}: provider has no result");
            Assert.IsTrue(controller.LastCompatibilityResult.HasValue, $"{phase}: LastCompatibilityResult empty");

            var result = controller.LastCompatibilityResult.Value;
            Assert.IsTrue(
                IndependentCompatibilityResultProvider.MatchesFrozenIndependentMeasurement(result),
                $"{phase}: provider result is not the frozen GEO-003 PASS");
            Assert.AreEqual(FrozenRequired, result.requiredValue, 0.0005f, $"{phase}: required drifted");
            Assert.AreEqual(FrozenAvailable, result.measuredValue, 0.0005f, $"{phase}: available drifted");
            Assert.AreEqual(FrozenDifference, result.signedDifference, 0.0005f, $"{phase}: difference drifted");
            Assert.AreEqual(ClearanceStatus.Pass, result.status, $"{phase}: status drifted");
            Assert.AreEqual("OTTO 1500", result.robotModel, $"{phase}: robot drifted");
            Assert.AreEqual("OTTO 1500 Spec Sheet", result.machineSourceDocument, $"{phase}: source document drifted");
            Assert.AreEqual(1, result.machineSourcePage, $"{phase}: source page drifted");
            Assert.AreEqual(IndependentCompatibilityResultProvider.BoundaryAFullName, result.boundaryA, $"{phase}: boundary A drifted");
            Assert.AreEqual(IndependentCompatibilityResultProvider.BoundaryBFullName, result.boundaryB, $"{phase}: boundary B drifted");

            Assert.AreEqual("PASS", controller.statusText.text, $"{phase}: status text");
            Assert.AreEqual("Clearance margin", controller.marginLabelText.text, $"{phase}: margin wording");
            Assert.IsTrue(controller.marginValueText.text.StartsWith("+"), $"{phase}: margin sign");
            StringAssert.Contains("1.915", controller.requiredValueText.text, $"{phase}: required text");
            StringAssert.Contains("7.510", controller.availableValueText.text, $"{phase}: available text");
            StringAssert.Contains("5.595", controller.marginValueText.text, $"{phase}: margin text");
            StringAssert.Contains("clear span", controller.availableBadgeText.text, $"{phase}: available badge wording");
            StringAssert.Contains("OTTO requires", controller.requiredBadgeText.text, $"{phase}: required badge wording");
            Assert.AreEqual("ShelfF_01_001", controller.measuredBetweenAText.text, $"{phase}: boundary A display");
            Assert.AreEqual("ShelfD_01_001", controller.measuredBetweenBText.text, $"{phase}: boundary B display");
            Assert.AreEqual("OTTO 1500 Spec Sheet · p.1", controller.sourceValueText.text, $"{phase}: source display");
            Assert.Greater(controller.resultPanel.alpha, 0.9f, $"{phase}: panel not visible");
        }
    }
}
