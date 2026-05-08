using TMPro;
using UnityEngine;
using System.Collections;

namespace Dialogue
{
    /// <summary>
    /// Компонент для побуквенного вывода текста в диалогах.
    /// Анимация начинается со второго слова после имени персонажа и двоеточия.
    /// Если двоеточия нет - текст выводится полностью с первого слова.
    /// </summary>
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("Настройки эффекта")]
        [Tooltip("Скорость вывода символов (секунды на символ)")]
        [Range(0.01f, 0.5f)]
        public float characterDelay = 0.03f;

        private TMP_Text _textComponent;
        private Coroutine _typewriterCoroutine;
        private string _fullText;
        private bool _isComplete = false;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnDestroy()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }
        }

        /// <summary>
        /// Запускает побуквенный вывод текста.
        /// </summary>
        /// <param name="text">Полный текст для отображения</param>
        public void DisplayText(string text)
        {
            if (_textComponent == null)
            {
                _textComponent = GetComponent<TMP_Text>();
            }

            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }

            _fullText = text;
            _isComplete = false;

            // Определяем точку начала анимации
            int startIndex = GetAnimationStartIndex(text);

            // Сразу отображаем часть до точки начала (например, "Имя: ")
            if (startIndex > 0)
            {
                _textComponent.text = text.Substring(0, startIndex);
            }
            else
            {
                _textComponent.text = "";
            }

            // Запускаем корутину для побуквенного вывода остальной части
            _typewriterCoroutine = StartCoroutine(TypewriterCoroutine(startIndex));
        }

        /// <summary>
        /// Мгновенно показывает весь текст.
        /// </summary>
        public void ShowAllText()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
            }

            if (!string.IsNullOrEmpty(_fullText))
            {
                _textComponent.text = _fullText;
            }

            _isComplete = true;
        }

        /// <summary>
        /// Проверяет, завершен ли вывод текста.
        /// </summary>
        public bool IsComplete => _isComplete;

        /// <summary>
        /// Определяет индекс, с которого нужно начинать побуквенную анимацию.
        /// Ищет первое двоеточие и начинает анимацию со второго слова после него.
        /// Если двоеточия нет - возвращает 0 (анимация с начала).
        /// </summary>
        private int GetAnimationStartIndex(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int colonIndex = text.IndexOf(':');

            // Если двоеточия нет - начинаем с первого символа
            if (colonIndex < 0)
            {
                return 0;
            }

            // Пропускаем двоеточие и возможные пробелы после него
            int afterColon = colonIndex + 1;
            while (afterColon < text.Length && char.IsWhiteSpace(text[afterColon]))
            {
                afterColon++;
            }

            // Если после двоеточия ничего нет - начинаем сразу после двоеточия
            if (afterColon >= text.Length)
            {
                return afterColon;
            }

            // Находим начало второго слова после двоеточия
            // Первое слово - это то, что сразу после двоеточия
            int wordCount = 0;
            int i = afterColon;

            while (i < text.Length)
            {
                // Пропускаем пробелы
                while (i < text.Length && char.IsWhiteSpace(text[i]))
                {
                    i++;
                }

                if (i >= text.Length)
                {
                    break;
                }

                // Начинаем слово
                wordCount++;
                int wordStart = i;

                // Пропускаем слово
                while (i < text.Length && !char.IsWhiteSpace(text[i]))
                {
                    i++;
                }

                // Если это второе слово - возвращаем его начало
                if (wordCount == 2)
                {
                    return wordStart;
                }
            }

            // Если не нашли второе слово - начинаем анимацию сразу после двоеточия и пробелов
            return afterColon;
        }

        private IEnumerator TypewriterCoroutine(int startIndex)
        {
            for (int i = startIndex; i < _fullText.Length; i++)
            {
                _textComponent.text = _fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(characterDelay);
            }

            _isComplete = true;
        }
    }
}