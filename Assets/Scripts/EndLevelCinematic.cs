using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndLevelCinematic : MonoBehaviour
{
    [Header("Configuración")]
    public VideoPlayer videoPlayer;
    public int indiceNivel2 = 2;
    public string nombreEscenaSiguiente = "Nivel2";

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.loopPointReached += CargarSiguienteNivel;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            videoPlayer.Play();
    }

    void CargarSiguienteNivel(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= CargarSiguienteNivel;

        if (!string.IsNullOrEmpty(nombreEscenaSiguiente)
            && Application.CanStreamedLevelBeLoaded(nombreEscenaSiguiente))
        {
            SceneManager.LoadScene(nombreEscenaSiguiente);
            return;
        }

        if (indiceNivel2 >= 0 && indiceNivel2 < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(indiceNivel2);
        else
            SceneManager.LoadScene("MainMenu");
    }
}
