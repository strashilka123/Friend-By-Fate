using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    private static SceneTransition instance;

    [SerializeField] private float fadeDuration = 0.6f;

    private CanvasGroup fadeCanvasGroup;
    private bool isTransitioning;

    public static void LoadScene(int sceneBuildIndex)
    {
        Instance.StartSceneTransition(sceneBuildIndex);
    }

    public static void LoadScene(string sceneName)
    {
        Instance.StartSceneTransition(sceneName);
    }

    public static void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void LoadNextScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private static SceneTransition Instance
    {
        get
        {
            if (instance == null)
            {
                CreateInstance();
            }

            return instance;
        }
    }

    private static void CreateInstance()
    {
        GameObject transitionObject = new GameObject("SceneTransition");
        instance = transitionObject.AddComponent<SceneTransition>();
        DontDestroyOnLoad(transitionObject);
        instance.CreateFadeCanvas();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadeCanvas();
    }

    private void CreateFadeCanvas()
    {
        if (fadeCanvasGroup != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("SceneTransitionCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        canvasObject.AddComponent<GraphicRaycaster>();
        fadeCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        GameObject fadeImageObject = new GameObject("BlackFade");
        fadeImageObject.transform.SetParent(canvasObject.transform, false);

        Image fadeImage = fadeImageObject.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform fadeRect = fadeImage.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
    }

    private void StartSceneTransition(int sceneBuildIndex)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionToScene(sceneBuildIndex));
    }

    private void StartSceneTransition(string sceneName)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(int sceneBuildIndex)
    {
        yield return FadeOutBeforeLoad();
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneBuildIndex);
        yield return WaitForSceneLoad(loadOperation);
        yield return FadeInAfterLoad();
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        yield return FadeOutBeforeLoad();
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return WaitForSceneLoad(loadOperation);
        yield return FadeInAfterLoad();
    }

    private IEnumerator FadeOutBeforeLoad()
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;
        yield return Fade(1f);
    }

    private IEnumerator WaitForSceneLoad(AsyncOperation loadOperation)
    {
        if (loadOperation == null)
        {
            yield break;
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeInAfterLoad()
    {
        yield return Fade(0f);
        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
