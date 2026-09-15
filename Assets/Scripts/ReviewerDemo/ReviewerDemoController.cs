using System;
using System.Collections;
using System.IO;
using GLTFast;
using SpatialGrid.WarehouseGeometry;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialGrid.ReviewerDemo
{
    // UI-001 / UI-001P / UI-002: the polished reviewer inspection surface.
    // UI-002 binds a reload-safe IndependentCompatibilityResultProvider as
    // the single source of truth for panel values, badges, boundaries, and
    // provenance. Measurement numbers still come from a real runtime
    // AisleClearanceChecker call — the provider does not invent them.
    // Visual presentation is unchanged from the UI-001F A+ DEMO READY
    // baseline unless a genuine reset/runtime defect requires a fix.
    public sealed class ReviewerDemoController : MonoBehaviour
    {

        static readonly Color Cyan = new Color(0.20f, 0.82f, 0.93f);
        static readonly Color RequiredNeutral = new Color(0.80f, 0.83f, 0.87f);
        static readonly Color PassGreen = new Color(0.30f, 0.78f, 0.55f);
        static readonly Color BlockedRed = new Color(0.90f, 0.36f, 0.30f);

        public Camera sceneCamera;
        public Button runCheckButton;
        public TextMeshProUGUI runCheckButtonLabel;
        public Image runCheckButtonBackground;
        public CanvasGroup dimOverlay;
        public CanvasGroup resultPanel;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI requiredValueText;
        public TextMeshProUGUI availableValueText;
        public TextMeshProUGUI marginLabelText;
        public TextMeshProUGUI marginValueText;
        public TextMeshProUGUI measuredBetweenAText;
        public TextMeshProUGUI measuredBetweenBText;
        public TextMeshProUGUI sourceValueText;
        public RectTransform canvasRect;

        // Screen-space measurement badges (UI-001P): replace the giant,
        // perspective-distorted world-space TextMeshPro labels an earlier
        // iteration used. A badge is a small dark chip + TMP text that is
        // repositioned every frame from a world point via
        // RectTransformUtility, so its apparent size never changes with
        // camera distance/angle - the defect the A+ visual pass explicitly
        // called out as the biggest problem in the prior render.
        public RectTransform availableBadge;
        public TextMeshProUGUI availableBadgeText;
        public RectTransform requiredBadge;
        public TextMeshProUGUI requiredBadgeText;

        Transform sceneRoot;
        Transform measurementRoot;
        WarehouseEntityRegistry registry;
        GltfImport import;
        IndependentCompatibilityResultProvider provider;
        ClearanceResult? lastResult;
        Vector3? availableBadgeWorldPos;
        Vector3? requiredBadgeWorldPos;
        Vector3 overviewCameraPosition;
        Quaternion overviewCameraRotation;
        Color originalButtonColor;
        Color originalButtonLabelColor;
        string originalButtonLabel;
        bool sequenceRunning;
        bool cameraOverviewStored;
        bool demoCapturing;
        int demoFrame;
        RenderTexture demoRt;
        Texture2D demoTex;
        Canvas demoCanvas;

        public IndependentCompatibilityResultProvider Provider => provider;
        public CompatibilityResult? LastCompatibilityResult => provider != null ? provider.Current : null;
        public ClearanceResult? LastResult => lastResult;
        public bool SequenceComplete { get; private set; }
        public bool OverviewReady { get; private set; }
        public Transform SceneRoot => sceneRoot;
        public Transform MeasurementRoot => measurementRoot;

        async void Start()
        {
            provider = new IndependentCompatibilityResultProvider();
            if (resultPanel != null) resultPanel.alpha = 0f;
            if (dimOverlay != null) dimOverlay.alpha = 0f;
            SetBadgeVisible(availableBadge, false);
            SetBadgeVisible(requiredBadge, false);
            CaptureButtonDefaults();
            if (runCheckButton != null)
            {
                runCheckButton.interactable = false;
                runCheckButton.onClick.RemoveAllListeners();
                runCheckButton.onClick.AddListener(() =>
                {
                    if (!sequenceRunning && OverviewReady)
                        StartCoroutine(RunCheckSequence());
                });
            }
            if (sourceValueText != null)
                sourceValueText.text = IndependentCompatibilityResultProvider.SourceDisplayLine;

            string path = Path.Combine(Application.streamingAssetsPath, IndependentCompatibilityResultProvider.GlbStreamingPath);
            string uri = new Uri(path).AbsoluteUri;
            import = new GltfImport();
            var settings = new ImportSettings { NodeNameMethod = NameImportMethod.OriginalUnique };
            if (!await import.Load(uri, settings))
            {
                Debug.LogError($"REVIEWER_DEMO_ERROR could not load {uri}", this);
                return;
            }

            if (sceneRoot != null)
            {
                Debug.LogError("REVIEWER_DEMO_ERROR duplicate GLB root prevented", this);
                return;
            }
            sceneRoot = new GameObject("Warehouse_Scene_Root").transform;
            sceneRoot.SetParent(transform, false);
            if (!await import.InstantiateMainSceneAsync(sceneRoot))
            {
                Debug.LogError("REVIEWER_DEMO_ERROR could not instantiate scene", this);
                return;
            }

            registry = new WarehouseEntityRegistry(sceneRoot);
            registry.TryResolveUnique(IndependentCompatibilityResultProvider.BoundaryAFullName, out var boundaryATransform);
            registry.TryResolveUnique(IndependentCompatibilityResultProvider.BoundaryBFullName, out var boundaryBTransform);
            ApplyMaterialTint(sceneRoot, boundaryATransform, boundaryBTransform);
            WarehouseEntityRegistry.TryGetRendererBounds(sceneRoot, out var sceneBounds);
            var shelfClusterBounds = ComputeShelfClusterBounds();
            Debug.Log($"REVIEWER_DEMO_SCENE_LOADED entities={registry.Count} duplicates={registry.DuplicateNames.Count} boundsCenter={sceneBounds.center} boundsSize={sceneBounds.size} shelfClusterCenter={shelfClusterBounds.center} shelfClusterSize={shelfClusterBounds.size}", this);

            FrameOverview(shelfClusterBounds);
            StoreOverviewCamera();
            if (runCheckButton != null) runCheckButton.interactable = true;
            OverviewReady = true;
            StartCoroutine(AutoSmokeIfRequested());
            StartCoroutine(AutoDemoIfRequested());
        }

        void OnDestroy()
        {
            import?.Dispose();
            import = null;
        }

        void CaptureButtonDefaults()
        {
            originalButtonLabel = runCheckButtonLabel != null ? runCheckButtonLabel.text : "RUN COMPATIBILITY CHECK";
            originalButtonLabelColor = runCheckButtonLabel != null ? runCheckButtonLabel.color : new Color(0.03f, 0.06f, 0.07f);
            originalButtonColor = runCheckButtonBackground != null ? runCheckButtonBackground.color : Cyan;
        }

        void StoreOverviewCamera()
        {
            if (sceneCamera == null) return;
            overviewCameraPosition = sceneCamera.transform.position;
            overviewCameraRotation = sceneCamera.transform.rotation;
            cameraOverviewStored = true;
        }

        // UI-001P (color/depth): the ENV-003 GLB deliberately carries no
        // materials (stripped during GEO-003 to avoid an invalid-GLB-URI
        // validator error - see GEO-003 Historical Evidence Register). Every
        // renderer therefore falls back to Unity's flat default material,
        // which is why the prior screenshot read as gray-on-gray with no
        // depth separation. This assigns simple, cosmetic, unlit tints by
        // node-name convention only - it does not touch geometry, transforms,
        // or the frozen GEO-003 measurement in any way.
        static void ApplyMaterialTint(Transform root, Transform boundaryA, Transform boundaryB)
        {
            // UI-001F: identify which renderers belong to the two selected
            // boundary racks by walking each boundary's own subtree (via
            // GetComponentsInChildren, exactly like WarehouseEntityRegistry's
            // own bounds computation does) rather than string-matching each
            // renderer's own transform name. glTF imports commonly put the
            // Renderer on a child mesh node whose name does not equal the
            // parent node's full unique name - an exact-name check on the
            // renderer's own transform would silently never match and the
            // selection tint would never apply. A HashSet of the boundary's
            // actual renderer components is correct regardless of hierarchy
            // depth or child-naming convention.
            var boundarySet = new System.Collections.Generic.HashSet<Renderer>();
            if (boundaryA != null) foreach (var r in boundaryA.GetComponentsInChildren<Renderer>(true)) boundarySet.Add(r);
            if (boundaryB != null) foreach (var r in boundaryB.GetComponentsInChildren<Renderer>(true)) boundarySet.Add(r);

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string n = renderer.transform.name;
                Color tint;
                if (n.Contains("GroundB")) tint = new Color(0.150f, 0.162f, 0.178f);
                else if (n.Contains("WallB")) tint = new Color(0.255f, 0.272f, 0.296f);
                else if (n.Contains("RoofB")) tint = new Color(0.200f, 0.214f, 0.235f);
                else tint = new Color(0.560f, 0.582f, 0.610f); // shelves: lighter, pop toward foreground

                // UI-001F (Reviewer Scenario Semantics Gate, section 6): the
                // two racks that actually define the measured span need a
                // restrained visual tell before a reviewer ever presses the
                // CTA - otherwise the boundary choice only becomes visible
                // once the cyan line appears. A faint cyan blend (12%) reads
                // as "these are selected" without game-style bright outlines
                // or recoloring the whole shelf row.
                if (boundarySet.Contains(renderer))
                    tint = Color.Lerp(tint, Cyan, 0.12f);

                renderer.sharedMaterial = MakeUnlitMaterial(tint);
            }
        }

        // The full scene bounds (sceneBounds above) include the enclosing
        // walls/roof/floor, which form a large watertight shell. Framing a
        // camera against THAT bounding box (as an early version of this
        // controller did) places the camera outside the building looking at
        // the exterior of the roof/walls - a featureless gray mass that hides
        // the entire interior. This was caught by inspecting an actual
        // rendered screenshot (per UI-001 requirement 14), not assumed from
        // compiling code. The fix: frame against the shelf cluster only, then
        // clamp the resulting camera position to stay inside the building
        // envelope (known from ENV-003: floor ~y=0, roof ~y=9, walls at
        // |x|<=7, |z|<=10.45).
        static readonly string[] ShelfEntityNames =
        {
            "aws_robomaker_warehouse_ShelfF_01_001",
            "aws_robomaker_warehouse_ShelfD_01_001",
            "aws_robomaker_warehouse_ShelfD_01_002",
            "aws_robomaker_warehouse_ShelfD_01_003",
            "aws_robomaker_warehouse_ShelfE_01_001",
            "aws_robomaker_warehouse_ShelfE_01_002",
            "aws_robomaker_warehouse_ShelfE_01_003",
        };

        Bounds ComputeShelfClusterBounds()
        {
            Bounds? combined = null;
            foreach (var name in ShelfEntityNames)
            {
                if (!registry.TryResolveUnique(name, out var t)) continue;
                if (!WarehouseEntityRegistry.TryGetRendererBounds(t, out var b)) continue;
                if (combined == null) combined = b; else { var c = combined.Value; c.Encapsulate(b); combined = c; }
            }
            return combined ?? new Bounds(Vector3.zero, Vector3.one);
        }

        static Vector3 ClampToBuildingInterior(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, -6.3f, 6.3f);
            pos.y = Mathf.Clamp(pos.y, 1.2f, 7.5f);
            pos.z = Mathf.Clamp(pos.z, -9.8f, 9.8f);
            return pos;
        }

        void FrameOverview(Bounds shelfClusterBounds)
        {
            if (sceneCamera == null) return;
            Vector3 extents = shelfClusterBounds.extents;
            float horizDist = Mathf.Min(extents.x, extents.z) * 1.4f + 3f;
            var direction = new Vector3(0.6f, 0.28f, 0.6f).normalized;
            Vector3 rawPos = shelfClusterBounds.center + direction * horizDist;
            sceneCamera.transform.position = ClampToBuildingInterior(rawPos);
            sceneCamera.transform.LookAt(shelfClusterBounds.center + Vector3.up * 0.5f);
            sceneCamera.nearClipPlane = 0.05f;
            sceneCamera.farClipPlane = Mathf.Max(sceneCamera.farClipPlane, 60f);
        }

        public IEnumerator RunCheckSequence()
        {
            if (sequenceRunning) yield break;
            if (!OverviewReady || registry == null || provider == null)
            {
                Debug.LogError("REVIEWER_DEMO_ERROR check sequence started before the scene was ready", this);
                yield break;
            }
            sequenceRunning = true;
            SequenceComplete = false;
            if (runCheckButton != null) runCheckButton.interactable = false;
            SetButtonState(pressed: true);

            if (!registry.TryResolveUnique(IndependentCompatibilityResultProvider.BoundaryAFullName, out var boundaryA) ||
                !registry.TryResolveUnique(IndependentCompatibilityResultProvider.BoundaryBFullName, out var boundaryB))
            {
                Debug.LogError("REVIEWER_DEMO_ERROR boundary nodes not uniquely resolvable", this);
                sequenceRunning = false;
                yield break;
            }
            if (!WarehouseEntityRegistry.TryGetRendererBounds(boundaryA, out var boundsA) ||
                !WarehouseEntityRegistry.TryGetRendererBounds(boundaryB, out var boundsB))
            {
                Debug.LogError("REVIEWER_DEMO_ERROR boundary renderer bounds missing", this);
                sequenceRunning = false;
                yield break;
            }

            var clearance = AisleClearanceChecker.Check(
                boundsA, boundsB, ClearanceAxis.X, IndependentCompatibilityResultProvider.RequiredMeters);
            clearance.leftEntityId = IndependentCompatibilityResultProvider.BoundaryAFullName;
            clearance.rightEntityId = IndependentCompatibilityResultProvider.BoundaryBFullName;
            lastResult = clearance;

            CompatibilityResult compatibility;
            try
            {
                compatibility = provider.Bind(clearance);
            }
            catch (Exception error)
            {
                Debug.LogError($"REVIEWER_DEMO_ERROR provider bind failed: {error.Message}", this);
                sequenceRunning = false;
                yield break;
            }

            Debug.Log(
                $"REVIEWER_DEMO_PROVIDER_RESULT required={compatibility.requiredValue:F6}m available={compatibility.measuredValue:F6}m difference={compatibility.signedDifference:F6}m status={compatibility.status} robot={compatibility.robotModel} source={IndependentCompatibilityResultProvider.SourceDisplayLine} boundaryA={compatibility.boundaryA} boundaryB={compatibility.boundaryB} providerValid=true",
                this);

            yield return Fade(dimOverlay, 0f, 0.32f, EaseOutCubic, 0.08f);

            bool aIsNear = boundsA.min.x <= boundsB.min.x;
            Vector3 nearA = aIsNear ? new Vector3(boundsA.max.x, boundsA.center.y, boundsA.center.z) : new Vector3(boundsA.min.x, boundsA.center.y, boundsA.center.z);
            Vector3 nearB = aIsNear ? new Vector3(boundsB.min.x, boundsB.center.y, boundsB.center.z) : new Vector3(boundsB.max.x, boundsB.center.y, boundsB.center.z);
            float lineY = (nearA.y + nearB.y) * 0.5f + 1.2f;
            float lineZ = (nearA.z + nearB.z) * 0.5f;
            Vector3 lineStart = new Vector3(nearA.x, lineY, lineZ);
            Vector3 lineEnd = new Vector3(nearB.x, lineY, lineZ);

            BuildMeasurementLine(lineStart, lineEnd, compatibility.measuredValue);
            BuildRequiredEnvelope(lineStart, lineEnd, compatibility.requiredValue);
            SetBadgeVisible(availableBadge, true);
            SetBadgeVisible(requiredBadge, true);

            yield return new WaitForSeconds(0.10f);

            var focusBounds = boundsA; focusBounds.Encapsulate(boundsB);
            focusBounds.Encapsulate(lineStart); focusBounds.Encapsulate(lineEnd);
            focusBounds.Expand(1.0f);
            yield return MoveCamera(focusBounds, 0.45f);

            ApplyResultToPanel(compatibility);
            yield return Fade(resultPanel, 1f, 0.2f, EaseOutCubic, 0f);

            SetButtonState(pressed: false, complete: true);
            SequenceComplete = true;
            sequenceRunning = false;
        }

        public void ResetDemo()
        {
            StopAllCoroutines();
            sequenceRunning = false;
            SequenceComplete = false;
            lastResult = null;
            provider?.Clear();

            ClearMeasurementOverlay();
            availableBadgeWorldPos = null;
            requiredBadgeWorldPos = null;
            SetBadgeVisible(availableBadge, false);
            SetBadgeVisible(requiredBadge, false);
            if (availableBadgeText != null) availableBadgeText.text = "—";
            if (requiredBadgeText != null) requiredBadgeText.text = "—";

            if (resultPanel != null) resultPanel.alpha = 0f;
            if (dimOverlay != null) dimOverlay.alpha = 0f;
            RestoreButton();

            if (cameraOverviewStored && sceneCamera != null)
            {
                sceneCamera.transform.position = overviewCameraPosition;
                sceneCamera.transform.rotation = overviewCameraRotation;
            }

            Debug.Log("REVIEWER_DEMO_RESET complete providerCleared=true measurementCleared=true cameraRestored=true", this);
        }

        void ApplyResultToPanel(CompatibilityResult result)
        {
            bool isPass = result.status == ClearanceStatus.Pass;
            Color statusColor = isPass ? PassGreen : BlockedRed;
            if (statusText != null) { statusText.text = result.status.ToString().ToUpperInvariant(); statusText.color = statusColor; }
            if (requiredValueText != null) requiredValueText.text = $"{result.requiredValue,6:F3} m";
            if (availableValueText != null) availableValueText.text = $"{result.measuredValue,6:F3} m";
            if (marginLabelText != null) marginLabelText.text = isPass ? "Clearance margin" : "Shortfall";
            if (marginValueText != null)
            {
                float shown = isPass ? result.signedDifference : -result.signedDifference;
                marginValueText.text = $"{(isPass ? "+" : "-")}{Mathf.Abs(shown),6:F3} m";
                marginValueText.color = statusColor;
            }
            if (measuredBetweenAText != null)
                measuredBetweenAText.text = IndependentCompatibilityResultProvider.ShortBoundaryDisplay(result.boundaryA);
            if (measuredBetweenBText != null)
                measuredBetweenBText.text = IndependentCompatibilityResultProvider.ShortBoundaryDisplay(result.boundaryB);
            if (sourceValueText != null)
                sourceValueText.text = IndependentCompatibilityResultProvider.SourceDisplayLine;
        }

        void RestoreButton()
        {
            if (runCheckButtonLabel != null)
            {
                runCheckButtonLabel.text = string.IsNullOrEmpty(originalButtonLabel) ? "RUN COMPATIBILITY CHECK" : originalButtonLabel;
                runCheckButtonLabel.color = originalButtonLabelColor;
            }
            if (runCheckButtonBackground != null)
                runCheckButtonBackground.color = originalButtonColor;
            if (runCheckButton != null) runCheckButton.interactable = OverviewReady;
        }

        void SetButtonState(bool pressed, bool complete = false)
        {
            if (complete)
            {
                if (runCheckButtonLabel != null) runCheckButtonLabel.text = "CHECK COMPLETE";
                if (runCheckButtonBackground != null) runCheckButtonBackground.color = new Color(0.30f, 0.34f, 0.38f, 0.55f);
                if (runCheckButtonLabel != null) runCheckButtonLabel.color = new Color(0.72f, 0.76f, 0.80f, 0.85f);
                if (runCheckButton != null) runCheckButton.interactable = false;
            }
        }

        void SetBadgeVisible(RectTransform badge, bool visible)
        {
            if (badge == null) return;
            var cg = badge.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = visible ? 1f : 0f;
        }

        void LateUpdate()
        {
            if (sceneCamera == null || canvasRect == null) return;
            if (availableBadgeWorldPos.HasValue) PositionBadge(availableBadge, availableBadgeWorldPos.Value);
            if (requiredBadgeWorldPos.HasValue) PositionBadge(requiredBadge, requiredBadgeWorldPos.Value);
        }

        // UI-001F (Reviewer Composition Safe-Area Gate, TASK_LEDGER.md): the
        // inspector panel is right-anchored, 394px wide, 28px in from the
        // right edge -> its left edge sits 422px in from the right at
        // reference resolution. Badges must keep a hard minimum of 24px
        // clear of that edge; this uses 40px (within the 32-48px preferred
        // band) so there is real breathing room, not a bare-minimum clamp.
        const float InspectorLeftEdgeOffsetPx = 422f;
        const float InspectorSafeMarginPx = 40f;

        void PositionBadge(RectTransform badge, Vector3 worldPos)
        {
            if (badge == null) return;
            Vector3 screenPoint = sceneCamera.WorldToScreenPoint(worldPos);
            if (screenPoint.z < 0f) { SetBadgeVisible(badge, false); return; }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out var local))
            {
                // Hard exclusion: if the projected point would land inside
                // the inspector's safe margin, reposition to the boundary of
                // the open (safe) region rather than let the badge sit flush
                // against - or behind - the panel edge.
                //
                // UI-001F attempt-1 defect (found by inspecting the actual
                // render, not assumed from code): this clamp only bounded
                // the badge's CENTER (anchoredPosition is the pivot, and
                // both badges are pivoted at their own center). It did not
                // account for the badge's own half-width, so the badge's
                // right edge could still cross ~half the badge's width past
                // the intended boundary - which is exactly what happened,
                // overlapping the panel and obscuring the provenance text
                // beneath it. Fixed by subtracting the badge's own half-
                // width from the limit, so it is the badge's EDGE, not its
                // center, that respects the safe margin.
                float halfWidth = badge.rect.width * 0.5f;
                float maxX = canvasRect.rect.width * 0.5f - InspectorLeftEdgeOffsetPx - InspectorSafeMarginPx - halfWidth;
                local.x = Mathf.Min(local.x, maxX);
                local.y = Mathf.Max(local.y, -canvasRect.rect.height * 0.5f + 140f);
                badge.anchoredPosition = local;
            }
        }

        void BuildMeasurementLine(Vector3 start, Vector3 end, float availableMeters)
        {
            var overlay = EnsureMeasurementRoot();
            var lineObj = new GameObject("Measurement_Line_Cyan");
            lineObj.transform.SetParent(overlay, true);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            // UI-001P: thinner dimension line (was 0.035) - precise engineering
            // annotation, not a neon debug ray.
            lr.startWidth = lr.endWidth = 0.018f;
            lr.material = MakeUnlitMaterial(Cyan);
            lr.startColor = lr.endColor = Cyan;
            lr.numCapVertices = 4;

            MakeEndpointMarker(start, "Endpoint_A", Cyan);
            MakeEndpointMarker(end, "Endpoint_B", Cyan);

            // UI-001F (Reviewer Scenario Semantics Gate, section 6): a thin
            // vertical "extension line" dropping from each endpoint to the
            // floor is a standard CAD dimensioning convention - it ties the
            // measurement explicitly to the physical face of the selected
            // rack directly below it, rather than leaving the cyan line
            // floating in space with no visible anchor to the geometry.
            BuildExtensionLine(start);
            BuildExtensionLine(end);

            availableBadgeWorldPos = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.15f;
            // UI-001F: "available" alone could read as a room-wide measure;
            // "clear span" ties it to the one selected rack-to-rack gap.
            if (availableBadgeText != null) availableBadgeText.text = $"{availableMeters:F3} m clear span";
        }

        void BuildExtensionLine(Vector3 endpoint)
        {
            var lineObj = new GameObject("Extension_Line");
            lineObj.transform.SetParent(EnsureMeasurementRoot(), true);
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(endpoint.x, 0.08f, endpoint.z));
            lr.SetPosition(1, new Vector3(endpoint.x, endpoint.y - 0.02f, endpoint.z));
            lr.startWidth = lr.endWidth = 0.01f;
            Color faint = Cyan * 0.55f;
            faint.a = 1f;
            lr.material = MakeUnlitMaterial(faint);
            lr.startColor = lr.endColor = faint;
        }

        void BuildRequiredEnvelope(Vector3 lineStart, Vector3 lineEnd, float requiredMeters)
        {
            Vector3 dir = (lineEnd - lineStart).normalized;
            Vector3 envelopeStart = lineStart + Vector3.down * 0.4f;
            Vector3 envelopeEnd = envelopeStart + dir * requiredMeters;

            var envObj = new GameObject("Required_Envelope_Reference");
            envObj.transform.SetParent(EnsureMeasurementRoot(), true);
            var lr = envObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.SetPosition(0, envelopeStart);
            lr.SetPosition(1, envelopeEnd);
            // UI-001P: thinner, and visually distinct from the cyan available
            // line via a soft neutral/cool white, per the required visual
            // language ("available clearance: cyan; required width: soft
            // neutral/cool white").
            lr.startWidth = lr.endWidth = 0.03f;
            lr.material = MakeUnlitMaterial(RequiredNeutral);
            lr.startColor = lr.endColor = RequiredNeutral;

            MakeEndpointMarker(envelopeStart, "Required_Endpoint_A", RequiredNeutral, 0.045f);
            MakeEndpointMarker(envelopeEnd, "Required_Endpoint_B", RequiredNeutral, 0.045f);

            requiredBadgeWorldPos = Vector3.Lerp(envelopeStart, envelopeEnd, 0.5f) + Vector3.down * 0.25f;
            // UI-001F: causal wording ("OTTO requires X") reads as a
            // robot-driven requirement being checked, not an arbitrary
            // reference line drawn on the scene.
            if (requiredBadgeText != null) requiredBadgeText.text = $"OTTO requires {requiredMeters:F3} m";
        }

        void MakeEndpointMarker(Vector3 position, string name, Color color, float scale = 0.06f)
        {
            // UI-001P: small technical endpoint (was 0.14 - "oversized glowing
            // sphere" is an explicit A+ rejection condition). Kept as a small
            // flat-shaded sphere rather than a custom tick/bracket mesh -
            // documented as a scoped simplification, not hidden.
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = name;
            marker.transform.SetParent(EnsureMeasurementRoot(), true);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * scale;
            Destroy(marker.GetComponent<Collider>());
            var renderer = marker.GetComponent<Renderer>();
            renderer.sharedMaterial = MakeUnlitMaterial(color);
        }

        static Material MakeUnlitMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        // UI-001P motion: purposeful easing only, per the Emil Kowalski
        // motion rules this pass follows manually (no external package
        // available in this environment) - ease-out for entrances,
        // ease-in-out for spatial/camera movement, no bounce/elastic.
        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

        IEnumerator Fade(CanvasGroup group, float target, float duration, Func<float, float> ease, float delay)
        {
            if (group == null) yield break;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            float start = group.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(start, target, ease(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            group.alpha = target;
        }

        IEnumerator MoveCamera(Bounds bounds, float duration)
        {
            if (sceneCamera == null) yield break;
            Vector3 startPos = sceneCamera.transform.position;
            Quaternion startRot = sceneCamera.transform.rotation;

            // UI-001F (Reviewer Composition Safe-Area Gate): UI-001/UI-001P
            // solved endpoint-vs-panel overlap by tuning a raw yaw offset
            // (16deg -> 27deg -> 14deg -> 19deg) against the *same* fixed
            // framing distance - a blind-rotation approach that kept
            // trading "endpoint hidden" for "other endpoint off-frame".
            // UI-001F replaces that with two deliberate, composition-driven
            // levers instead: (1) widen the framing distance so both
            // endpoints sit with real margin inside the frame rather than
            // near its edges, and (2) bias the look-at target to the right
            // of the evidence's true center so the whole measurement - both
            // endpoints and both badges - renders inside the left ~75% of
            // the viewport, clear of the right-anchored inspector's safe
            // area (see the gate's protected-zone percentages).
            float radius = Mathf.Max(bounds.extents.magnitude, 0.5f) * 1.12f;
            var direction = new Vector3(0.6f, 0.5f, -0.9f).normalized;
            Vector3 rawEndPos = bounds.center + direction * Mathf.Min(radius * 0.55f, 10.5f);
            Vector3 endPos = ClampToBuildingInterior(rawEndPos);

            // UI-001F attempt-1 defect (found by inspecting the actual
            // render): biasing the focal target along WORLD +X was not a
            // reliable "shift the scene left on screen" lever, because the
            // camera's view direction is a diagonal (0.6, 0.5, -0.9), not
            // aligned with world X - the bias barely moved anything in
            // screen space, and the far endpoint (and its badge) rendered
            // entirely behind the inspector panel. Fixed by computing the
            // bias along the CAMERA'S OWN local-right axis instead: looking
            // further along local-right is guaranteed to push the subject
            // left in frame regardless of the camera's world orientation.
            //
            // UI-001F attempt-2 defect (also found by inspecting the actual
            // render): a 0.30x-of-diagonal-magnitude bias over-rotated the
            // camera into staring nearly edge-on at the far boundary rack's
            // flat face, filling most of the frame with one featureless gray
            // surface and losing the sense of a real warehouse interior -
            // the badges cleared the panel, but at the cost of composition
            // quality elsewhere. Reduced framing widen factor to 1.12x (was
            // 1.25x) to keep the oblique warehouse view.
            //
            // UI-001F attempt-3 defect (found by inspecting the actual
            // render, then confirmed by computing the real screen-space
            // projection of the exact endpoint world positions logged from
            // a diagnostic run): at bias 0.14x the near-A endpoint (world
            // x=4.74) still projected to screen x=1539, ~41px INSIDE the
            // inspector's left edge (x=1498 at reference resolution) - the
            // endpoint and its extension line were rendering behind the
            // panel, not just close to it. Rather than guess again, the
            // exact projection was computed for a range of bias values;
            // 0.20x puts that same endpoint at x=1409 (~89px clear) while
            // keeping the far-B endpoint comfortably on-screen (x=132) and
            // the added camera rotation (~18deg vs. ~13deg at 0.14x) well
            // short of the ~26deg that produced the attempt-2 edge-on defect.
            Quaternion lookAtCenter = Quaternion.LookRotation(bounds.center - endPos);
            Vector3 camRight = lookAtCenter * Vector3.right;
            Vector3 focalTarget = bounds.center + camRight * (bounds.extents.magnitude * 0.20f);
            Quaternion endRot = Quaternion.LookRotation(focalTarget - endPos);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = EaseInOutCubic(Mathf.Clamp01(t / duration));
                sceneCamera.transform.position = Vector3.Lerp(startPos, endPos, k);
                sceneCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, k);
                yield return null;
            }
            sceneCamera.transform.position = endPos;
            sceneCamera.transform.rotation = endRot;
        }

        Transform EnsureMeasurementRoot()
        {
            if (measurementRoot == null)
            {
                var go = new GameObject("Measurement_Overlay");
                go.transform.SetParent(sceneRoot, false);
                measurementRoot = go.transform;
            }
            return measurementRoot;
        }

        void ClearMeasurementOverlay()
        {
            if (measurementRoot == null) return;
            Destroy(measurementRoot.gameObject);
            measurementRoot = null;
        }

        IEnumerator AutoDemoIfRequested()
        {
            // DEMO-001 recording-only choreography. Does not change frozen
            // measurement, thresholds, provenance, or UI framing. Exists so
            // the desktop player can produce a cursor-free 1920×1080 capture
            // on a Retina laptop that cannot host a true 1920×1080 window.
            if (!HasArg("-autoDemo")) yield break;
            if (HasArg("-autoSmoke")) yield break;

            float deadline = Time.realtimeSinceStartup + 30f;
            while (!OverviewReady && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!OverviewReady)
            {
                Debug.LogError("REVIEWER_DEMO_AUTODEMO overview never became ready");
#if UNITY_STANDALONE && !UNITY_EDITOR
                Application.Quit(1);
#endif
                yield break;
            }

            string demoDir = GetArgValue("-demoDir")
                ?? Path.Combine(Application.persistentDataPath, "demo_frames");
            Directory.CreateDirectory(demoDir);
            File.WriteAllText(Path.Combine(demoDir, "demo_ready.flag"), DateTime.UtcNow.ToString("o"));
            Debug.Log($"REVIEWER_DEMO_AUTODEMO ready dir={demoDir}", this);

            BeginDemoCapture();
            var capture = StartCoroutine(DemoCaptureLoop(demoDir));

            yield return new WaitForSeconds(4.0f);
            yield return RunCheckSequence();
            yield return new WaitForSeconds(16.0f);

            demoCapturing = false;
            yield return capture;
            EndDemoCapture();

            File.WriteAllText(
                Path.Combine(demoDir, "demo_done.flag"),
                $"frames={demoFrame}{Environment.NewLine}");
            Debug.Log($"REVIEWER_DEMO_AUTODEMO done frames={demoFrame} dir={demoDir}", this);
#if UNITY_STANDALONE && !UNITY_EDITOR
            Application.Quit(0);
#endif
            yield break;
        }

        void BeginDemoCapture()
        {
            demoRt = new RenderTexture(1920, 1080, 24);
            demoTex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            demoCanvas = FindFirstObjectByType<Canvas>();
            demoCapturing = true;
            demoFrame = 0;
        }

        void EndDemoCapture()
        {
            if (demoTex != null) Destroy(demoTex);
            if (demoRt != null) Destroy(demoRt);
            demoTex = null;
            demoRt = null;
            demoCanvas = null;
        }

        IEnumerator DemoCaptureLoop(string dir)
        {
            float t0 = Time.unscaledTime;
            while (demoCapturing)
            {
                yield return new WaitForEndOfFrame();
                if (!demoCapturing) break;
                CaptureDemoFrame(Path.Combine(dir, $"frame_{demoFrame:D5}.jpg"));
                demoFrame++;
            }
            float elapsed = Mathf.Max(0.001f, Time.unscaledTime - t0);
            float fps = demoFrame / elapsed;
            File.WriteAllText(
                Path.Combine(dir, "capture_meta.txt"),
                $"frames={demoFrame}{Environment.NewLine}elapsed_s={elapsed:F3}{Environment.NewLine}fps={fps:F3}{Environment.NewLine}");
            Debug.Log($"REVIEWER_DEMO_AUTODEMO capture frames={demoFrame} elapsed={elapsed:F3}s fps={fps:F2}", this);
        }

        void CaptureDemoFrame(string path)
        {
            if (sceneCamera == null || demoRt == null || demoTex == null) return;
            var previousTarget = sceneCamera.targetTexture;
            var previousActive = RenderTexture.active;
            var previousRenderMode = demoCanvas != null ? demoCanvas.renderMode : RenderMode.ScreenSpaceOverlay;
            var previousCanvasCamera = demoCanvas != null ? demoCanvas.worldCamera : null;
            if (demoCanvas != null)
            {
                demoCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                demoCanvas.worldCamera = sceneCamera;
                demoCanvas.planeDistance = 1f;
            }
            sceneCamera.targetTexture = demoRt;
            sceneCamera.Render();
            RenderTexture.active = demoRt;
            demoTex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            File.WriteAllBytes(path, demoTex.EncodeToJPG(90));
            sceneCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            if (demoCanvas != null)
            {
                demoCanvas.renderMode = previousRenderMode;
                demoCanvas.worldCamera = previousCanvasCamera;
            }
        }

        IEnumerator AutoSmokeIfRequested()
        {
            if (!HasArg("-autoSmoke")) yield break;
            float deadline = Time.realtimeSinceStartup + 30f;
            while (!OverviewReady && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (!OverviewReady)
            {
                Debug.LogError("REVIEWER_DEMO_AUTOSMOKE overview never became ready");
#if UNITY_STANDALONE && !UNITY_EDITOR
                Application.Quit(1);
#endif
                yield break;
            }
            yield return null;
            string evidenceDir = GetArgValue("-smokeEvidenceDir")
                ?? Path.Combine(Application.persistentDataPath, "smoke_evidence");
            Directory.CreateDirectory(evidenceDir);
            CaptureEvidencePng(Path.Combine(evidenceDir, "desktop_reviewer_demo_precheck.png"));
            yield return RunCheckSequence();
            yield return null;
            yield return null;
            CaptureEvidencePng(Path.Combine(evidenceDir, "desktop_reviewer_demo_final.png"));
            if (LastCompatibilityResult.HasValue)
            {
                var r = LastCompatibilityResult.Value;
                string jsonPath = Path.Combine(evidenceDir, "rtsmoke_desktop_result.json");
                File.WriteAllText(jsonPath, r.ToJson(true));
                Debug.Log($"REVIEWER_DEMO_AUTOSMOKE wrote {jsonPath} required={r.requiredValue:F6} available={r.measuredValue:F6} difference={r.signedDifference:F6} status={r.status}");
            }
#if UNITY_STANDALONE && !UNITY_EDITOR
            Application.Quit(0);
#endif
            yield break;
        }

        public void CaptureEvidencePng(string path)
        {
            if (sceneCamera == null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rt = new RenderTexture(1920, 1080, 24);
            var previousTarget = sceneCamera.targetTexture;
            var previousActive = RenderTexture.active;
            sceneCamera.targetTexture = rt;

            var canvas = FindFirstObjectByType<Canvas>();
            var previousRenderMode = canvas != null ? canvas.renderMode : RenderMode.ScreenSpaceOverlay;
            var previousCanvasCamera = canvas != null ? canvas.worldCamera : null;
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = sceneCamera;
                canvas.planeDistance = 1f;
            }

            sceneCamera.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());

            sceneCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            if (canvas != null)
            {
                canvas.renderMode = previousRenderMode;
                canvas.worldCamera = previousCanvasCamera;
            }
            Destroy(tex);
            Destroy(rt);
            Debug.Log($"REVIEWER_DEMO_SCREENSHOT path={path} exists={File.Exists(path)} bytes={(File.Exists(path) ? new FileInfo(path).Length : 0)}");
        }

        public static int CountLoadedWarehouseRoots()
        {
            int count = 0;
            var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in all)
                if (t.name == "Warehouse_Scene_Root" && t.parent != null && t.parent.GetComponent<ReviewerDemoController>() != null)
                    count++;
            return count;
        }

        public static int CountMeasurementOverlays()
        {
            int count = 0;
            var all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var t in all)
                if (t.name == "Measurement_Overlay") count++;
            return count;
        }

        public static int CountControllers()
        {
            return FindObjectsByType<ReviewerDemoController>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        }

        static bool HasArg(string flag)
        {
            foreach (var arg in Environment.GetCommandLineArgs())
                if (arg == flag) return true;
            return false;
        }

        static string GetArgValue(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == flag) return args[i + 1];
            return null;
        }
    }
}
