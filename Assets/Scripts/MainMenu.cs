using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        int sceneIndexToLoad;

        // Проверяем, есть ли сохранение прогресса
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            int lastPlayed = SaveManager.Instance.GetLastSceneIndex();
            sceneIndexToLoad = lastPlayed + 1;

            // Проверка на выход за границы
            if (sceneIndexToLoad >= SceneManager.sceneCountInBuildSettings)
            {
                sceneIndexToLoad = 1; 
            }

            Debug.Log($"Загрузка продолжения: сцена {sceneIndexToLoad}");
        }
        else
        {
            sceneIndexToLoad = 1;
            Debug.Log("Новая игра: сцена 1");
        }

        SceneManager.LoadScene(sceneIndexToLoad);
    }

    public void ExitGame()
    {
        Debug.Log("The game is closed");
        Application.Quit();
    }


    public void ClearSaves()
    {
        SaveManager.Instance.DeleteSave();
    }

}