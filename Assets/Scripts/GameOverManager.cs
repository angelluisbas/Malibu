using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float limiteCaida = -10f;
    
    private bool juegoTerminado = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (juegoTerminado) return;

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        
        if (jugador != null && jugador.transform.position.y < limiteCaida)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (juegoTerminado) return;
        
        juegoTerminado = true;
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}