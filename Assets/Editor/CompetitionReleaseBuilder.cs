using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CompetitionReleaseBuilder
{
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    static string RequestPath => Path.Combine(Root, "Library/CompetitionReleaseBuild.request");
    static double nextCheck;
    [Serializable] public class Request { public string outputPath; public string sourceCommit; }
    [Serializable] public class Result
    {
        public bool success;
        public string outputPath, sourceCommit, unityVersion, status;
        public long totalBytes;
        public int errors, warnings;
        public string[] messages;
    }

    static CompetitionReleaseBuilder() { EditorApplication.update += CheckRequest; }

    static void CheckRequest()
    {
        if (EditorApplication.timeSinceStartup < nextCheck || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        nextCheck = EditorApplication.timeSinceStartup + 1;
        if (!File.Exists(RequestPath)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.isPlaying = false; return; }
        var request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
        File.Delete(RequestPath);
        Build(request);
    }

    public static void Build(Request request)
    {
        var result = new Result { outputPath = request.outputPath, sourceCommit = request.sourceCommit, unityVersion = Application.unityVersion };
        string oldProductName = PlayerSettings.productName;
        try
        {
            string output = Path.GetFullPath(request.outputPath);
            if (!output.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("A Windows executable output path is required.");
            if (File.Exists(output)) throw new IOException("Output already exists; choose a new release folder.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var scene = SceneManager.GetActiveScene();
            if (scene.path == "Assets/Scenes/MainMenu.unity" && scene.isDirty) EditorSceneManager.SaveScene(scene);
            PlayerSettings.productName = "PongPongPong";
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/SampleScene.unity" },
                locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
            });
            result.success = report.summary.result == BuildResult.Succeeded;
            result.status = report.summary.result.ToString();
            result.totalBytes = (long)report.summary.totalSize;
            result.errors = (int)report.summary.totalErrors;
            result.warnings = (int)report.summary.totalWarnings;
            result.messages = report.steps.SelectMany(step => step.messages)
                .Where(message => message.type == LogType.Error || message.type == LogType.Exception || message.type == LogType.Warning)
                .Select(message => message.content).Distinct().ToArray();
        }
        catch (Exception error) { result.success = false; result.status = error.ToString(); }
        finally { PlayerSettings.productName = oldProductName; }
        File.WriteAllText(Path.Combine(Root, "Library/CompetitionReleaseBuild.result.json"), JsonUtility.ToJson(result, true));
    }
}
