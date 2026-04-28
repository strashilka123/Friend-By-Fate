using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Dialogue;

public class DialogueToQTETrigger : MonoBehaviour
{
    [SerializeField] private string triggerTag = "Smth4";
    private DialogueStory _dialogueStory;

    private void Start()
    {
        _dialogueStory = FindObjectOfType<DialogueStory>(true);
        if (_dialogueStory != null)
        {
            _dialogueStory.ChangedStory += CheckForTransition;
        }
    }

    private void CheckForTransition(DialogueStory.Story story)
    {
        if (story.Tag == triggerTag)
        {
            StartCoroutine(TransitionToNextScene());
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(2f);

        // Отключаем диалоговую систему
        if (_dialogueStory != null)
        {
            _dialogueStory.gameObject.SetActive(false);
        }

        // Отключаем кнопки ответов
        var buttons = FindObjectOfType<AnswerButtons>();
        if (buttons != null)
        {
            buttons.gameObject.SetActive(false);
        }

        // Загружаем следующую сцену (AppartsScene)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void OnDestroy()
    {
        if (_dialogueStory != null)
        {
            _dialogueStory.ChangedStory -= CheckForTransition;
        }
    }
}