using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueSwitcherForPassages : MonoBehaviour
    {
        [SerializeField] private string[] _disableTags;
        private DialogueStory _dialogueStory;

        private void Start()
        {
            _dialogueStory = FindObjectOfType<DialogueStory>(true);
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory += Disable;
            }
        }

        private void Disable(DialogueStory.Story story)
        {
            if (_disableTags.All(disableTag => story.Tag != disableTag))
                return;

            StartCoroutine(DisableAndLoadCoroutine());
        }

        private IEnumerator DisableAndLoadCoroutine()
        {
            // Ждем 2.5 секунды
            yield return new WaitForSeconds(2.5f);

            if (_dialogueStory != null)
            {
                _dialogueStory.gameObject.SetActive(false);
            }

            // Загружаем следующую сцену (QTEgame)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}