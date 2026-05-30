using UnityEngine;
using UnityEngine.UI;

public class GatoEnergiaVida : MonoBehaviour
{
    const float TiempoRegenMinimoSegundos = 30f;

    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual = 100f;
    public float danoPorCansancio = 20f;

    [Header("Configuración de Saltos en Pared")]
    public int saltosMaximosPared = 3;
    private int saltosRealizados;

    [Header("Regeneración")]
    [Tooltip("Segundos entre cada bloque de +20 (30 s por defecto).")]
    public float tiempoEsperaRegen = 30f;
    public float vidaRegeneradaPorCiclo = 20f;
    [Tooltip("Solo para pruebas: permite tiempos cortos en el Inspector.")]
    public bool usarTiemposDePrueba = false;

    [Header("Componentes de UI")]
    public Slider sliderUI;
    public Image imagenFill;

    [Header("Canvas sobre el gato")]
    public bool barraFijaEnElGato = true;
    public bool mirarCamara = true;
    public Camera camaraUI;

    [Header("Ajuste visual World Space")]
    [Tooltip("Usa la misma barra que en la escena Juego (Nivel 1).")]
    public bool usarPresetNivel1 = true;
    [Tooltip("Si está desmarcado, respeta la posición/escala que pongas en el Inspector.")]
    public bool aplicarAjusteVisualAlIniciar = false;
    public Vector3 posicionLocalCanvas = new Vector3(0f, 0f, 7.12f);
    public Vector3 rotacionLocalCanvas = Vector3.zero;
    [Tooltip("Suma esto tras orientar la barra hacia la cámara (ej. Y=180 si se ve al revés).")]
    public Vector3 rotacionExtraMirarCamara = Vector3.zero;
    public Vector3 escalaLocalCanvas = new Vector3(0.5f, 0.1f, 0.1f);
    [Tooltip("Solo si la barra queda gigante o invisible por la escala del modelo.")]
    public bool compensarEscalaDelPadre = false;
    public Vector2 tamanoSlider = new Vector2(2f, 0.5f);
    public Vector2 anclajeCanvas = new Vector2(0.14f, 1.26f);
    public int ordenRenderizado = 0;
    [Tooltip("0 = no mueve la barra cada frame (recomendado).")]
    public float distanciaHaciaCamara = 0f;

    Canvas canvasEnEsteObjeto;
    RectTransform rectFill;
    float tiempoRegenDisponible = -1f;
    bool sistemaInicializado;

    public bool PuedeSaltarEnPared => vidaActual > 0f;

    void Awake()
    {
        canvasEnEsteObjeto = GetComponent<Canvas>();
        AplicarTiemposSeguros();
        ResolverReferenciasUI();
    }

    void Start()
    {
        if (!sistemaInicializado)
        {
            vidaActual = vidaMaxima;
            saltosRealizados = 0;
            tiempoRegenDisponible = -1f;
            sistemaInicializado = true;
        }

        AplicarTiemposSeguros();
        ResolverReferenciasUI();
        ConfigurarSlider();
        if (usarPresetNivel1)
            AplicarPresetNivel1();
        else if (aplicarAjusteVisualAlIniciar)
            AplicarAjusteVisualWorldSpace();
        else
            AsegurarCanvasWorldSpace();
        ActualizarBarraVisual();

        if (sliderUI == null || rectFill == null)
            Debug.LogError("GatoEnergiaVida: asigna Slider UI en el Canvas.", this);

        if (transform.parent != null && transform.parent.name.ToLower().Contains("rig"))
        {
            Debug.LogWarning(
                "GatoEnergiaVida: para Nivel 2, mueve el Canvas a Malibufinal100 (fuera de 'rig') si la barra se ve rara.",
                this);
        }
    }

    void Update()
    {
        ActualizarRegeneracion();
    }

