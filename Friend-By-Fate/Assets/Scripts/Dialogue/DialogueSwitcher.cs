using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueSwitcher : MonoBehaviour
    {
        [SerializeField] private string[] _disableTags;
        [SerializeField] private GameObject _dialogueWindow;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _newBackground;

        [SerializeField] private playerController _playerController;
        [SerializeField] private Joystick _joystick;
        [SerializeField] private GameObject _playerObject;

        [SerializeField] private GameObject _hintText;

        private DialogueStory _dialogueStory;
        private const string DIALOGUE_COMPLETED_KEY = "DialogueCompleted";
        private static bool _isFirstLoad = true;

        private void Start()
        {
            _dialogueStory = FindObjectOfType<DialogueStory>(true);
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory += Disable;
            }

            // Проверяем, первый ли это запуск сцены
            if (_isFirstLoad)
            {
                // Первый запуск - очищаем сохранение
                PlayerPrefs.DeleteKey(DIALOGUE_COMPLETED_KEY);
                PlayerPrefs.Save();
                _isFirstLoad = false;
                Debug.Log("Первый запуск, диалог будет показан");
            }

            // Проверяем, был ли диалог уже показан
            bool dialogueCompleted = PlayerPrefs.GetInt(DIALOGUE_COMPLETED_KEY, 0) == 1;

            if (dialogueCompleted)
            {
                SkipDialogue();
            }
            else
            {
                StartDialogue();
            }
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

        private void SkipDialogue()
        {
            Debug.Log("Диалог уже был, пропускаем");

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

        private async void Disable(DialogueStory.Story story)
        {
            if (_disableTags.All(disableTag => story.Tag != disableTag))
                return;

            await Task.Delay(1000);

            if (_backgroundImage != null && _newBackground != null)
            {
                _backgroundImage.sprite = _newBackground;
            }

            if (_dialogueWindow != null)
                _dialogueWindow.SetActive(false);
            else
                _dialogueStory.gameObject.SetActive(false);

            PlayerPrefs.SetInt(DIALOGUE_COMPLETED_KEY, 1);
            PlayerPrefs.Save();

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
