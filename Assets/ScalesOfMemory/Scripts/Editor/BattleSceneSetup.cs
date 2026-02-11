#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BattleSceneSetup
{
    [MenuItem("ScalesOfMemory/Setup Battle Scene")]
    public static void SetupScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Camera - centered to see both player (bottom-left) and enemies (top-right)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0.5f, 2f, -6f);
            cam.transform.rotation = Quaternion.Euler(5, 0, 0);
            cam.backgroundColor = BattleConstants.Background;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 50f;
        }

        // Ambient light
        RenderSettings.ambientLight = new Color32(30, 30, 50, 255);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        // Directional light
        var lightObj = GameObject.Find("Directional Light");
        if (lightObj != null)
        {
            var light = lightObj.GetComponent<Light>();
            light.color = new Color32(80, 80, 120, 255);
            light.intensity = 0.5f;
        }

        // BattleManager
        var bmGo = new GameObject("BattleManager");
        bmGo.AddComponent<BattleManager>();

        // BattleUI
        var uiGo = new GameObject("BattleUI");
        uiGo.AddComponent<BattleUIManager>();

        // VFX Manager
        var vfxGo = new GameObject("BattleVFX");
        vfxGo.AddComponent<BattleVFX>();

        // Enemy Slots
        var slotsParent = new GameObject("EnemySlots");
        var slot0 = new GameObject("Slot_0");
        slot0.transform.SetParent(slotsParent.transform);
        slot0.transform.position = new Vector3(-2f, 2f, 0f);

        var slot1 = new GameObject("Slot_1");
        slot1.transform.SetParent(slotsParent.transform);
        slot1.transform.position = new Vector3(0f, 2f, 0f);

        var slot2 = new GameObject("Slot_2");
        slot2.transform.SetParent(slotsParent.transform);
        slot2.transform.position = new Vector3(2f, 2f, 0f);

        // Save scene
        string scenePath = "Assets/ScalesOfMemory/BattleScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[ScalesOfMemory] Battle scene created at: " + scenePath);

        Debug.Log("[ScalesOfMemory] Scene created. Run 'ScalesOfMemory > Create All Assets' then 'Assign References'");
    }

    [MenuItem("ScalesOfMemory/Assign References")]
    public static void AssignReferences()
    {
        var bm = Object.FindObjectOfType<BattleManager>();
        if (bm == null)
        {
            Debug.LogError("BattleManager not found in scene!");
            return;
        }

        string soPath = "Assets/ScalesOfMemory/ScriptableObjects";
        var so = new SerializedObject(bm);

        // Starter deck
        string[] cardFiles = { "Strike_1", "Strike_2", "Guard_1", "Guard_2",
            "Fireball", "IceShield", "LightningArrow", "NatureHeal", "DarkSlash", "ManaFocus" };
        var deckProp = so.FindProperty("starterDeck");
        deckProp.arraySize = cardFiles.Length;
        for (int i = 0; i < cardFiles.Length; i++)
        {
            var card = AssetDatabase.LoadAssetAtPath<CardData>(soPath + "/Cards/" + cardFiles[i] + ".asset");
            deckProp.GetArrayElementAtIndex(i).objectReferenceValue = card;
        }

        // Encounters (3 battles: 1 enemy, 2 enemies, 3 enemies)
        var encountersProp = so.FindProperty("encounters");
        encountersProp.arraySize = 3;

        // Encounter 1: Goblin (1 enemy)
        string[][] encounterEnemies = new string[][]
        {
            new string[] { "Goblin" },
            new string[] { "Slime", "Bat" },
            new string[] { "Orc", "Skeleton", "Goblin" }
        };

        for (int e = 0; e < encounterEnemies.Length; e++)
        {
            var encounterElement = encountersProp.GetArrayElementAtIndex(e);
            var enemiesProp = encounterElement.FindPropertyRelative("enemies");
            enemiesProp.arraySize = encounterEnemies[e].Length;

            for (int i = 0; i < encounterEnemies[e].Length; i++)
            {
                var enemy = AssetDatabase.LoadAssetAtPath<EnemyData>(
                    soPath + "/Enemies/" + encounterEnemies[e][i] + ".asset");
                enemiesProp.GetArrayElementAtIndex(i).objectReferenceValue = enemy;
            }
        }

        // Enemy slots
        var slotsParent = GameObject.Find("EnemySlots");
        if (slotsParent != null)
        {
            var slotsProp = so.FindProperty("enemySlots");
            int slotCount = slotsParent.transform.childCount;
            slotsProp.arraySize = slotCount;
            for (int i = 0; i < slotCount; i++)
            {
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                    slotsParent.transform.GetChild(i);
            }
        }

        // Status effects
        so.FindProperty("burnData").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StatusEffectData>(soPath + "/StatusEffects/Burn.asset");
        so.FindProperty("freezeData").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StatusEffectData>(soPath + "/StatusEffects/Freeze.asset");
        so.FindProperty("poisonData").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<StatusEffectData>(soPath + "/StatusEffects/Poison.asset");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(bm);

        Debug.Log("[ScalesOfMemory] All references assigned! 3 encounters configured.");
    }
}
#endif
