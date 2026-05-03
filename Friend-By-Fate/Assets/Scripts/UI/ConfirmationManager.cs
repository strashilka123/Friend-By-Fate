using UnityEngine;

public class UIConfirmationPopup : MonoBehaviour
{
    [SerializeField] private GameObject confirmationPanel;

    public void OpenPopup()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    // Метод для кнопки "ДА"
    public void OnClickYes()
    {
        // 1. Вызываем удаление прогресса из твоего SaveManager
        SaveManager.Instance.ResetAllProgress();
        
        // 2. Закрываем окно
        ClosePopup();

        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Перезагрузка на главный экран
    }

    // Метод для кнопки "НЕТ"
    public void OnClickNo()
    {
        ClosePopup();
    }

    private void ClosePopup()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
}