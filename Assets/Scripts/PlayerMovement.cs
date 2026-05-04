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

    [Header("Mecánicas de Cable")]
    public float velocidadCable = 15f; 

    private Rigidbody rb;
    
    // Variables de estado
    private bool enElSuelo = false;
    private bool puedeDobleSalto = false;
    private bool tocandoPared = false;
    private bool enCable = false; // NUEVO: Estado del cable
    private int saltosParedRestantes;
    private float direccionMuroX; 
    private Vector3 ejeCable; // Para conocer la inclinación del cable

    // Temporizador para evitar que el Input cancele la fuerza del Wall Jump
    private float tiempoBloqueoControl = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        saltosParedRestantes = saltosParedMaximos;
    }

    void Update()
    {
        // 1. Lógica del Cable (Tiene prioridad absoluta si estamos sobre uno)
        if (enCable)
        {
            MoverEnCable();

            // Permitir saltar para soltarse del cable
            if (Input.GetButtonDown("Jump"))
            {
                SalirDelCable();
                RealizarSalto(fuerzaSalto);
                puedeDobleSalto = true; // Damos la opción de doble salto en el aire
            }
            return; // Detenemos la ejecución del Update aquí para ignorar el resto del movimiento
        }

        // 2. Reducimos el temporizador si estamos bloqueados
        if (tiempoBloqueoControl > 0)
        {
            tiempoBloqueoControl -= Time.deltaTime;
        }
        else
        {
            // 3. Solo aplicamos el movimiento manual normal si NO estamos bloqueados
            float movimientoX = Input.GetAxis("Horizontal");
            rb.linearVelocity = new Vector3(movimientoX * velocidad, rb.linearVelocity.y, 0);
        }

        // 4. Lógica de Saltos (Suelo, Pared, Doble Salto)
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
                
                // Bloqueamos el control direccional por 0.25 segundos
                tiempoBloqueoControl = 0.25f; 
            }
            else if (puedeDobleSalto)
            {
                RealizarSalto(fuerzaSalto);
                puedeDobleSalto = false;
            }
        }
    }

    // NUEVO: Movimiento adaptado al eje del cable
    void MoverEnCable()
    {
        float movimientoX = Input.GetAxis("Horizontal");
        Vector3 direccionMovimiento = ejeCable.normalized;

        // Aseguramos que presionar izquierda/derecha mueva al personaje correctamente por el cable
        if (movimientoX < 0 && direccionMovimiento.x > 0) direccionMovimiento = -direccionMovimiento;
        else if (movimientoX > 0 && direccionMovimiento.x < 0) direccionMovimiento = -direccionMovimiento;

        // Aplicamos la velocidad sin usar gravedad
        rb.linearVelocity = direccionMovimiento * (Mathf.Abs(movimientoX) * velocidadCable);
    }

    void RealizarSalto(float fuerza)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
        rb.AddForce(Vector3.up * fuerza, ForceMode.Impulse);
    }

    // NUEVO: Limpia el estado cuando el gato abandona el cable
    void SalirDelCable()
    {
        enCable = false;
        rb.useGravity = true; // Devolvemos la gravedad a la normalidad
    }

    void FixedUpdate()
    {
        enElSuelo = false;
        tocandoPared = false;
    }

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

    // NUEVO: Detectar si el gato agarra un cable
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cable") && !enCable)
        {
            enCable = true;
            rb.useGravity = false; // El gato deja de caer
            rb.linearVelocity = Vector3.zero; // Frenamos la inercia

            // Usamos el eje X local del objeto como su dirección (Ideal si escalas un cubo en X)
            ejeCable = other.transform.right;
        }
    }

    // NUEVO: Detectar si el gato llega al final del cable y se cae
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cable") && enCable)
        {
            SalirDelCable();
        }
    }
}