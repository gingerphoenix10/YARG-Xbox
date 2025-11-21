using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor
{
    public static class MakeTestBuild
    {
        private const string YARG_TEST_BUILD = "YARG_TEST_BUILD";
        private const string YARG_NIGHTLY_BUILD = "YARG_NIGHTLY_BUILD";

        [MenuItem("File/Make Test Build", false, 220)]
        public static void MakeTestBuildClicked()
        {
            MakeBuild(YARG_TEST_BUILD);
        }

        [MenuItem("File/Make Nightly Build", false, 220)]
        public static void MakeNightlyBuildClicked()
        {
            MakeBuild(YARG_NIGHTLY_BUILD);
        }

        [MenuItem("File/Make & Run Nightly Build", false, 220)]
        public static void MakeRunNightlyBuildClicked()
        {
            MakeBuild(YARG_NIGHTLY_BUILD, BuildOptions.AutoRunPlayer);
        }

        [MenuItem("File/Make & Run Nightly Dev Build", false, 220)]
        public static void MakeRunNightlyDevBuildClicked()
        {
            MakeBuild(YARG_NIGHTLY_BUILD, BuildOptions.AutoRunPlayer, BuildOptions.Development);
        }

        public static void MakeBuild(string defineSymbol, params BuildOptions[] options)
        {
            if (options == null)
                options = new BuildOptions[0];

            // Get build settings
            var buildSettings = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(default);

            // Get current defines
            // TODO: BuildTargetGroup is slated for deprecation, figure out how to do this with NamedBuildTarget instead
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var originalDefines);
            originalDefines ??= Array.Empty<string>();

            // Set test build define
            var buildDefines = buildSettings.extraScriptingDefines ?? Array.Empty<string>();
            if (!originalDefines.Contains(defineSymbol) && !buildDefines.Contains(defineSymbol))
            {
                ArrayUtility.Add(ref buildDefines, defineSymbol);
            }

            buildSettings.extraScriptingDefines = buildDefines;

            foreach (BuildOptions option in options)
                buildSettings.options |= option;

            // Build the player
            BuildPipeline.BuildPlayer(buildSettings);
        }
    }
}