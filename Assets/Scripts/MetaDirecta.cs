using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaDirecta : MonoBehaviour
{
    [Header("Configuración")]
    public int indiceSiguienteEscena = 3; // Pon aquí el número de tu EscenaFinal

    private void OnTriggerEnter(Collider other)
    {
        // Si el gato toca la meta, cargamos la escena inmediatamente
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(indiceSiguienteEscena);
        }
    }
}