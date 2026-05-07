using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;

    private bool isPaused = false;
    private bool isGameOver = false;

    void Update()
    {
        // Detecta si presionamos Escape o P para pausar
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (!isGameOver) // No permitimos pausar si ya perdimos
            {
                if (isPaused) Resume();
                else Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        // Esto detiene TODO el movimiento físico y animaciones que usen tiempo
        Time.timeScale = 0f; 
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        // Esto devuelve el tiempo a la normalidad
        Time.timeScale = 1f; 
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameOverUI.SetActive(true);
        // Opcional: También detenemos el tiempo al perder
        Time.timeScale = 0f; 
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f; // ¡Muy importante! Resetear el tiempo antes de cargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Resetear tiempo
        SceneManager.LoadScene(0); 
    }
}