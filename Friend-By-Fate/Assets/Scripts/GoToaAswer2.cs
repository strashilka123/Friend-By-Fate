using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToAn2 : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }



}
