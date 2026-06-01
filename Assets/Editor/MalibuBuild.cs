using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class MalibuBuild
{
    const string BuildFolderFull = "Builds/Windows";
    const string BuildFolderDemo = "Builds/WindowsDemo";
    const string ExecutableFull = "Malibu.exe";
    const string ExecutableDemo = "MalibuDemo.exe";

    static readonly string[] ScenesFull =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Juego.unity",
        "Assets/Scenes/Nivel2.unity",
        "Assets/Scenes/EscenaFinal.unity"
    };

    static readonly string[] ScenesDemo =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Juego.unity"
    };

    [MenuItem("Malibu/Build Produccion (Windows)")]
    public static void BuildWindowsFromMenu()
    {
        BuildPlayer(ScenesFull, BuildFolderFull, ExecutableFull, "juego completo");
    }

    [MenuItem("Malibu/Build Demo (Windows)")]
    public static void BuildDemoWindowsFromMenu()
    {
        BuildPlayer(ScenesDemo, BuildFolderDemo, ExecutableDemo, "demo");
    }

    public static void BuildWindows()
    {
        BuildPlayer(ScenesFull, BuildFolderFull, ExecutableFull, "juego completo");
    }

    public static void BuildDemoWindows()
    {
        BuildPlayer(ScenesDemo, BuildFolderDemo, ExecutableDemo, "demo");
    }

    static void BuildPlayer(string[] scenes, string folder, string executable, string label)
    {
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var outputDir = Path.Combine(projectRoot, folder);
        var outputPath = Path.Combine(outputDir, executable);

        Directory.CreateDirectory(outputDir);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log($"Iniciando build de {label} en: {outputPath}");

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build {label} completado ({summary.totalSize / (1024f * 1024f):F1} MB): {outputPath}");
            EditorUtility.RevealInFinder(outputPath);
        }
        else
        {
            Debug.LogError($"Build {label} fallo: " + summary.result);
            EditorApplication.Exit(1);
        }
    }
}
