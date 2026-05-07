using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScreen : MonoBehaviour
{
    public void VolverAlMenu()
    {
        // Cargamos la escena 0 (Menú Principal)
        SceneManager.LoadScene(0);
    }
}