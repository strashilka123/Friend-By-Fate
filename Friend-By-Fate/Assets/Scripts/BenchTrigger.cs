using UnityEngine;
using UnityEngine.SceneManagement;

public class BenchTrigger : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = "MiniGameScene"; // Имя сцены с мини-игрой
    // private bool isPlayerNear = false;
    private bool isGameStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что в зону триггера вошел игрок (по тегу)
        if (other.CompareTag("Player") && !isGameStarted)
        {
            // isPlayerNear = true;
            StartMiniGame();
            Debug.Log("Подошел к лавочке.");
        }
    }

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNear = false;
    //        Debug.Log("Отошел от лавочки");
    //    }
    //}

    //private void Update()
    //{
    //    // Проверяем нажатие клавиши, если игрок рядом
    //    if (isPlayerNear && !isGameStarted)
    //    {
    //        // Для тестирования на компьютере (клавиша E или пробел)
    //        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
    //        {
    //            StartMiniGame();
    //        }
    //    }
    //}

    private void StartMiniGame()
    {
        isGameStarted = true;
        Debug.Log("Запуск мини-игры");

        // Загружаем сцену мини-игры
        SceneManager.LoadScene(miniGameSceneName);
    }
}