    void LateUpdate()
    {
        if (!barraFijaEnElGato || canvasEnEsteObjeto == null) return;
        if (canvasEnEsteObjeto.renderMode != RenderMode.WorldSpace) return;

        var cam = camaraUI != null ? camaraUI : Camera.main;
        if (cam == null) return;

        if (mirarCamara)
        {
            transform.rotation = cam.transform.rotation;
            if (rotacionExtraMirarCamara != Vector3.zero)
                transform.rotation *= Quaternion.Euler(rotacionExtraMirarCamara);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(rotacionLocalCanvas);
        }

        if (distanciaHaciaCamara > 0f && aplicarAjusteVisualAlIniciar)
        {
            var baseLocal = posicionLocalCanvas;
            var empuje = transform.InverseTransformDirection(-cam.transform.forward) * distanciaHaciaCamara;
            transform.localPosition = baseLocal + empuje;
        }
    }

    void AplicarTiemposSeguros()
    {
        if (!usarTiemposDePrueba)
            tiempoEsperaRegen = TiempoRegenMinimoSegundos;

        if (vidaRegeneradaPorCiclo <= 0f)
            vidaRegeneradaPorCiclo = danoPorCansancio;
    }

    public void SetEnSuelo(bool valor)
    {
        if (valor)
            saltosRealizados = 0;
    }

    public bool RegistrarSaltoEnPared()
    {
        if (vidaActual <= 0f || saltosRealizados >= saltosMaximosPared)
            return false;

        saltosRealizados++;

        if (saltosRealizados >= saltosMaximosPared)
            AplicarDanoPorCansancio();

        return true;
    }

    void ActualizarRegeneracion()
    {
        if (vidaActual >= vidaMaxima)
        {
            tiempoRegenDisponible = -1f;
            return;
        }

        if (tiempoRegenDisponible < 0f) return;
        if (Time.time < tiempoRegenDisponible) return;

        vidaActual = Mathf.Min(vidaActual + vidaRegeneradaPorCiclo, vidaMaxima);
        ActualizarBarraVisual();

        Debug.Log($"+{vidaRegeneradaPorCiclo:0} vida. Total: {vidaActual:0}/{vidaMaxima:0}");

        if (vidaActual >= vidaMaxima)
        {
            vidaActual = vidaMaxima;
            tiempoRegenDisponible = -1f;
            ActualizarBarraVisual();
            return;
        }

        tiempoRegenDisponible = Time.time + tiempoEsperaRegen;
    }

    void AplicarDanoPorCansancio()
    {
        vidaActual -= danoPorCansancio;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        saltosRealizados = 0;

        AplicarTiemposSeguros();

        if (tiempoRegenDisponible < 0f)
            tiempoRegenDisponible = Time.time + tiempoEsperaRegen;

        ActualizarBarraVisual();

        float segundosRestantes = Mathf.Max(0f, tiempoRegenDisponible - Time.time);
        Debug.Log(
            $"¡Cansancio! -{danoPorCansancio:0} vida. Total: {vidaActual:0}/{vidaMaxima:0}. " +
            $"Próximos +{vidaRegeneradaPorCiclo:0} en {segundosRestantes:0}s (espera {tiempoEsperaRegen:0}s).");

        if (vidaActual <= 0f)
            MorirPorCansancio();
    }

    void MorirPorCansancio()
    {
        Debug.Log("El gato murió por cansancio.");
        var movimiento = GetComponentInParent<PlayerMovement>();
        if (movimiento != null)
            movimiento.Die();
    }

    void ActualizarBarraVisual()
    {
        float porcentaje = vidaMaxima > 0f ? Mathf.Clamp01(vidaActual / vidaMaxima) : 0f;
        Color colorBarra = ObtenerColorPorVida();

        if (sliderUI != null)
        {
            sliderUI.minValue = 0f;
            sliderUI.maxValue = vidaMaxima;
            sliderUI.SetValueWithoutNotify(vidaActual);
        }

        if (rectFill != null)
        {
            rectFill.anchorMin = Vector2.zero;
            rectFill.anchorMax = new Vector2(porcentaje, 1f);
            rectFill.offsetMin = Vector2.zero;
            rectFill.offsetMax = Vector2.zero;
            rectFill.localScale = Vector3.one;
        }

        if (imagenFill != null)
        {
            imagenFill.color = colorBarra;
            imagenFill.type = Image.Type.Simple;
        }

        Canvas.ForceUpdateCanvases();
    }

    Color ObtenerColorPorVida()
    {
        if (vidaActual > 80f) return Color.green;
        if (vidaActual > 40f) return Color.yellow;
        return Color.red;
    }

