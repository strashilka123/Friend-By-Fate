using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using System.Linq;

#if UNITY_EDITOR
public class RemoveDialogueTextSettings : EditorWindow
{
    [MenuItem("Tools/🔙 REMOVE: Сбросить настройки DialogueText")]
    static void RemoveSettings()
    {
        if (!EditorUtility.DisplayDialog("Сброс настроек",
            "Скрипт сбросит настройки всех DialogueText в ТЕКУЩЕЙ сцене.\n\n💡 Рекомендуется сделать копию сцены!",
            "Сбросить", "Отмена"))
            return;

        Undo.SetCurrentGroupName("Remove Dialogue Text Settings");
        int group = Undo.GetCurrentGroup();

        var textObjects = FindObjectsOfType<TextMeshProUGUI>()
            .Where(t => t.name.Contains("DialogueText"))
            .ToArray();

        int count = 0;
        foreach (var text in textObjects)
        {
            Undo.RecordObject(text, "Reset Text Settings");

            // Сброс настроек к дефолтным
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.enableAutoSizing = false;

            count++;
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"✅ Сброшено настроек: {count}");
    }

    [MenuItem("Tools/🔙 REMOVE ALL: Сбросить во ВСЕХ сценах")]
    static void RemoveAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Сброс во всех сценах",
            "Скрипт сбросит настройки DialogueText во ВСЕХ сценах из Build Settings.\n\n⚠️ Сделайте бэкап!",
            "Сбросить все", "Отмена"))
            return;

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        int total = 0;

        foreach (string scenePath in scenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            var textObjects = FindObjectsOfType<TextMeshProUGUI>()
                .Where(t => t.name.Contains("DialogueText"))
                .ToArray();

            int sceneCount = 0;
            foreach (var text in textObjects)
            {
                Undo.RecordObject(text, "Reset Text Settings");
                text.fontSize = 36;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.enableWordWrapping = true;
                text.enableAutoSizing = false;
                sceneCount++;
            }

            if (sceneCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"✅ {scene.name}: сброшено {sceneCount} текстов");
            }

            EditorSceneManager.CloseScene(scene, true);
            total += sceneCount;
        }

        Debug.Log($"🎉 Готово! Всего сброшено: {total}");
    }
}
#endif