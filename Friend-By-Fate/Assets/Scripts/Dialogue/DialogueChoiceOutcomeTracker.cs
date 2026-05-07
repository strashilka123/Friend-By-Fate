using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueChoiceOutcomeTracker : MonoBehaviour
    {
        [SerializeField] private int _startingValue;
        [SerializeField] private int _goodEndingThreshold = 5;
        [SerializeField] private int _neutralEndingThreshold = -10;
        [SerializeField] private ChoiceImpact[] _trackedChoices;
        [SerializeField] private bool _applyChoiceImpactOnlyOnce = true;
        [SerializeField] private string _endTag = "END";
        [SerializeField] private string _goodEndingSceneName = "GoodEnd";
        [SerializeField] private string _badEndingSceneName = "BadEnd";

        private readonly HashSet<string> _appliedChoiceTags = new();
        private Dictionary<string, int> _choiceDeltaByReplyTag;
        private DialogueStory _dialogueStory;

        public int CurrentValue { get; private set; }

        public event Action<int> ValueChanged;

        [Serializable]
        public struct ChoiceImpact
        {
            [field: SerializeField] public string ReplyTag { get; private set; }
            [field: SerializeField] public int Delta { get; private set; }
        }

        private void Awake()
        {
            _choiceDeltaByReplyTag = new Dictionary<string, int>();

            foreach (var choiceImpact in _trackedChoices)
            {
                if (string.IsNullOrWhiteSpace(choiceImpact.ReplyTag))
                {
                    continue;
                }

                _choiceDeltaByReplyTag[choiceImpact.ReplyTag] = choiceImpact.Delta;
            }

            CurrentValue = _startingValue;
            _dialogueStory = GetComponent<DialogueStory>();

            SaveManager.ProgressReset += ResetProgress;
            Debug.Log($"[DialogueChoiceOutcomeTracker] Initialized with start value {CurrentValue}");
        }

        private void OnEnable()
        {
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory += OnStoryChanged;
            }
        }

        private void OnDisable()
        {
            if (_dialogueStory != null)
            {
                _dialogueStory.ChangedStory -= OnStoryChanged;
            }
        }

        private void OnDestroy()
        {
            SaveManager.ProgressReset -= ResetProgress;
            Debug.Log("[DialogueChoiceOutcomeTracker] Unsubscribed from SaveManager.ProgressReset.");
        }

        private void OnStoryChanged(DialogueStory.Story story)
        {
            if (string.Equals(story.Tag, _endTag, StringComparison.OrdinalIgnoreCase))
            {
                EvaluateAndLoadEnding();
            }
        }

        public void ResetProgress()
        {
            CurrentValue = _startingValue;
            _appliedChoiceTags.Clear();
            ValueChanged?.Invoke(CurrentValue);
            Debug.Log("Прогресс выборов сброшен.");
        }

        public void RegisterChoice(string replyTag)
        {
            if (string.IsNullOrWhiteSpace(replyTag))
            {
                return;
            }

            if (_applyChoiceImpactOnlyOnce && _appliedChoiceTags.Contains(replyTag))
            {
                Debug.Log($"[DialogueChoiceOutcomeTracker] Choice '{replyTag}' already applied once. Skipping.");
                return;
            }

            if (_choiceDeltaByReplyTag.TryGetValue(replyTag, out int delta))
            {
                CurrentValue += delta;
                _appliedChoiceTags.Add(replyTag);
                ValueChanged?.Invoke(CurrentValue);
                Debug.Log($"[DialogueChoiceOutcomeTracker] Choice applied for tag {replyTag}. Delta={delta:+#;-#;0}, Current={CurrentValue}");
            }
        }

        public string GetEndingTag()
        {
            if (CurrentValue >= _goodEndingThreshold)
            {
                return "ending_good";
            }

            if (CurrentValue >= _neutralEndingThreshold)
            {
                return "ending_neutral";
            }

            return "ending_bad";
        }

        private void EvaluateAndLoadEnding()
        {
            string sceneToLoad;
            if (CurrentValue >= _goodEndingThreshold)
            {
                sceneToLoad = _goodEndingSceneName;
                Debug.Log($"[DialogueChoiceOutcomeTracker] Loading GOOD ending scene: {sceneToLoad}");
            }
            else
            {
                sceneToLoad = _badEndingSceneName;
                Debug.Log($"[DialogueChoiceOutcomeTracker] Loading BAD ending scene: {sceneToLoad}");
            }

            // Небольшая задержка перед загрузкой сцены
            Invoke(nameof(LoadEndingScene), 1f);
        }

        private void LoadEndingScene()
        {
            string sceneToLoad = CurrentValue >= _goodEndingThreshold ? _goodEndingSceneName : _badEndingSceneName;
            SceneTransition.LoadScene(sceneToLoad);
        }
    }
}