    void ResolverReferenciasUI()
    {
        if (sliderUI == null)
            sliderUI = GetComponentInChildren<Slider>(true);

        if (sliderUI == null) return;

        if (sliderUI.fillRect == null)
        {
            var fill = sliderUI.transform.Find("Fill Area/Fill");
            if (fill != null)
                sliderUI.fillRect = fill as RectTransform;
        }

        rectFill = sliderUI.fillRect;

        if (rectFill != null)
            imagenFill = rectFill.GetComponent<Image>();
    }

    void ConfigurarSlider()
    {
        if (sliderUI == null) return;

        sliderUI.interactable = false;
        sliderUI.transition = Selectable.Transition.None;
        sliderUI.minValue = 0f;
        sliderUI.maxValue = vidaMaxima;
        sliderUI.value = vidaActual;
        sliderUI.wholeNumbers = false;
        sliderUI.direction = Slider.Direction.LeftToRight;

        var fillArea = sliderUI.transform.Find("Fill Area") as RectTransform;
        if (fillArea != null)
        {
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = Vector2.zero;
            fillArea.offsetMax = Vector2.zero;
        }

        sliderUI.enabled = false;

        var rectSlider = sliderUI.GetComponent<RectTransform>();
        if (rectSlider != null)
            rectSlider.sizeDelta = tamanoSlider;
    }

    [ContextMenu("Aplicar preset visual Nivel 1")]
    public void AplicarPresetNivel1()
    {
        usarPresetNivel1 = true;
        aplicarAjusteVisualAlIniciar = false;
        compensarEscalaDelPadre = false;
        distanciaHaciaCamara = 0f;
        ordenRenderizado = 0;

        posicionLocalCanvas = new Vector3(0f, 0f, 7.12f);
        escalaLocalCanvas = new Vector3(0.5f, 0.1f, 0.1f);
        tamanoSlider = new Vector2(2f, 0.5f);
        anclajeCanvas = new Vector2(0.14f, 1.26f);
        rotacionLocalCanvas = new Vector3(0f, -88.37f, 0f);

        var rect = transform as RectTransform;
        if (rect != null)
        {
            rect.localRotation = Quaternion.Euler(rotacionLocalCanvas);
            rect.localPosition = posicionLocalCanvas;
            rect.localScale = escalaLocalCanvas;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anclajeCanvas;
            rect.sizeDelta = new Vector2(2f, 0.5f);
        }

        AsegurarCanvasWorldSpace();
        ConfigurarSlider();
        ActualizarBarraVisual();
    }

    void AsegurarCanvasWorldSpace()
    {
        if (canvasEnEsteObjeto == null) return;

        canvasEnEsteObjeto.renderMode = RenderMode.WorldSpace;
        canvasEnEsteObjeto.overrideSorting = ordenRenderizado != 0;
        canvasEnEsteObjeto.sortingOrder = ordenRenderizado;

        var cam = camaraUI != null ? camaraUI : Camera.main;
        if (cam != null)
            canvasEnEsteObjeto.worldCamera = cam;
    }

    void AplicarAjusteVisualWorldSpace()
    {
        AsegurarCanvasWorldSpace();
        if (!barraFijaEnElGato) return;

        transform.localPosition = posicionLocalCanvas;
        AplicarEscalaCanvas();
        if (!mirarCamara)
            transform.localRotation = Quaternion.Euler(rotacionLocalCanvas);
    }

    void AplicarEscalaCanvas()
    {
        if (!compensarEscalaDelPadre || transform.parent == null)
        {
            transform.localScale = escalaLocalCanvas;
            return;
        }

        const float minimo = 0.0001f;
        var escalaPadre = transform.parent.lossyScale;
        transform.localScale = new Vector3(
            escalaLocalCanvas.x / Mathf.Max(Mathf.Abs(escalaPadre.x), minimo),
            escalaLocalCanvas.y / Mathf.Max(Mathf.Abs(escalaPadre.y), minimo),
            escalaLocalCanvas.z / Mathf.Max(Mathf.Abs(escalaPadre.z), minimo));
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (vidaMaxima < 1f) vidaMaxima = 1f;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        AplicarTiemposSeguros();
    }
#endif
}
