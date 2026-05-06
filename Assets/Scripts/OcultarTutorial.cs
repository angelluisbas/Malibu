using UnityEngine;

public class OcultarTutorial : MonoBehaviour
{
    [Header("Tiempo en pantalla")]
    public float segundosVisible = 5f; // Puedes cambiar este número en el Inspector

    void Start()
    {
        // Invoke llama a la función "ApagarCanvas" después del tiempo establecido
        Invoke("ApagarCanvas", segundosVisible);
    }

    void ApagarCanvas()
    {
        // Apaga el Canvas
        gameObject.SetActive(false); 
    }
}