using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;


/// <summary>
/// Инструмент для применения выбранного шрифта ко всем объектам DialogueWindow и их текстам.
/// Игнорирует сцены с "Main" в названии.
/// </summary>
public class ApplyFontToDialogues : EditorWindow
{
    private TMP_FontAsset selectedFont;
    private Vector2 scrollPosition;
    private List<string> scenePaths = new List<string>();
    [MenuItem("Tools/Dialogue/Apply Font to All Dialogues")]
    public static void ShowWindow()
    {
        GetWindow<ApplyFontToDialogues>("Font Applier");
    }
    private void OnEnable()
    {
        FindAllScenes();
    }
    private void FindAllScenes()
    {
        scenePaths.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().Contains("main"))
            {
                scenePaths.Add(path);
            }
        }
    }
    private void OnGUI()
    {
        GUILayout.Label("Dialogue Font Applier", EditorStyles.boldLabel);
        GUILayout.Space(10);
        selectedFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Target Font Asset", selectedFont, typeof(TMP_FontAsset), false);
        GUILayout.Space(10);
        GUILayout.Label($"Found {scenePaths.Count} scenes (excluding Main).");

        if (selectedFont == null)
        {
            EditorGUILayout.HelpBox("Please select a TMP Font Asset first.", MessageType.Warning);
        }
        GUILayout.Space(10);
        GUI.enabled = selectedFont != null;
        if (GUILayout.Button("Apply Font to All DialogueText Objects"))
        {
            ApplyFontToScenes();
        }
        GUI.enabled = true;
    }
    private void ApplyFontToScenes()
    {
        if (!EditorUtility.DisplayDialog("Confirm",
            $"Are you sure you want to apply font '{selectedFont.name}' to all DialogueText objects in {scenePaths.Count} scenes?",
            "Yes", "No"))
        {
            return;
        }
        int totalChanged = 0;
        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            int changedInScene = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                // Ищем все объекты с компонентом DialogueWindow
                Dialogue.DialogueWindow[] dialogueWindows = root.GetComponentsInChildren<Dialogue.DialogueWindow>(true);
                foreach (var dw in dialogueWindows)
                {
                    // Проверяем, что имя объекта содержит DialogueText (как в задании)
                    // Или можно искать конкретное поле текста внутри скрипта, если оно публичное
                    if (dw.gameObject.name.Contains("DialogueText") || dw.GetComponent<TMP_Text>() != null)
                    {
                        TMP_Text textComp = dw.GetComponent<TMP_Text>();
                        if (textComp == null)
                        {
                            // Пробуем найти в детях, если на самом объекте нет
                            textComp = dw.GetComponentInChildren<TMP_Text>();
                        }
                        if (textComp != null && textComp.font != selectedFont)
                        {
                            textComp.font = selectedFont;
                            EditorUtility.SetDirty(textComp);
                            changedInScene++;
                        }
                    }
                }
            }
            if (changedInScene > 0)
            {
                EditorSceneManager.SaveScene(scene);
                totalChanged += changedInScene;
                Debug.Log($"[Font Applier] Scene {scene.name}: Changed {changedInScene} objects.");
            }
        }
        EditorUtility.DisplayDialog("Done", $"Finished! Total objects changed: {totalChanged}", "OK");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}