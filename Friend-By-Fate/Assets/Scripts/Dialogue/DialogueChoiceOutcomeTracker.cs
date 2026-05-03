using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueChoiceOutcomeTracker : MonoBehaviour
    {
        [SerializeField] private int _startingValue;
        [SerializeField] private int _goodEndingThreshold = 3;
        [SerializeField] private int _neutralEndingThreshold = 0;
        [SerializeField] private ChoiceImpact[] _trackedChoices;
        [SerializeField] private bool _applyChoiceImpactOnlyOnce = true;

        private readonly HashSet<string> _appliedChoiceTags = new();
        private Dictionary<string, int> _choiceDeltaByReplyTag;

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
            SaveManager.ProgressReset += ResetProgress;
            Debug.Log($"[DialogueChoiceOutcomeTracker] Initialized with start value {CurrentValue}");
        }

        private void OnDestroy()
        {
            SaveManager.ProgressReset -= ResetProgress;
            Debug.Log("[DialogueChoiceOutcomeTracker] Unsubscribed from SaveManager.ProgressReset.");
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
    }
}
