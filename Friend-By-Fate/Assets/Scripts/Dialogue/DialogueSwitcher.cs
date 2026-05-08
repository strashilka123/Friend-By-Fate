using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue
{
    public class DialogueSwitcher : MonoBehaviour
    {
        [SerializeField] private string[] _disableTags;
        [SerializeField] private GameObject _dialogueWindow;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _newBackground;

        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Joystick _joystick;
        [SerializeField] private GameObject _playerObject;

        [SerializeField] private GameObject _hintText;

        // Время задержки перед завершением диалога (в секундах)
        [SerializeField] private float _endDelay = 3f;

        private DialogueStory _dialogueStory;

        private void Start()
        {
            _dialogueStory = FindObjectOfType<DialogueStory>(true);
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory += Disable;
            }
            StartDialogue();
        }

        private void StartDialogue()
        {
            if (_playerController != null)
                _playerController.IsPaused = true;

            if (_joystick != null)
                _joystick.gameObject.SetActive(false);

            if (_dialogueWindow != null)
                _dialogueWindow.SetActive(true);

            if (_playerObject != null)
                _playerObject.SetActive(false);

            if (_hintText != null)
                _hintText.SetActive(false);
        }

        private async void Disable(DialogueStory.Story story)
        {
            if (_disableTags.All(disableTag => story.Tag != disableTag))
                return;

            await Task.Delay((int)(_endDelay * 1000));

            if (_backgroundImage != null && _newBackground != null)
            {
                _backgroundImage.sprite = _newBackground;
            }

            if (_dialogueWindow != null)
                _dialogueWindow.SetActive(false);
            else if (_dialogueStory != null)
                _dialogueStory.gameObject.SetActive(false);

            if (_playerController != null)
                _playerController.IsPaused = false;

            if (_joystick != null)
                _joystick.gameObject.SetActive(true);

            if (_playerObject != null)
                _playerObject.SetActive(true);

            if (_hintText != null)
                _hintText.SetActive(true);
        }

        private void OnDestroy()
        {
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory -= Disable;
            }
        }
    }
}