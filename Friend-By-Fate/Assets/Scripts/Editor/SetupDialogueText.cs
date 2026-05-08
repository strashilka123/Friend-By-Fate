using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetupDialogueText : EditorWindow
{
    [MenuItem("Tools/Dialogue/Setup DialogueText (Font 39 + Black Underlay)")]
    public static void ShowWindow()
    {
        if (EditorUtility.DisplayDialog("Confirm Action",
            "Этот скрипт найдет все объекты 'DialogueText' с компонентом 'DialogueWindow' во всех сценах (кроме Main) и:\n\n" +
            "1. Установит размер шрифта: 39\n" +
            "2. Добавит черную обводку (Underlay): Alpha 0.9, Dilate 1, Softness 0.05\n" +
            "3. Очистит старые эффекты (Shadow/Outline от Unity).\n\n" +
            "Продолжить?", "Да", "Отмена"))
        {
            ProcessScenes();
        }
    }

    private static void ProcessScenes()
    {
        string scenesFolder = "Assets/Scenes";
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder });
        List<string> scenePaths = new List<string>();

        // Сбор путей к сценам
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.ToLower().Contains("main")) continue;
            scenePaths.Add(path);
        }

        int totalModified = 0;

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            int sceneCount = 0;

            foreach (GameObject root in rootObjects)
            {
                // Рекурсивный поиск объектов
                TMP_Text[] textComponents = root.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text tmpText in textComponents)
                {
                    // Проверка: имя объекта должно быть "DialogueText" и должен быть компонент DialogueWindow
                    if (tmpText.name != "DialogueText") continue;

                    if (tmpText.GetComponent<Dialogue.DialogueWindow>() == null) continue;

                    ApplySettings(tmpText);
                    sceneCount++;
                }
            }

            if (sceneCount > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Setup] Scene '{scene.name}': Updated {sceneCount} objects.");
                totalModified += sceneCount;
            }
        }

        Debug.Log($"[Complete] Total objects updated: {totalModified}");
        EditorUtility.DisplayDialog("Success", $"Готово! Изменено объектов: {totalModified}", "OK");
    }

    private static void ApplySettings(TMP_Text tmpText)
    {
        // 1. Очистка старых компонентов Unity UI (если были добавлены ранее)
        Shadow shadowComp = tmpText.GetComponent<Shadow>();
        if (shadowComp != null)
        {
            Object.DestroyImmediate(shadowComp);
        }

        // Примечание: TMP_Outline не существует как отдельный компонент в новых версиях, 
        // он является частью материала или эффекта, поэтому удаляем только стандартные Unity тени.

        // 2. Установка размера шрифта
        tmpText.fontSize = 39;

        // 3. Настройка Underlay (Черная обводка) через SerializedObject
        SerializedObject so = new SerializedObject(tmpText);

        // Включаем поддержку вершинной геометрии (нужно для эффектов)
        // В новых версиях TMP это свойство называется enableVertexGeometry, но оно часто скрыто.
        // Мы просто меняем параметры Underlay, TMP сам включит нужные флаги.

        // Находим свойства подложки
        SerializedProperty underlayColorProp = so.FindProperty("m_underlayColor");
        SerializedProperty underlayDilateProp = so.FindProperty("m_underlayDilate");
        SerializedProperty underlaySoftnessProp = so.FindProperty("m_underlaySoftness");
        SerializedProperty underlayOffsetXProp = so.FindProperty("m_underlayOffsetX");
        SerializedProperty underlayOffsetYProp = so.FindProperty("m_underlayOffsetY");

        if (underlayColorProp != null)
        {
            // Цвет: Черный (0,0,0), Альфа: 0.9
            Color blackHighAlpha = new Color(0f, 0f, 0f, 0.9f);
            underlayColorProp.colorValue = blackHighAlpha;
        }

        if (underlayDilateProp != null)
        {
            underlayDilateProp.floatValue = 1f; // Толщина обводки
        }

        if (underlaySoftnessProp != null)
        {
            underlaySoftnessProp.floatValue = 0.05f; // Мягкость
        }

        // Сбрасываем смещение, чтобы обводка была ровной вокруг текста
        if (underlayOffsetXProp != null) underlayOffsetXProp.floatValue = 0f;
        if (underlayOffsetYProp != null) underlayOffsetYProp.floatValue = 0f;

        so.ApplyModifiedProperties();

        // Принудительно обновляем меш текста, чтобы изменения отобразились сразу
        tmpText.ForceMeshUpdate();

        Debug.Log($"[Updated] Object: {tmpText.gameObject.name} in scene {tmpText.gameObject.scene.name}");
    }
}