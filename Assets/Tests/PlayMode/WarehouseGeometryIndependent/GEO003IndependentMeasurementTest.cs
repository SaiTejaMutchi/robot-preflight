using System;
using System.Collections;
using System.IO;
using GLTFast;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SpatialGrid.WarehouseGeometry.IndependentTests
{
    // GEO-003: measures the independently sourced AWS RoboMaker Small Warehouse World
    // aisle, per the frozen method in evidence/environments/warehouse_measurement_preregistration.md.
    // This does not assert an expected required/available/difference/status value -
    // per Rule 4 ("accept the result; do not cherry-pick"), the whole point of this
    // test is to observe and record whatever the deterministic checker produces.
    public class GEO003IndependentMeasurementTest
    {
        const string Source = "WarehouseGeometry/aws_independent_warehouse.glb";
        const string BoundaryA = "aws_robomaker_warehouse_ShelfF_01_001";
        const string BoundaryB = "aws_robomaker_warehouse_ShelfD_01_001";
        const float RequiredMeters = 1.915f;

        [UnityTest]
        public IEnumerator MeasureIndependentAisle()
        {
            string path = Path.Combine(Application.streamingAssetsPath, Source);
            string uri = new Uri(path).AbsoluteUri;

            var import = new GltfImport();
            var settings = new ImportSettings { NodeNameMethod = NameImportMethod.OriginalUnique };
            var loadTask = import.Load(uri, settings);
            while (!loadTask.IsCompleted) yield return null;
            Assert.IsTrue(loadTask.Result, $"glTFast could not load {uri}");

            var root = new GameObject("Independent_Warehouse_Root").transform;
            var instantiateTask = import.InstantiateMainSceneAsync(root);
            while (!instantiateTask.IsCompleted) yield return null;
            Assert.IsTrue(instantiateTask.Result, "glTFast could not instantiate the independent-environment scene");

            var registry = new WarehouseEntityRegistry(root);
            Debug.Log($"GEO003_IMPORT root={root.name} renderers={root.GetComponentsInChildren<Renderer>(true).Length} entities={registry.Count} duplicates={registry.DuplicateNames.Count}", root);

            bool foundA = registry.TryResolveUnique(BoundaryA, out var boundaryA);
            bool foundB = registry.TryResolveUnique(BoundaryB, out var boundaryB);
            Assert.IsTrue(foundA, $"Boundary A '{BoundaryA}' was not uniquely resolvable");
            Assert.IsTrue(foundB, $"Boundary B '{BoundaryB}' was not uniquely resolvable");

            bool boundsA = WarehouseEntityRegistry.TryGetRendererBounds(boundaryA, out var a);
            bool boundsB = WarehouseEntityRegistry.TryGetRendererBounds(boundaryB, out var b);
            Assert.IsTrue(boundsA, $"Boundary A '{BoundaryA}' has no renderer geometry");
            Assert.IsTrue(boundsB, $"Boundary B '{BoundaryB}' has no renderer geometry");

            var result = AisleClearanceChecker.Check(a, b, ClearanceAxis.X, RequiredMeters);
            result.leftEntityId = BoundaryA;
            result.rightEntityId = BoundaryB;

            Debug.Log($"GEO003_BOUNDS boundaryA_min={a.min} boundaryA_max={a.max} boundaryB_min={b.min} boundaryB_max={b.max}", root);
            Debug.Log($"GEO003_INDEPENDENT_RESULT required={result.requiredMeters:F6}m available={result.availableMeters:F6}m difference={result.differenceMeters:+0.000000;-0.000000;0.000000}m status={result.status} boundaryA={BoundaryA} boundaryB={BoundaryB} axis=X sourceFile={Source}", root);

            // Structural acceptance only: a deterministic result was produced from
            // uniquely-resolved, real geometry. The numeric outcome (PASS/BLOCKED/REVIEW)
            // is accepted as observed, not asserted against a pre-chosen expectation.
            Assert.IsTrue(result.requiredMeters > 0f);

            import.Dispose();
            UnityEngine.Object.Destroy(root.gameObject);
            yield return null;
        }
    }
}
