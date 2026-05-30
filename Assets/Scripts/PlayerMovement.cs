
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public Animator catAnimator;
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public GatoEnergiaVida energiaVida;

    [Header("Configuración de Movimiento")]
    public float velocidad = 10f;
    public float fuerzaSalto = 7f;
    public float limiteCaida = -3f;

    [Header("Mecánicas de Pared")]
    public float fuerzaSaltoParedY = 6f;
    public float fuerzaEmpujeParedX = 8f;
    public int saltosParedMaximos = 3;

    [Header("Mecánicas de Cable")]
    public float velocidadCable = 15f;

    private Rigidbody rb;
    private GameManager gameManager;

    // Estados
    private bool enElSuelo = false;
    private bool puedeDobleSalto = false;
    private bool tocandoPared = false;
    private bool enCable = false;

    private int saltosParedRestantes;

    private float direccionMuroX;
    private float tiempoBloqueoControl = 0f;

    private Vector3 ejeCable;

    // Posiciones del modelo
    private Vector3 posicionOriginalDerecha;
    private Vector3 posicionIzquierda;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        saltosParedRestantes = saltosParedMaximos;

        gameManager = FindObjectOfType<GameManager>();
        if (energiaVida == null)
            energiaVida = GetComponentInChildren<GatoEnergiaVida>(true);

        if (energiaVida == null)
            Debug.LogWarning(
                "PlayerMovement: arrastra el Canvas (GatoEnergiaVida) al campo Energia Vida en Malibu.",
                this);

        // Guardar posiciones del modelo
        if (catAnimator != null)
        {
            posicionOriginalDerecha = catAnimator.transform.localPosition;

            posicionIzquierda = new Vector3(
                -posicionOriginalDerecha.x,
                posicionOriginalDerecha.y,
                posicionOriginalDerecha.z
            );
        }

        // Configuración segura del audio
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // Sonido 2D
        }
    }

    void Update()
    {
        // Detectar caída del mapa
        if (transform.position.y < limiteCaida)
        {
            Die();
            return;
        }

        float inputHorizontal = Input.GetAxisRaw("Horizontal");

        // =========================
        // ANIMACIONES Y ROTACIÓN
        // =========================
        if (catAnimator != null)
        {
            bool isMoving = inputHorizontal != 0;
            catAnimator.SetBool("isWalking", isMoving);

            if (inputHorizontal > 0)
            {
                catAnimator.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                catAnimator.transform.localPosition = posicionOriginalDerecha;
            }
            else if (inputHorizontal < 0)
            {
                catAnimator.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                catAnimator.transform.localPosition = posicionIzquierda;
            }
        }

        // =========================
        // MOVIMIENTO EN CABLE
        // =========================
        if (enCable)
        {
            MoverEnCable();

            if (Input.GetButtonDown("Jump"))
            {
                SalirDelCable();

                RealizarSalto(fuerzaSalto);

                puedeDobleSalto = true;
            }

            return;
        }

        if (energiaVida != null)
            energiaVida.SetEnSuelo(enElSuelo);

        // =========================
        // BLOQUEO DE CONTROL
        // =========================
        if (tiempoBloqueoControl > 0)
        {
            tiempoBloqueoControl -= Time.deltaTime;
        }
        else
        {
            rb.linearVelocity = new Vector3(
                inputHorizontal * velocidad,
                rb.linearVelocity.y,
                0
            );
        }

        // =========================
        // SISTEMA DE SALTO
        // =========================
        if (Input.GetButtonDown("Jump"))
        {
            // Salto normal
            if (enElSuelo)
            {
                RealizarSalto(fuerzaSalto);

                puedeDobleSalto = true;
            }

            // Wall Jump
            else if (tocandoPared && saltosParedRestantes > 0
                && (energiaVida == null || energiaVida.PuedeSaltarEnPared))
            {
                Vector3 direccionRebote = new Vector3(
                    direccionMuroX * fuerzaEmpujeParedX,
                    fuerzaSaltoParedY,
                    0
                );

                rb.linearVelocity = Vector3.zero;

                rb.AddForce(direccionRebote, ForceMode.Impulse);

                saltosParedRestantes--;

                if (energiaVida != null)
                    energiaVida.RegistrarSaltoEnPared();

                puedeDobleSalto = true;

                tiempoBloqueoControl = 0.25f;

                // Sonido Wall Jump
                ReproducirSonidoSalto();

                // Girar automáticamente
                if (direccionMuroX > 0)
                {
                    catAnimator.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    catAnimator.transform.localPosition = posicionIzquierda;
                }
                else
                {
                    catAnimator.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                    catAnimator.transform.localPosition = posicionOriginalDerecha;
                }
            }

            // Doble salto
            else if (puedeDobleSalto)
            {
                RealizarSalto(fuerzaSalto);

                puedeDobleSalto = false;
            }
        }
    }

    // =========================
    // MOVIMIENTO EN CABLE
    // =========================
    void MoverEnCable()
    {
        float movimientoX = Input.GetAxis("Horizontal");

        Vector3 direccionMovimiento = ejeCable.normalized;

        if (movimientoX < 0 && direccionMovimiento.x > 0)
            direccionMovimiento = -direccionMovimiento;
        else if (movimientoX > 0 && direccionMovimiento.x < 0)
            direccionMovimiento = -direccionMovimiento;

        rb.linearVelocity = direccionMovimiento * (Mathf.Abs(movimientoX) * velocidadCable);

        // Volteo del modelo
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

    // =========================
    // SALTO
    // =========================
    void RealizarSalto(float fuerza)
    {
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0,
            0
        );

        rb.AddForce(Vector3.up * fuerza, ForceMode.Impulse);

        ReproducirSonidoSalto();
    }

    // =========================
    // AUDIO
    // =========================
    void ReproducirSonidoSalto()
    {
        if (audioSource != null && jumpSound != null)
        {
            // Variación leve de tono
            audioSource.pitch = Random.Range(0.95f, 1.05f);

            audioSource.PlayOneShot(jumpSound);
        }
    }

    // =========================
    // SALIR DEL CABLE
    // =========================
    void SalirDelCable()
    {
        enCable = false;

        rb.useGravity = true;
    }

    // =========================
    // MUERTE
    // =========================
    public void Die()
    {
        if (gameManager != null)
            gameManager.ReiniciarNivelAlMorir();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // =========================
    // FIXED UPDATE
    // =========================
    void FixedUpdate()
    {
        enElSuelo = false;
        tocandoPared = false;
    }

    // =========================
    // DETECCIÓN DE COLISIONES
    // =========================
    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            // Suelo
            if (contacto.normal.y > 0.5f)
            {
                enElSuelo = true;

                saltosParedRestantes = saltosParedMaximos;

                puedeDobleSalto = true;
            }

            // Pared
            else if (Mathf.Abs(contacto.normal.x) > 0.5f)
            {
                tocandoPared = true;

                direccionMuroX = contacto.normal.x;
            }
        }
    }

    // =========================
    // ENTRAR A CABLE
    // =========================
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

    // =========================
    // SALIR DEL CABLE
    // =========================
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cable") && enCable)
        {
            SalirDelCable();
        }
    }
}

