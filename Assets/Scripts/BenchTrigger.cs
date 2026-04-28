using UnityEngine;
using UnityEngine.SceneManagement;

public class BenchTrigger : MonoBehaviour
{
    [SerializeField] private string miniGameSceneName = "MiniGameScene"; // ��� ����� � ����-�����
    // private bool isPlayerNear = false;
    private bool isGameStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���������, ��� � ���� �������� ����� ����� (�� ����)
        if (other.CompareTag("Player") && !isGameStarted)
        {
            // isPlayerNear = true;
            StartMiniGame();
            Debug.Log("������� � �������.");
        }
    }

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        isPlayerNear = false;
    //        Debug.Log("������ �� �������");
    //    }
    //}

    //private void Update()
    //{
    //    // ��������� ������� �������, ���� ����� �����
    //    if (isPlayerNear && !isGameStarted)
    //    {
    //        // ��� ������������ �� ���������� (������� E ��� ������)
    //        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
    //        {
    //            StartMiniGame();
    //        }
    //    }
    //}

    private void StartMiniGame()
    {
        isGameStarted = true;
        Debug.Log("������ ����-����");

        // ��������� ����� ����-����
        SceneManager.LoadScene(miniGameSceneName);
    }
}