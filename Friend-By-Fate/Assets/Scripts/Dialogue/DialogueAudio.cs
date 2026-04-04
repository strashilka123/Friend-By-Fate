using UnityEngine;

namespace Dialogue
{
    [RequireComponent(typeof(AudioSource))]
    public class DialogueAudio : MonoBehaviour
    {
        private AudioSource _audioSource;
        private DialogueStory _dialogueStory;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _dialogueStory = GetComponent<DialogueStory>();
            _dialogueStory.ChangedStory += PlayVoice;
        }

        private void PlayVoice(DialogueStory.Story story)
        {
            _audioSource.Stop();
            if (story.VoiceClip != null)
            {
                _audioSource.clip = story.VoiceClip;
                _audioSource.Play();
            }
        }
    }
}