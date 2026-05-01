using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 10f; 
    public float fuerzaSalto = 7f; 
    
    [Header("Mecánicas de Pared")]
    public float fuerzaSaltoParedY = 6f; 
    public float fuerzaEmpujeParedX = 8f; 
    public int saltosParedMaximos = 3; 

    private Rigidbody rb;
    
    // Variables de estado
    private bool enElSuelo = false;
    private bool puedeDobleSalto = false;
    private bool tocandoPared = false;
    private int saltosParedRestantes;
    private float direccionMuroX; 

    // Temporizador para evitar que el Input cancele la fuerza del Wall Jump
    private float tiempoBloqueoControl = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        saltosParedRestantes = saltosParedMaximos;
    }

    void Update()
    {
        // 1. Reducimos el temporizador si estamos bloqueados
        if (tiempoBloqueoControl > 0)
        {
            tiempoBloqueoControl -= Time.deltaTime;
        }
        else
        {
            // 2. Solo aplicamos el movimiento manual si NO estamos bloqueados por un rebote
            float movimientoX = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector3(movimientoX * velocidad, rb.linearVelocity.y, 0);
        }

        // 3. Lógica de Saltos
        if (Input.GetButtonDown("Jump")) 
        {
            if (enElSuelo)
            {
                RealizarSalto(fuerzaSalto);
                puedeDobleSalto = true;
            }
            else if (tocandoPared && saltosParedRestantes > 0)
            {
                // SALTO ENTRE PAREDES (Wall Jump)
                Vector3 direccionRebote = new Vector3(direccionMuroX * fuerzaEmpujeParedX, fuerzaSaltoParedY, 0);
                
                rb.linearVelocity = Vector3.zero; // Limpiamos inercia
                rb.AddForce(direccionRebote, ForceMode.Impulse);
                
                saltosParedRestantes--;
                puedeDobleSalto = true; 
                
                // Bloqueamos el control direccional por 0.25 segundos para que el gato sea empujado
                tiempoBloqueoControl = 0.25f; 
            }
            else if (puedeDobleSalto)
            {
                RealizarSalto(fuerzaSalto);
                puedeDobleSalto = false;
            }
        }
    }

    void RealizarSalto(float fuerza)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
        rb.AddForce(Vector3.up * fuerza, ForceMode.Impulse);
    }

    // Para evitar falsos negativos, reiniciamos los estados al inicio del cálculo de físicas
    void FixedUpdate()
    {
        enElSuelo = false;
        tocandoPared = false;
    }

    // Y dejamos que los contactos reales nos confirmen el estado
    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.5f)
            {
                enElSuelo = true;
                saltosParedRestantes = saltosParedMaximos;
                puedeDobleSalto = true;
            }
            else if (Mathf.Abs(contacto.normal.x) > 0.5f)
            {
                tocandoPared = true;
                direccionMuroX = contacto.normal.x; 
            }
        }
    }
}