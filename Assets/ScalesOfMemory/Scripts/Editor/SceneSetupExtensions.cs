#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneSetupExtensions
{
    [MenuItem("ScalesOfMemory/Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = BattleConstants.Background;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // GameManager (persistent singleton)
        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();

        // MainMenuUI
        var menuGo = new GameObject("MainMenuUI");
        menuGo.AddComponent<MainMenuUI>();

        // Save
        string scenePath = "Assets/ScalesOfMemory/MainMenuScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[ScalesOfMemory] Main Menu scene created at: " + scenePath);
    }

    [MenuItem("ScalesOfMemory/Setup Map Scene")]
    public static void SetupMapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = BattleConstants.Background;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        // MapUI
        var mapGo = new GameObject("MapUI");
        mapGo.AddComponent<MapUI>();

        // Save
        string scenePath = "Assets/ScalesOfMemory/MapScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[ScalesOfMemory] Map scene created at: " + scenePath);
    }

    [MenuItem("ScalesOfMemory/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/ScalesOfMemory/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/ScalesOfMemory/MapScene.unity", true),
            new EditorBuildSettingsScene("Assets/ScalesOfMemory/BattleScene.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[ScalesOfMemory] Build settings updated with 3 scenes (MainMenu, Map, Battle).");
    }

    [MenuItem("ScalesOfMemory/Assign RunConfig to GameManager")]
    public static void AssignRunConfig()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("GameManager not found in scene! Open MainMenuScene first.");
            return;
        }

        string configPath = "Assets/ScalesOfMemory/ScriptableObjects/RunConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<RunConfig>(configPath);
        if (config == null)
        {
            Debug.LogError("RunConfig not found at " + configPath + ". Run 'Create Run Config' first.");
            return;
        }

        var so = new SerializedObject(gm);
        so.FindProperty("runConfig").objectReferenceValue = config;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gm);

        Debug.Log("[ScalesOfMemory] RunConfig assigned to GameManager.");
    }
}
#endif
