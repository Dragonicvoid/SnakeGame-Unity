using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class PlayFromScene : EditorWindow
{
    private static string PlaymodeTargetScene = "Preload";
    private static string CurrentScene;

    [MenuItem("Developer Tools/Play From Start")]
    private static void PlayFromMainMenu()
    {
        if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) { return; } // Save current scene if it has unsaved changes

        // Remember currently active scene
        CurrentScene = SceneManager.GetActiveScene().path;

        // Start playing from the main scene
        if (EditorSceneManager.OpenScene(GetScenePath(PlaymodeTargetScene), OpenSceneMode.Single).IsValid())
        {
            EditorApplication.isPlaying = true;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        else { UnityEngine.Debug.LogError("Scene not found: " + PlaymodeTargetScene); }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (!string.IsNullOrEmpty(CurrentScene)) { EditorSceneManager.OpenScene(CurrentScene); }
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }

    private static string GetScenePath(string sceneName)
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.path.Contains(sceneName)) { return scene.path; }
        }
        return null;
    }
}