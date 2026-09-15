using SpatialGrid.ReviewerDemo;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpatialGrid.ReviewerDemo.Editor
{
    // UI-001 / UI-001P scene construction. Run headlessly via:
    //   Unity -batchmode -projectPath <p> -executeMethod SpatialGrid.ReviewerDemo.Editor.BuildReviewerDemoScene.Build -quit
    // Builds the scene programmatically (camera, UI hierarchy, controller
    // wiring) rather than hand-authoring .unity YAML, which is error-prone.
    // UI-001P revision: explicit typography roles (brand/eyebrow/title/
    // status/metric-label/metric-value/metadata), tinted-neutral panel,
    // screen-space measurement badges (no more giant world-space text),
    // compact CTA, tighter panel rhythm.
    public static class BuildReviewerDemoScene
    {
        static readonly Color DarkBg = new Color(0.043f, 0.055f, 0.067f, 1f);
        // UI-001P: tinted dark neutral, not near-black, per the Impeccable
        // anti-pattern list ("no pure flat black/gray visual system if
        // tinted neutrals improve depth").
        static readonly Color PanelBg = new Color(0.078f, 0.092f, 0.104f, 0.95f);
        static readonly Color PanelBorder = new Color(1f, 1f, 1f, 0.07f);
        static readonly Color Cyan = new Color(0.20f, 0.82f, 0.93f);
        static readonly Color Green = new Color(0.30f, 0.78f, 0.55f);
        static readonly Color TextPrimary = new Color(0.94f, 0.96f, 0.97f);
        static readonly Color TextSecondary = new Color(0.66f, 0.71f, 0.77f);
        static readonly Color TextMuted = new Color(0.48f, 0.53f, 0.59f);

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = DarkBg;
            cam.fieldOfView = 45f;
            camGo.AddComponent<AudioListener>();

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("ReviewerCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            var canvasRect = canvasGo.GetComponent<RectTransform>();

            // Dim overlay (full-screen, behind the panel/branding, above the 3D scene).
            var dimGroup = CreateFullScreenImage(canvasGo.transform, "DimOverlay", Color.black);
            var dimCanvasGroup = dimGroup.gameObject.AddComponent<CanvasGroup>();
            dimCanvasGroup.alpha = 0f;
            dimCanvasGroup.blocksRaycasts = false;
            dimCanvasGroup.interactable = false;

            // BRAND role: interface identity, not a watermark - full-contrast
            // primary text for the wordmark, secondary (not washed-out) for
            // the descriptor line underneath.
            var brandGo = CreateText(canvasGo.transform, "Brand", "SIGNALWEAVE", 21, TextPrimary, TextAlignmentOptions.TopLeft, FontStyles.Bold, tracking: 2f);
            SetAnchoredRect(brandGo, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -26f), new Vector2(420f, 32f));
            var tagGo = CreateText(canvasGo.transform, "Tagline", "Robot Compatibility Preflight", 13, TextSecondary, TextAlignmentOptions.TopLeft, FontStyles.Normal);
            SetAnchoredRect(tagGo, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -52f), new Vector2(420f, 24f));

            // Robot badge: compact chip, top-right.
            var badgeBgGo = CreatePanel(canvasGo.transform, "RobotBadgeBg", new Color(0.11f, 0.13f, 0.155f, 0.9f));
            SetAnchoredRect(badgeBgGo, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -26f), new Vector2(198f, 38f));
            var badgeTextGo = CreateText(badgeBgGo.transform, "RobotBadgeText", "ROBOT · OTTO 1500", 13, TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold, tracking: 0.5f);
            SetAnchoredRect(badgeTextGo, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // Primary CTA - compact, restrained cyan accent, not an oversized pill.
            var buttonGo = CreatePanel(canvasGo.transform, "RunCheckButton", Cyan);
            SetAnchoredRect(buttonGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(268f, 48f));
            var button = buttonGo.AddComponent<Button>();
            var buttonImage = buttonGo.GetComponent<Image>();
            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(Cyan, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(Cyan, Color.black, 0.12f);
            colors.disabledColor = new Color(0.30f, 0.34f, 0.38f, 0.55f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            var buttonTextGo = CreateText(buttonGo.transform, "Label", "RUN COMPATIBILITY CHECK", 14, new Color(0.03f, 0.06f, 0.07f), TextAlignmentOptions.Center, FontStyles.Bold, tracking: 1f);
            SetAnchoredRect(buttonTextGo, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // Result panel: single panel, no nested cards, subtle border/depth.
            // UI-001F: grew by 35px (437->472) to fit the two new provenance
            // lines added below (Reviewer Scenario Semantics Gate - see
            // TASK_LEDGER.md) without crowding existing content.
            var panelBorderGo = CreatePanel(canvasGo.transform, "ResultPanelBorder", PanelBorder);
            SetAnchoredRect(panelBorderGo, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-27f, 1f), new Vector2(396f, 474f));
            var panelGo = CreatePanel(canvasGo.transform, "ResultPanel", PanelBg);
            SetAnchoredRect(panelGo, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-28f, 0f), new Vector2(394f, 472f));
            var panelCanvasGroup = panelGo.AddComponent<CanvasGroup>();
            panelCanvasGroup.alpha = 0f;

            const float pad = 26f;
            float y = -pad;
            AddPanelText(panelGo.transform, IndependentCompatibilityResultProvider.RobotModel, 21, TextPrimary, FontStyles.Bold, ref y, 26f, pad, 0f);
            y -= 3f;
            AddPanelText(panelGo.transform, "GEOMETRY PREFLIGHT", 12, TextSecondary, FontStyles.Bold, ref y, 18f, pad, 1.6f);
            // UI-001F (Reviewer Scenario Semantics Gate): "check" was vague
            // about what physical feature is evaluated; "requirement" ties it
            // explicitly to the OTTO spec value shown below.
            AddPanelText(panelGo.transform, "One-way aisle-width requirement", 13, TextMuted, FontStyles.Normal, ref y, 22f, pad, 0f);
            y -= 10f;
            var statusText = AddPanelText(panelGo.transform, "—", 27, Green, FontStyles.Bold, ref y, 34f, pad, 0.5f);
            y -= 12f;
            AddPanelDivider(panelGo.transform, ref y, pad);
            y -= 12f;

            var requiredRow = AddPanelRow(panelGo.transform, "Required", ref y, pad);
            var requiredValueText = requiredRow.value;
            // UI-001F: "Available" alone read as if the whole room/aisle
            // network were measured. "Available clear span" ties the number
            // to one selected rack-to-rack gap - see Reviewer Scenario
            // Semantics Gate.
            var availableRow = AddPanelRow(panelGo.transform, "Available clear span", ref y, pad);
            var availableValueText = availableRow.value;
            var marginRow = AddPanelRow(panelGo.transform, "Clearance margin", ref y, pad);
            var marginLabelText = marginRow.label;
            var marginValueText = marginRow.value;

            y -= 12f;
            AddPanelDivider(panelGo.transform, ref y, pad);
            y -= 12f;

            AddPanelText(panelGo.transform, "SOURCE", 10.5f, TextMuted, FontStyles.Bold, ref y, 16f, pad, 1.6f);
            var sourceLine = AddPanelText(panelGo.transform, IndependentCompatibilityResultProvider.SourceDisplayLine, 13.5f, TextPrimary, FontStyles.Normal, ref y, 22f, pad, 0f);
            y -= 8f;
            // UI-001F: "MEASURED BETWEEN" was easy to skim past as metadata;
            // "SELECTED RACK BOUNDARIES" states plainly that a reviewer chose
            // these two specific racks as the measurement's start/end.
            AddPanelText(panelGo.transform, "SELECTED RACK BOUNDARIES", 10.5f, TextMuted, FontStyles.Bold, ref y, 16f, pad, 1.6f);
            var measuredA = AddPanelText(panelGo.transform, "—", 13.5f, TextPrimary, FontStyles.Normal, ref y, 20f, pad, 0f);
            var measuredB = AddPanelText(panelGo.transform, "—", 13.5f, TextPrimary, FontStyles.Normal, ref y, 20f, pad, 0f);
            y -= 6f;
            // UI-001F (Reviewer Scenario Semantics Gate, section 4): small
            // provenance line so the measurement cannot read as an arbitrary
            // room-wide line - it is explicitly one preregistered pair on a
            // named independent dataset.
            AddPanelText(panelGo.transform, "Independent AWS warehouse geometry", 11f, TextMuted, FontStyles.Normal, ref y, 15f, pad, 0.2f);
            AddPanelText(panelGo.transform, "Preregistered boundary pair", 11f, TextMuted, FontStyles.Normal, ref y, 15f, pad, 0.2f);

            // Screen-space measurement badges (UI-001P): small dark chips
            // with camera-facing text, repositioned every frame by the
            // controller via RectTransformUtility - replaces the removed
            // giant world-space TextMeshPro labels.
            var availableBadgeGo = CreateBadge(canvasGo.transform, "AvailableBadge", Cyan);
            var requiredBadgeGo = CreateBadge(canvasGo.transform, "RequiredBadge", new Color(0.80f, 0.83f, 0.87f));

            // Controller wiring.
            var controllerGo = new GameObject("ReviewerDemoController");
            var controller = controllerGo.AddComponent<ReviewerDemoController>();
            controller.sceneCamera = cam;
            controller.runCheckButton = button;
            controller.runCheckButtonLabel = buttonTextGo.GetComponent<TextMeshProUGUI>();
            controller.runCheckButtonBackground = buttonImage;
            controller.dimOverlay = dimCanvasGroup;
            controller.resultPanel = panelCanvasGroup;
            controller.statusText = statusText;
            controller.requiredValueText = requiredValueText;
            controller.availableValueText = availableValueText;
            controller.marginLabelText = marginLabelText;
            controller.marginValueText = marginValueText;
            controller.measuredBetweenAText = measuredA;
            controller.measuredBetweenBText = measuredB;
            controller.sourceValueText = sourceLine;
            controller.canvasRect = canvasRect;
            controller.availableBadge = availableBadgeGo.GetComponent<RectTransform>();
            controller.availableBadgeText = availableBadgeGo.GetComponentInChildren<TextMeshProUGUI>();
            controller.requiredBadge = requiredBadgeGo.GetComponent<RectTransform>();
            controller.requiredBadgeText = requiredBadgeGo.GetComponentInChildren<TextMeshProUGUI>();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            bool saved = EditorSceneManager.SaveScene(scene, "Assets/Scenes/ReviewerDemo.unity");
            Debug.Log($"BUILD_REVIEWER_DEMO_SCENE saved={saved} path=Assets/Scenes/ReviewerDemo.unity");

            RegisterInBuildSettings("Assets/Scenes/ReviewerDemo.unity");
        }

        static GameObject CreateBadge(Transform parent, string name, Color accent)
        {
            var go = CreatePanel(parent, name, new Color(0.06f, 0.07f, 0.085f, 0.85f));
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(190f, 30f);
            var border = go.AddComponent<Outline>();
            border.effectColor = new Color(accent.r, accent.g, accent.b, 0.35f);
            border.effectDistance = new Vector2(1f, -1f);
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            var textGo = CreateText(go.transform, "Text", "—", 13, accent, TextAlignmentOptions.Center, FontStyles.Bold, tracking: 0.2f);
            SetAnchoredRect(textGo, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return go;
        }

        // Appends the scene to EditorBuildSettings if not already present, so
        // SceneManager.LoadSceneAsync("ReviewerDemo") works in Play Mode tests
        // and the scene is available for DEMO-001's eventual desktop build.
        // Existing entries (including PlatformVR_GeometryTest, still required
        // by RT-002/RT-003/GEO-003's own Play Mode tests) are left untouched.
        static void RegisterInBuildSettings(string scenePath)
        {
            var existing = EditorBuildSettings.scenes;
            foreach (var s in existing)
                if (s.path == scenePath) { Debug.Log($"BUILD_REVIEWER_DEMO_SCENE already registered in EditorBuildSettings: {scenePath}"); return; }

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            System.Array.Copy(existing, updated, existing.Length);
            updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
            Debug.Log($"BUILD_REVIEWER_DEMO_SCENE registered in EditorBuildSettings at index {existing.Length}: {scenePath}");
        }

        static Transform CreateFullScreenImage(Transform parent, string name, Color color)
        {
            var go = CreatePanel(parent, name, new Color(color.r, color.g, color.b, 1f));
            SetAnchoredRect(go, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return go.transform;
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        static GameObject CreateText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align, FontStyles style, float tracking = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = style;
            tmp.characterSpacing = tracking;
            return go;
        }

        static void SetAnchoredRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }

        static TextMeshProUGUI AddPanelText(Transform parent, string text, float size, Color color, FontStyles style, ref float y, float height, float pad, float tracking)
        {
            var go = CreateText(parent, "Text_" + text.Replace(" ", "_"), text, size, color, TextAlignmentOptions.TopLeft, style, tracking);
            SetAnchoredRect(go, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(pad, y), new Vector2(-pad * 2f, height));
            y -= height;
            return go.GetComponent<TextMeshProUGUI>();
        }

        static void AddPanelDivider(Transform parent, ref float y, float pad)
        {
            var go = CreatePanel(parent, "Divider", new Color(1f, 1f, 1f, 0.08f));
            SetAnchoredRect(go, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(pad, y), new Vector2(-pad * 2f, 1f));
        }

        static (TextMeshProUGUI label, TextMeshProUGUI value) AddPanelRow(Transform parent, string label, ref float y, float pad)
        {
            const float rowHeight = 28f;
            // UI-001F: 13.5->12.5 so the longer "Available clear span" label
            // (mandated by the Reviewer Scenario Semantics Gate) fits on one
            // line at the same anchor width as the shorter row labels.
            var labelGo = CreateText(parent, "RowLabel_" + label.Replace(" ", "_"), label, 12.5f, TextSecondary, TextAlignmentOptions.TopLeft, FontStyles.Normal);
            SetAnchoredRect(labelGo, new Vector2(0f, 1f), new Vector2(0.55f, 1f), new Vector2(0f, 1f), new Vector2(pad, y), new Vector2(-12f, rowHeight));
            var valueGo = CreateText(parent, "RowValue_" + label.Replace(" ", "_"), "—", 16, TextPrimary, TextAlignmentOptions.TopRight, FontStyles.Bold);
            SetAnchoredRect(valueGo, new Vector2(0.55f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-pad, y), new Vector2(12f, rowHeight));
            y -= rowHeight;
            return (labelGo.GetComponent<TextMeshProUGUI>(), valueGo.GetComponent<TextMeshProUGUI>());
        }
    }
}
