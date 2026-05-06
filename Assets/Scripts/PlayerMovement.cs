using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator catAnimator;
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
    private bool enCable = false;
    private int saltosParedRestantes;
    private float direccionMuroX; 
    private Vector3 ejeCable;

    // Temporizador para evitar que el Input cancele la fuerza del Wall Jump
    private float tiempoBloqueoControl = 0f;
    
    // Variables para guardar la posición original del modelo
    private Vector3 posicionOriginalDerecha;
    private Vector3 posicionIzquierda;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        saltosParedRestantes = saltosParedMaximos;
        
        // Guardamos la posición original (mirando a la derecha)
        if (catAnimator != null)
        {
            posicionOriginalDerecha = catAnimator.transform.localPosition;
            
            // Calculamos la posición para cuando mira a la izquierda
            // Invertimos el desplazamiento en X para compensar la rotación
            posicionIzquierda = new Vector3(-posicionOriginalDerecha.x, 
                                            posicionOriginalDerecha.y, 
                                            posicionOriginalDerecha.z);
        }
    }

    void Update()
    {
        // Usamos GetAxisRaw para que el valor sea EXACTAMENTE 0 al instante de soltar la tecla
        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        // --- LÓGICA DE ANIMACIÓN Y ROTACIÓN DEL GATO ---
        if (catAnimator != null)
        {
            // Como usamos GetAxisRaw, podemos ser exactos. Si es distinto de 0, camina.
            bool isMoving = inputHorizontal != 0;
            catAnimator.SetBool("isWalking", isMoving);

            // VOLTEAR EL MODELO 3D Y AJUSTAR POSICIÓN
            if (inputHorizontal > 0)
            {
                // Derecha: Giramos 90 grados y usamos posición original
                catAnimator.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                catAnimator.transform.localPosition = posicionOriginalDerecha;
            }
            else if (inputHorizontal < 0)
            {
                // Izquierda: Giramos -90 grados y usamos posición compensada
                catAnimator.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                catAnimator.transform.localPosition = posicionIzquierda;
            }
        }
        // -----------------------------------------------

        // 1. Lógica del Cable (Tiene prioridad absoluta si estamos sobre uno)
        if (enCable)
        {
            MoverEnCable();

            // Permitir saltar para soltarse del cable
            if (Input.GetButtonDown("Jump"))
            {
                SalirDelCable();
                RealizarSalto(fuerzaSalto);
                puedeDobleSalto = true;
            }
            return;
        }

        // 2. Reducimos el temporizador si estamos bloqueados
        if (tiempoBloqueoControl > 0)
        {
            tiempoBloqueoControl -= Time.deltaTime;
        }
        else
        {
            // 3. Solo aplicamos el movimiento manual normal si NO estamos bloqueados
            rb.linearVelocity = new Vector3(inputHorizontal * velocidad, rb.linearVelocity.y, 0);
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
                
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(direccionRebote, ForceMode.Impulse);
                
                saltosParedRestantes--;
                puedeDobleSalto = true; 
                
                // Bloqueamos el control direccional por 0.25 segundos
                tiempoBloqueoControl = 0.25f;
                
                // Voltear automáticamente al rebotar en pared
                if (direccionMuroX > 0)
                {
                    // Pared derecha → mirar izquierda
                    catAnimator.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    catAnimator.transform.localPosition = posicionIzquierda;
                }
                else
                {
                    // Pared izquierda → mirar derecha
                    catAnimator.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    catAnimator.transform.localPosition = posicionOriginalDerecha;
                }
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
        
        // Volteo en el cable
        if (catAnimator != null)
        {
            if (movimientoX > 0)
            {
                catAnimator.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                catAnimator.transform.localPosition = posicionOriginalDerecha;
            }
            else if (movimientoX < 0)
            {
                catAnimator.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                catAnimator.transform.localPosition = posicionIzquierda;
            }
        }
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
        rb.useGravity = true;
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
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
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