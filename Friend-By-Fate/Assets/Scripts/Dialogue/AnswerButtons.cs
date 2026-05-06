using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dialogue
{
    public class AnswerButtons : MonoBehaviour
    {
        [SerializeField] private Button[] _buttons;
        private TMP_Text[] _buttonsText;
        private string[] _currentReplyTags;
        private string[] _nextStoryTags; // Массив для переходов
        private string[] _pointTags;
        private DialogueStory _dialogueStory;
        private DialogueChoiceOutcomeTracker _choiceOutcomeTracker;


        private void Awake()
        {
            _dialogueStory = GetComponent<DialogueStory>();
            _choiceOutcomeTracker = GetComponent<DialogueChoiceOutcomeTracker>();
            _dialogueStory.ChangedStory += ChangeAnswers;

            _buttonsText = new TMP_Text[_buttons.Length];
            _currentReplyTags = new string[_buttons.Length];

            for (int i = 0; i < _buttons.Length; i++)
            {
                int button = i;
                _buttons[i].onClick.AddListener(() => SendAnswer(button));
                _buttonsText[i] = _buttons[i].gameObject.GetComponentInChildren<TMP_Text>();
            }
        }

        private void ChangeAnswers(DialogueStory.Story story)
        {
            Debug.Log("Кнопки обновляются! Тег реплики: " + story.Tag);
            for (int i = 0; i < _buttonsText.Length; i++)
            {
                if (story.Answers.Length <= i)
                {
                    _buttonsText[i].text = null;
                    _buttons[i].interactable = false;
                    continue;
                }

                var answer = story.Answers[i];
                _buttonsText[i].text = answer.Text;
                _nextStoryTags[i] = answer.NextStoryTag;
                _pointTags[i] = answer.PointTag;

                _buttons[i].interactable = true;
            }
        }

        private void SendAnswer(int buttonIndex)
        {
            string pTag = _pointTags[buttonIndex];
            string nTag = _nextStoryTags[buttonIndex];

            _choiceOutcomeTracker?.RegisterChoice(pTag);
            _dialogueStory.ChangeStory(nTag);
        }
    }
}