using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToAn1 : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 3);
    }



}
