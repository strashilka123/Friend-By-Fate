using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SaveManager))]
public class SaveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveManager saveManager = (SaveManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Инструменты отладки", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сохранить"))
        {
            saveManager.SaveGame();
            Debug.Log("Игра сохранена вручную.");
        }

        if (GUILayout.Button("Загрузить"))
        {
            saveManager.LoadGame();
            Debug.Log("Игра загружена.");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Удалить сохранения"))
        {
            saveManager.DeleteSave();
            Debug.Log("Сохранения удалены.");
        }

        EditorGUILayout.Space(10);

        string lastSaveTime = SaveManager.GetLastSaveTime();

        if (!string.IsNullOrEmpty(lastSaveTime))
        {
            EditorGUILayout.HelpBox($"Последнее сохранение: {lastSaveTime}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Сохранений не найдено.", MessageType.Warning);
        }
    }
}