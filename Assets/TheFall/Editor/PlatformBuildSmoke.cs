using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TheFall.Editor
{
    public static class PlatformBuildSmoke
    {
        private const string BuildRootEnvironmentVariable = "THE_FALL_BUILD_ROOT";

        [MenuItem("The Fall/Validation/Build Smoke/macOS")]
        public static void BuildMacOS()
        {
            Build(BuildTarget.StandaloneOSX, "macOS/TheFall.app");
        }

        [MenuItem("The Fall/Validation/Build Smoke/Windows")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Windows/TheFall.exe");
        }

        [MenuItem("The Fall/Validation/Build Smoke/Android")]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, "Android/TheFall.apk");
        }

        [MenuItem("The Fall/Validation/Build Smoke/iOS")]
        public static void BuildIOS()
        {
            Build(BuildTarget.iOS, "iOS");
        }

        private static void Build(BuildTarget target, string relativeOutputPath)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                throw new BuildFailedException(
                    $"Unity {Application.unityVersion} does not have build support installed for {target}. " +
                    "Install the matching Unity Hub platform module and required native toolchain before retrying.");
            }

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured for a build smoke.");
            }

            var buildRoot = Environment.GetEnvironmentVariable(BuildRootEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(buildRoot))
            {
                buildRoot = Path.Combine(Directory.GetCurrentDirectory(), "Build", "Smoke");
            }

            var outputPath = Path.GetFullPath(Path.Combine(buildRoot, relativeOutputPath));
            var outputDirectory = target == BuildTarget.iOS
                ? outputPath
                : Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new BuildFailedException($"Unable to resolve the build output directory for {outputPath}.");
            }

            Directory.CreateDirectory(outputDirectory);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.Development,
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{target} build smoke failed with {report.summary.totalErrors} errors and " +
                    $"{report.summary.totalWarnings} warnings. Output: {outputPath}");
            }

            Debug.Log(
                $"The Fall {target} build smoke succeeded: {report.summary.totalSize} bytes in " +
                $"{report.summary.totalTime}. Output: {outputPath}");
        }
    }
}
