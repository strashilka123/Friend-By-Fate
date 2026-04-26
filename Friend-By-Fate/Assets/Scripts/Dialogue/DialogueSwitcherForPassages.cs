using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
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
            _dialogueStory.ChangedStory += Disable;
        }

        private async void Disable(DialogueStory.Story story)
        {
            if (_disableTags.All(disableTag => story.Tag != disableTag))
                return;

            await Task.Delay(2500);

            _dialogueStory.gameObject.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
