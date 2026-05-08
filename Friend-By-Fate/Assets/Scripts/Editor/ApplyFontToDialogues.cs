using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplyFontToAllScenes : EditorWindow
{
    private TMP_FontAsset selectedFont;
    private string scenesFolder = "Assets/Scenes";

    [MenuItem("Tools/Dialogue/Apply Font to All Scenes (Except Main)")]
    public static void ShowWindow()
    {
        GetWindow<ApplyFontToAllScenes>("Font Applier");
    }

    private void OnGUI()
    {
        GUILayout.Label("Mass Font Changer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Select Font Asset", selectedFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);
        if (selectedFont == null)
        {
            EditorGUILayout.HelpBox("Please select a TextMeshPro Font Asset first.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox($"Target Folder: {scenesFolder}\nExcludes any scene with 'Main' in the name.\nApplies to ALL TextMeshPro objects (Buttons, UI, Dialogues).", MessageType.Info);
        }

        GUILayout.Space(20);
        GUI.enabled = (selectedFont != null);
        if (GUILayout.Button("Apply Font to All Scenes", GUILayout.Height(40)))
        {
            ApplyFontToAllScenesLogic();
        }
        GUI.enabled = true;
    }

    private void ApplyFontToAllScenesLogic()
    {
        if (selectedFont == null) return;

        // Получаем список всех сцен в папке
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder });
        List<string> scenePaths = new List<string>();

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Пропускаем сцены с именем "Main" (регистронезависимо)
            if (path.ToLower().Contains("main"))
            {
                Debug.Log($"[Skip] Skipping scene: {path}");
                continue;
            }
            scenePaths.Add(path);
        }

        if (scenePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No scenes found in Assets/Scenes (excluding Main).", "OK");
            return;
        }

        bool proceed = EditorUtility.DisplayDialog("Confirm",
            $"Found {scenePaths.Count} scenes to process.\nThis will modify assets on disk.\nContinue?", "Yes", "Cancel");

        if (!proceed) return;

        int successCount = 0;
        int modifiedObjectsCount = 0;
        Scene originalScene = SceneManager.GetActiveScene();

        try
        {
            foreach (string scenePath in scenePaths)
            {
                try
                {
                    // Открываем сцену в режиме Single (она становится активной)
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    TMP_Text[] allTextObjects = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

                    int sceneModifications = 0;
                    foreach (TMP_Text textObj in allTextObjects)
                    {
                        if (textObj.gameObject.scene != scene) continue;

                        if (textObj.font != selectedFont)
                        {
                            Undo.RecordObject(textObj, "Change Font");
                            textObj.font = selectedFont;

                            textObj.ForceMeshUpdate();

                            sceneModifications++;
                        }
                    }

                    // Сохраняем сцену только если были изменения
                    if (sceneModifications > 0)
                    {
                        EditorSceneManager.SaveScene(scene);
                        Debug.Log($"[Success] Scene '{scene.name}': Modified {sceneModifications} text objects.");
                        modifiedObjectsCount += sceneModifications;
                        successCount++;
                    }
                    else
                    {
                        Debug.Log($"[Info] Scene '{scene.name}': No changes needed.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Error] Failed to process scene {scenePath}: {e.Message}");
                }
            }
        }
        finally
        {
            // Возвращаем оригинальную сцену, если она была другой
            if (!string.IsNullOrEmpty(originalScene.path))
            {
                EditorSceneManager.OpenScene(originalScene.path, OpenSceneMode.Single);
            }
        }

        EditorUtility.DisplayDialog("Complete",
            $"Process finished!\nProcessed: {successCount} scenes.\nTotal objects modified: {modifiedObjectsCount}", "OK");

        AssetDatabase.Refresh();
    }
}