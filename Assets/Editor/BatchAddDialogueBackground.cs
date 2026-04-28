using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public class BatchAddDialogueBackground : EditorWindow
{
    [MenuItem("Tools/Apply Dark Dialogue Underlay")]
    static void ApplySettings()
    {
        if (!EditorUtility.DisplayDialog("Настройка темного фона",
            "Применить Font Size 38 и очень темный фон (Alpha 0.9)?",
            "Да", "Отмена"))
            return;

        TMP_FontAsset targetFont = null;
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset LiberationSans SDF");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        var scenes = EditorBuildSettings.scenes;
        foreach (var sceneBuild in scenes)
        {
            if (!sceneBuild.enabled) continue;

            var scene = EditorSceneManager.OpenScene(sceneBuild.path);
            var textObjects = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            int count = 0;

            foreach (var text in textObjects)
            {
                // Ищем DialogueText, как в иерархии на скрине
                if (text.gameObject.scene == scene && text.name.Contains("DialogueText"))
                {
                    ConfigureDarkUnderlay(text, targetFont);
                    count++;
                }
            }

            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }
        Debug.Log("🎉 Готово! Фон стал темнее.");
    }

    static void ConfigureDarkUnderlay(TextMeshProUGUI text, TMP_FontAsset font)
    {
        Undo.RecordObject(text, "Apply Dark Underlay");

        if (font != null) text.font = font;

        text.fontSize = 38; 
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;

        if (text.fontSharedMaterial != null)
        {
            Undo.RecordObject(text.fontSharedMaterial, "Update Material Color");

            text.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");

            text.fontSharedMaterial.SetColor("_UnderlayColor", new Color(0.05f, 0.05f, 0.05f, 0.9f));

            text.fontSharedMaterial.SetFloat("_UnderlayOffsetX", 0f);
            text.fontSharedMaterial.SetFloat("_UnderlayOffsetY", 0f);

            // Твои параметры расширения и мягкости
            text.fontSharedMaterial.SetFloat("_UnderlayDilate", 1f);
            text.fontSharedMaterial.SetFloat("_UnderlaySoftness", 0.05f);

            // Убираем стандартную обводку
            text.fontSharedMaterial.SetFloat("_OutlineWidth", 0f);
        }
    }
}