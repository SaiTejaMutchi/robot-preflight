using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace SpatialGrid.WarehouseGeometry
{
    public sealed class WarehouseGeometryTestController : MonoBehaviour
    {
        [SerializeField] string source = "WarehouseGeometry/warehouse_source.glb";
        [SerializeField] float requiredWidthMeters = 1.915f;
        [SerializeField] ClearanceAxis measurementAxis = ClearanceAxis.Z;
        GltfImport import;
        Transform loadedRoot;
        readonly WarehouseHighlightController highlight = new();
        Vector3 initialCameraPosition;
        Quaternion initialCameraRotation;
        float initialNearClip;
        bool cameraStateCaptured;

        public string Source { get => source; set => source = value; }
        public ClearanceResult? LastResult { get; private set; }
        public Transform LoadedRoot => loadedRoot;
        public bool IsHighlighted => highlight.IsHighlighted;

        async void Start()
        {
            try { await Run(); }
            catch (Exception error) { Debug.LogError($"WAREHOUSE_TEST ERROR {error}", this); }
        }

        public async Task Run()
        {
            await Unload();
            string uri = source.Contains("://") ? source : ToStreamingAssetsUri(source);
            import = new GltfImport();
            var settings = new ImportSettings { NodeNameMethod = NameImportMethod.OriginalUnique };
            if (!await import.Load(uri, settings)) throw new InvalidOperationException($"glTFast could not load {uri}");

            loadedRoot = new GameObject("Warehouse_Loaded_Root").transform;
            loadedRoot.SetParent(transform, false);
            if (!await import.InstantiateMainSceneAsync(loadedRoot)) throw new InvalidOperationException("glTFast could not instantiate the main scene");

            var registry = new WarehouseEntityRegistry(loadedRoot);
            if (!registry.TryResolveUnique("Rack_Left", out var left)
                || !registry.TryResolveUnique("Rack_Right", out var right)
                || !registry.TryResolveUnique("Aisle_01", out var aisle)
                || !registry.TryResolveUnique("Blocker_Region_01", out var blocker))
                throw new InvalidOperationException("Expected unique Rack_Left, Rack_Right, Aisle_01, and Blocker_Region_01 node names");
            if (!WarehouseEntityRegistry.TryGetRendererBounds(left, out var leftBounds) || !WarehouseEntityRegistry.TryGetRendererBounds(right, out var rightBounds))
                throw new InvalidOperationException("Rack nodes contain no renderer geometry");

            var result = AisleClearanceChecker.Check(leftBounds, rightBounds, measurementAxis, requiredWidthMeters);
            result.leftEntityId = "Rack_Left"; result.rightEntityId = "Rack_Right";
            LastResult = result;
            var renderers = loadedRoot.GetComponentsInChildren<Renderer>(true);
            WarehouseEntityRegistry.TryGetRendererBounds(loadedRoot, out var sceneBounds);
            var importedSceneRoot = loadedRoot.childCount > 0 ? loadedRoot.GetChild(0) : loadedRoot;
            Debug.Log($"WAREHOUSE_IMPORT root={importedSceneRoot.name} position={importedSceneRoot.position} rotation={importedSceneRoot.rotation.eulerAngles} scale={importedSceneRoot.lossyScale} boundsCenter={sceneBounds.center} boundsSize={sceneBounds.size} renderers={renderers.Length} entities={registry.Count} duplicates={registry.DuplicateNames.Count}", this);
            Debug.Log($"WAREHOUSE_ENTITIES Rack_Left={left.name} Rack_Right={right.name} Aisle_01={aisle.name} Blocker_Region_01={blocker.name}", this);
            Debug.Log($"WAREHOUSE_TEST required={result.requiredMeters:F3}m available={result.availableMeters:F3}m difference={result.differenceMeters:+0.000;-0.000;0.000}m status={result.status}", this);

            if (result.status != ClearanceStatus.Pass) highlight.Highlight(blocker, new Color(1f,.08f,.02f,1f));
            if (WarehouseEntityRegistry.TryGetRendererBounds(blocker, out var focusBounds) && Camera.main != null)
            {
                CaptureCameraState(Camera.main);
                WarehouseCameraFocus.Focus(Camera.main, focusBounds);
            }
            Debug.Log("WAREHOUSE_ACCEPTANCE legacyBypassed=true compressionRequired=false", this);
        }

        public async Task Unload()
        {
            highlight.Clear();
            LastResult = null;
            RestoreCameraState();
            if (loadedRoot != null)
            {
                Destroy(loadedRoot.gameObject);
                loadedRoot = null;
                await Task.Yield();
            }
            import?.Dispose();
            import = null;
        }

        void CaptureCameraState(Camera camera)
        {
            if (cameraStateCaptured) return;
            initialCameraPosition = camera.transform.position;
            initialCameraRotation = camera.transform.rotation;
            initialNearClip = camera.nearClipPlane;
            cameraStateCaptured = true;
        }

        void RestoreCameraState()
        {
            if (!cameraStateCaptured || Camera.main == null) return;
            Camera.main.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
            Camera.main.nearClipPlane = initialNearClip;
            cameraStateCaptured = false;
        }

        void OnDestroy()
        {
            highlight.Clear();
            RestoreCameraState();
            if (loadedRoot != null) Destroy(loadedRoot.gameObject);
            import?.Dispose();
        }

        static string ToStreamingAssetsUri(string relative)
        {
            string path = Path.Combine(Application.streamingAssetsPath, relative);
            return path.Contains("://") ? path : new Uri(path).AbsoluteUri;
        }
    }
}
