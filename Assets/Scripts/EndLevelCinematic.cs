using UnityEngine;
using UnityEngine.Video; // Necesario para usar el VideoPlayer
using UnityEngine.SceneManagement; 

public class EndLevelCinematic : MonoBehaviour
{
    [Header("Configuración")]
    public VideoPlayer videoPlayer;
    public int indiceNivel2 = 2; // El índice de tu Nivel 2 en los Build Profiles

    void Start()
    {
        // Si no asignaste el VideoPlayer en el inspector, lo busca automáticamente
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Esta es la parte mágica: nos suscribimos al evento del fin del video
        // Le dice a Unity: "Cuando el video termine, ejecuta el método CargarSiguienteNivel"
        videoPlayer.loopPointReached += CargarSiguienteNivel;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entró en la zona de meta es el protagonista
        if (other.CompareTag("Player"))
        {
            // Opcional: Desactivar el movimiento del gato o la UI para que no interfieran
            // other.gameObject.SetActive(false); 
            
            // Reproduce el video
            videoPlayer.Play();
        }
    }

    // Este método se ejecutará automáticamente gracias a loopPointReached
    void CargarSiguienteNivel(VideoPlayer vp)
    {
        // Desvinculamos el evento por buenas prácticas de memoria
        videoPlayer.loopPointReached -= CargarSiguienteNivel; 
        
        // Cargamos la siguiente escena
        SceneManager.LoadScene(indiceNivel2);
    }
}