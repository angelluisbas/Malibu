using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;

    bool isPaused;
    bool isGameOver;

    void Awake()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
        if (gameOverUI != null)
            gameOverUI.SetActive(false);

        AsegurarCanvasPantallaCompleta();
    }

    void Update()
    {
        if (!isGameOver && SolicitarPausa())
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    static bool SolicitarPausa()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                return true;
        }
#endif
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
    }

    void AsegurarCanvasPantallaCompleta()
    {
        if (pauseMenuUI == null) return;

        var canvas = pauseMenuUI.GetComponentInParent<Canvas>(true);
        if (canvas == null) return;

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var rect = canvas.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    public void Pause()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogWarning("GameManager: asigna Menu Pausa en el Inspector.", this);
            return;
        }

        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        if (pauseMenuUI == null) return;

        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartLevel()
    {
        ReiniciarNivel();
    }

    public void ReiniciarNivelAlMorir()
    {
        ReiniciarNivel();
    }

    void ReiniciarNivel()
    {
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;

        const string menu = "MainMenu";
        if (Application.CanStreamedLevelBeLoaded(menu))
            SceneManager.LoadScene(menu);
        else
            SceneManager.LoadScene(0);
    }
}
