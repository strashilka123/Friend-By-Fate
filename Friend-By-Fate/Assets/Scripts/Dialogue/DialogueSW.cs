using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public class DialogueSW : MonoBehaviour
    {
        [SerializeField] private string targetTag = "NPC2";
        [SerializeField] private string cardSceneName = "CardScene";
        [SerializeField] private float transitionDelay = 2f;
        [SerializeField] private bool disableDialogueBeforeLoad = true;

        private DialogueStory dialogueStory;
        private bool transitionStarted;

        private void Start()
        {
            dialogueStory = GetComponent<DialogueStory>();
            if (dialogueStory == null)
            {
                dialogueStory = FindObjectOfType<DialogueStory>(true);
            }

            if (dialogueStory != null)
            {
                dialogueStory.ChangedStory += CheckForCardSceneTransition;
            }
            else
            {
                Debug.LogWarning("[DialogueSW] DialogueStory не найден. Переход к сцене карт не будет работать.", this);
            }
        }

        private void CheckForCardSceneTransition(DialogueStory.Story story)
        {
            if (transitionStarted || story.Tag != targetTag)
            {
                return;
            }

            transitionStarted = true;
            StartCoroutine(LoadCardSceneAfterDelay());
        }

        public void LoadNextSceneByButton()
        {
            if (transitionStarted)
            {
                return;
            }

            transitionStarted = true;
            StartCoroutine(LoadNextSceneAfterDelay());
        }

        private IEnumerator LoadNextSceneAfterDelay()
        {
            yield return WaitBeforeTransition();
            //DisableDialogueIfNeeded();
            SceneTransition.LoadNextScene();
        }

        private IEnumerator WaitBeforeTransition()
        {
            if (transitionDelay > 0f)
            {
                yield return new WaitForSeconds(transitionDelay);
            }
        }

        //private void DisableDialogueIfNeeded()
        //{
        //    if (disableDialogueBeforeLoad && dialogueStory != null)
        //    {
        //        dialogueStory.gameObject.SetActive(false);
        //    }
        //}

        private IEnumerator LoadCardSceneAfterDelay()
        {
            if (transitionDelay > 0f)
            {
                yield return new WaitForSeconds(transitionDelay);
            }

            if (disableDialogueBeforeLoad && dialogueStory != null)
            {
                dialogueStory.gameObject.SetActive(false);
            }

            SceneTransition.LoadScene(cardSceneName);
        }

        private void OnDestroy()
        {
            if (dialogueStory != null)
            {
                dialogueStory.ChangedStory -= CheckForCardSceneTransition;
            }
        }
    }
}
