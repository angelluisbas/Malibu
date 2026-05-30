using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BienvenidaNivel1 : MonoBehaviour
{
    const string RutaImagen = "Assets/Imagenes y audios/Nivel 1.png";

    [Header("Imagen Nivel 1")]
    public Sprite imagenNivel1;
    public Vector2 tamanoImagen = new Vector2(520f, 300f);
    public Vector2 posicionImagen = new Vector2(0f, -40f);

    [Header("Tiempo en pantalla")]
    public float segundosVisible = 8f;

    GameObject canvasRaiz;

    void Awake()
    {
        ResolverImagen();
    }

    void ResolverImagen()
    {
        if (imagenNivel1 != null) return;

#if UNITY_EDITOR
        imagenNivel1 = AssetDatabase.LoadAssetAtPath<Sprite>(RutaImagen);
#endif
        if (imagenNivel1 == null)
        {
            Debug.LogWarning(
                "BienvenidaNivel1: arrastra Nivel 1.png en Imagen Nivel 1 " +
                $"(ruta: {RutaImagen}).",
                this);
        }
    }

    void Start()
    {
        ResolverImagen();
        if (imagenNivel1 == null) return;

        CrearInterfaz();
        if (segundosVisible > 0f)
            Invoke(nameof(Ocultar), segundosVisible);
    }

    void CrearInterfaz()
    {
        canvasRaiz = new GameObject("CanvasBienvenidaNivel1", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasRaiz.transform.SetParent(transform, false);

        var canvas = canvasRaiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var scaler = canvasRaiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var go = new GameObject("ImagenNivel1", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvasRaiz.transform, false);

        var imagen = go.GetComponent<Image>();
        imagen.sprite = imagenNivel1;
        imagen.preserveAspect = true;
        imagen.raycastTarget = false;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = posicionImagen;
        rect.sizeDelta = tamanoImagen;
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
