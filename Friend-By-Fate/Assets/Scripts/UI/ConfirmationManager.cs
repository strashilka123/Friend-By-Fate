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

        SceneTransition.LoadScene(0);
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