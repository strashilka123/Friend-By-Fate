using TMPro;
using UnityEngine;


namespace Dialogue
{
    public class DialogueWindow : MonoBehaviour
    {
        private TMP_Text _text;
        private DialogueStory _dialogueStory;
        private TypewriterEffect _typewriterEffect;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _dialogueStory = FindObjectOfType<DialogueStory>();
            _typewriterEffect = GetComponent<TypewriterEffect>();
            //_dialogueStory.ChangedStory += ChangeAnswers;
            _typewriterEffect = GetComponent<TypewriterEffect>();

            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory += ChangeAnswers;
            }
        }

        private void ChangeAnswers(DialogueStory.Story story)
        {
            if (string.IsNullOrEmpty(story.Text)) return;

            if (_typewriterEffect != null)
            {
                _typewriterEffect.DisplayText(story.Text);
            }
            else
            {
                _text.text = story.Text;
            }
        }
    }
}
