using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform objetivo; 
    
    // Subimos la Y del offset para ver más hacia arriba
    public Vector3 offset = new Vector3(0, 3f, -10f); 
    
    // Qué tan rápido la cámara alcanza al personaje (menor = más rápido)
    public float tiempoSuavizado = 0.15f; 

    // Variable interna que requiere SmoothDamp
    private Vector3 velocidadReferencia = Vector3.zero;

    void LateUpdate()
    {
        if (objetivo != null)
        {
            // 1. Calculamos la posición a la que la cámara DEBERÍA ir
            Vector3 posicionDeseada = objetivo.position + offset;

            // 2. Transición suave desde donde está la cámara ahora, hasta la posición deseada
            transform.position = Vector3.SmoothDamp(transform.position, posicionDeseada, ref velocidadReferencia, tiempoSuavizado);
        }
    }
}