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
            yield return new WaitForSeconds(3f);

            if (_dialogueStory != null)
            {
                _dialogueStory.gameObject.SetActive(false);
            }

            SceneTransition.LoadNextScene();
        }
    }
}