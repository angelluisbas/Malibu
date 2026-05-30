using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BienvenidaDemo : MonoBehaviour
{
    const string RutaImagenDemo = "Assets/Imagenes y audios/CartelDemo.png";

    [Header("Imagen (CartelDemo.png — arrastra desde Imagenes y audios)")]
    [Tooltip("Arrastra CartelDemo.png aquí. No uses Materials/Demo (ese es un material 3D).")]
    public Sprite imagenDemo;
    public Vector2 tamanoImagen = new Vector2(520f, 300f);
    public Vector2 posicionImagen = new Vector2(-280f, -20f);
    public bool imagenEnLadoDerecho = true;

    [Header("Textos (si no hay imagen, o debajo de ella)")]
    public string titulo = "DEMO";
    public string leyenda = "Explora el mapa y llega a tu destino";
    public bool mostrarTextoConImagen = false;

    [Header("Tiempo en pantalla")]
    public float segundosVisible = 8f;

    [Header("Colores")]
    public Color colorFondoPanel = new Color(0f, 0f, 0f, 0.72f);
    public Color colorTitulo = new Color(1f, 0.92f, 0.2f, 1f);
    public Color colorLeyenda = Color.white;

    GameObject canvasRaiz;

    void Awake()
    {
        ResolverImagenDemo();
    }

    void ResolverImagenDemo()
    {
        if (imagenDemo != null) return;

#if UNITY_EDITOR
        imagenDemo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaImagenDemo);
#endif
        if (imagenDemo == null)
        {
            Debug.LogWarning(
                "BienvenidaDemo: arrastra CartelDemo.png en Imagen Demo " +
                $"(ruta: {RutaImagenDemo}).",
                this);
        }
    }

    void Start()
    {
        CrearInterfaz();
        if (segundosVisible > 0f)
            Invoke(nameof(Ocultar), segundosVisible);
    }

    void CrearInterfaz()
    {
        var fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        canvasRaiz = new GameObject("CanvasBienvenidaDemo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRaiz.transform.SetParent(transform, false);

        var canvas = canvasRaiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        var scaler = canvasRaiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (imagenDemo != null)
        {
            CrearImagenDemo(canvasRaiz.transform, imagenDemo);
            if (mostrarTextoConImagen)
                CrearPanelTexto(canvasRaiz.transform, fuente);
        }
        else
        {
            CrearPanelTexto(canvasRaiz.transform, fuente);
        }
    }

    void CrearImagenDemo(Transform padre, Sprite sprite)
    {
        var go = new GameObject("ImagenDemo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);

        var imagen = go.GetComponent<Image>();
        imagen.sprite = sprite;
        imagen.preserveAspect = true;
        imagen.raycastTarget = false;

        var rect = go.GetComponent<RectTransform>();
        if (imagenEnLadoDerecho)
        {
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
        }
        else
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
        }
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicionImagen;
        rect.sizeDelta = tamanoImagen;
    }

    void CrearPanelTexto(Transform padre, Font fuente)
    {
        var panel = CrearPanel(padre, colorFondoPanel);
        var rectPanel = panel.GetComponent<RectTransform>();
        rectPanel.anchorMin = new Vector2(0.5f, 1f);
        rectPanel.anchorMax = new Vector2(0.5f, 1f);
        rectPanel.pivot = new Vector2(0.5f, 1f);
        rectPanel.anchoredPosition = new Vector2(0f, -40f);
        rectPanel.sizeDelta = new Vector2(920f, 200f);

        var tituloGo = CrearTexto(panel.transform, titulo, 72, TextAnchor.MiddleCenter, colorTitulo, fuente);
        var rectTitulo = tituloGo.GetComponent<RectTransform>();
        rectTitulo.anchorMin = new Vector2(0f, 0.55f);
        rectTitulo.anchorMax = new Vector2(1f, 1f);
        rectTitulo.offsetMin = Vector2.zero;
        rectTitulo.offsetMax = Vector2.zero;

        var leyendaGo = CrearTexto(panel.transform, leyenda, 30, TextAnchor.MiddleCenter, colorLeyenda, fuente);
        var rectLeyenda = leyendaGo.GetComponent<RectTransform>();
        rectLeyenda.anchorMin = new Vector2(0f, 0f);
        rectLeyenda.anchorMax = new Vector2(1f, 0.5f);
        rectLeyenda.offsetMin = new Vector2(24f, 8f);
        rectLeyenda.offsetMax = new Vector2(-24f, -8f);
    }

    static GameObject CrearPanel(Transform padre, Color color)
    {
        var go = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        var imagen = go.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return go;
    }

    static GameObject CrearTexto(Transform padre, string contenido, int tamano, TextAnchor ancla, Color color, Font fuente)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(padre, false);

        var texto = go.GetComponent<Text>();
        texto.text = contenido;
        texto.font = fuente;
        texto.fontSize = tamano;
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = ancla;
        texto.color = color;
        texto.raycastTarget = false;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        return go;
    }

    public void Ocultar()
    {
        if (canvasRaiz != null)
            canvasRaiz.SetActive(false);
    }

    void OnDestroy()
    {
        if (canvasRaiz != null)
            Destroy(canvasRaiz);
    }
}
