using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SpatialGrid.ReviewerDemo.Editor
{
    // RT-SMOKE desktop builder. Headless:
    //   Unity -batchmode -nographics -projectPath <p> -executeMethod SpatialGrid.ReviewerDemo.Editor.BuildReviewerDemoStandalone.Build -quit
    // Emits a macOS app that launches directly into ReviewerDemo — no legacy
    // SpatialGrid scenes are included in the player.
    public static class BuildReviewerDemoStandalone
    {
        const string ScenePath = "Assets/Scenes/ReviewerDemo.unity";
        const string OutputPath = "Builds/macos/RobotPreflightReviewerDemo.app";

        public static void Build()
        {
            BuildInternal(BuildOptions.Development, "BUILD_REVIEWER_DEMO_STANDALONE");
        }

        // DEMO-001: same player without the Development watermark, which would
        // contaminate the reviewer-facing recording.
        public static void BuildRelease()
        {
            BuildInternal(BuildOptions.None, "BUILD_REVIEWER_DEMO_RELEASE");
        }

        static void BuildInternal(BuildOptions options, string logTag)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            int prevW = PlayerSettings.defaultScreenWidth;
            int prevH = PlayerSettings.defaultScreenHeight;
            var prevMode = PlayerSettings.fullScreenMode;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneOSX,
                options = options,
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary summary = report.summary;

            PlayerSettings.defaultScreenWidth = prevW;
            PlayerSettings.defaultScreenHeight = prevH;
            PlayerSettings.fullScreenMode = prevMode;

            Debug.Log($"{logTag} result={summary.result} output={OutputPath} sizeBytes={summary.totalSize} errors={summary.totalErrors} warnings={summary.totalWarnings} options={options}");
            if (summary.result != BuildResult.Succeeded)
            {
                foreach (var step in report.steps)
                    foreach (var msg in step.messages)
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                            Debug.LogError($"BUILD_ERROR {msg.content}");
                EditorApplication.Exit(1);
            }
        }
    }
}
