using System.Collections.Generic;
using TMPro; // Подключаем пространство имен TextMeshPro
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AddOutlineToDialogue : EditorWindow
{
    [MenuItem("Tools/Dialogue/Add Black Outline to DialogueText")]
    public static void AddOutline()
    {
        string scenesFolder = "Assets/Scenes";
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder });

        int modifiedCount = 0;
        int skippedCount = 0;

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Пропускаем сцены с именем "Main"
            if (path.ToLower().Contains("main"))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool sceneModified = false;

            // Находим все объекты с именем "DialogueText"
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject obj in allObjects)
            {
                // Проверяем, что объект из текущей сцены
                if (obj.scene != scene) continue;

                // Проверяем имя объекта (должно быть точно "DialogueText")
                if (obj.name != "DialogueText") continue;

                // Проверяем наличие скрипта DialogueWindow
                if (!obj.TryGetComponent<Dialogue.DialogueWindow>(out _))
                {
                    // if (!obj.TryGetComponent<DialogueWindow>(out _)) 
                    continue;
                }

                // Получаем компонент текста
                TMP_Text textComponent = obj.GetComponentInChildren<TMP_Text>();
                if (textComponent == null) continue;

                // Проверяем, есть ли уже компонент тени/обводки
                Shadow shadow = textComponent.GetComponent<Shadow>();

                if (shadow == null)
                {
                    // Добавляем компонент Shadow, который будет работать как обводка
                    shadow = textComponent.gameObject.AddComponent<Shadow>();
                    sceneModified = true;
                    Debug.Log($"[Outline Added] to {obj.name} in scene {scene.name}");
                }

                // Настраиваем параметры обводки
                shadow.effectColor = new Color(0f, 0f, 0f, 0.8f); // Черный цвет, прозрачность 0.8
                shadow.effectDistance = new Vector2(2f, -2f); // Размер обводки (можно настроить)
                shadow.useGraphicAlpha = true; // Учитывать прозрачность текста

                EditorUtility.SetDirty(textComponent);
            }

            if (sceneModified)
            {
                EditorSceneManager.SaveScene(scene);
                modifiedCount++;
            }
        }

        EditorUtility.DisplayDialog("Готово",
            $"Обработка завершена.\nДобавлена черная обводка (через эффект тени) к объектам DialogueText.\nИзменено сцен: {modifiedCount}", "OK");

        AssetDatabase.Refresh();
    }
}