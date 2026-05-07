using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para recargar niveles

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverUI; // Arrastra tu GameOverPanel aquí desde el Inspector
    
    private bool isGameOver = false;

    public void TriggerGameOver()
    {
        // Evitamos que se ejecute varias veces si el jugador recibe daño continuo
        if (isGameOver) return; 

        isGameOver = true;
        
        // Activamos la pantalla de Game Over
        gameOverUI.SetActive(true);
        
        // Opcional: Pausar el juego para que no sigan ocurriendo cosas de fondo
        // Time.timeScale = 0f; 
    }

    public void RestartLevel()
    {
        // Si pausaste el juego, asegúrate de restaurar el tiempo antes de recargar
        // Time.timeScale = 1f; 
        
        // Recarga la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        // Time.timeScale = 1f;
        // Asumiendo que tu menú principal está en el índice 0 de tus Build Settings
        SceneManager.LoadScene(0); 
    }
}