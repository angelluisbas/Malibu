using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public GameOverManager gestorGameOver;
    public GameObject gameOverPanel;

    public void MostrarGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void OnBotonReiniciar()
    {
        gestorGameOver.ReiniciarNivel();
    }

    public void OnBotonMenu()
    {
        gestorGameOver.VolverAlMenu();
    }
}