using UnityEngine;
using UnityEngine.SceneManagement;

public class BenchTrigger : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = "MiniGameScene"; 
    private bool isGameStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isGameStarted)
        {
            StartMiniGame();
        }
    }



    private void StartMiniGame()
    {
        isGameStarted = true;
        SceneTransition.LoadScene(miniGameSceneName);
    }
}