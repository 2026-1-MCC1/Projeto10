using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalResultScreen : MonoBehaviour
{
    [Header("Estrutura")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image backgroundOverlay;
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image accentBar;
    [SerializeField] private Image resultImage;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI classificationText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI statsSummaryText;

    [Header("Imagens por Final")]
    [SerializeField] private Sprite gestorExemplarSprite;
    [SerializeField] private Sprite bomGestorSprite;
    [SerializeField] private Sprite gestaoDesequilibradaSprite;
    [SerializeField] private Sprite magnataSemEscrupulosSprite;
    [SerializeField] private Sprite cidadeEmColapsoSprite;

    private bool runtimeBuilt;

    /// <summary>
    /// Monta a tela final em runtime e a mantem escondida no inicio.
    /// </summary>
    private void Awake()
    {
        BuildRuntimeLayout();
        HideImmediate();
    }

    /// <summary>
    /// Exibe a tela final com a classificacao e os dados do jogador.
    /// </summary>
    public void Show(FinalEvaluationResult result, PlayerStats stats)
    {
        BuildRuntimeLayout();

        if (result == null || panelRoot == null)
        {
            return;
        }

        titleText.text = result.Title;
        subtitleText.text = result.Subtitle;
        classificationText.text = $"Classificacao: {result.Title}";
        classificationText.color = result.AccentColor;
        messageText.text = result.Message;
        statsSummaryText.text = EndingEvaluator.BuildStatsSummary(stats);

        accentBar.color = result.AccentColor;

        Sprite sprite = ResolveSprite(result.EndingType);
        resultImage.sprite = sprite;
        resultImage.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.18f);

        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Esconde imediatamente a tela final.
    /// </summary>
    public void HideImmediate()
    {
        BuildRuntimeLayout();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Informa se a tela final esta visivel.
    /// </summary>
    public bool IsVisible()
    {
        return panelRoot != null && panelRoot.activeSelf;
    }

    /// <summary>
    /// Resolve a imagem associada a cada final.
    /// </summary>
    private Sprite ResolveSprite(CityEndingType endingType)
    {
        switch (endingType)
        {
            case CityEndingType.GestorExemplar:
                return gestorExemplarSprite;
            case CityEndingType.BomGestor:
                return bomGestorSprite;
            case CityEndingType.MagnataSemEscrupulos:
                return magnataSemEscrupulosSprite;
            case CityEndingType.CidadeEmColapso:
                return cidadeEmColapsoSprite;
            default:
                return gestaoDesequilibradaSprite;
        }
    }

    /// <summary>
    /// Reconstrui completamente o layout da tela final para evitar sobras visuais antigas.
    /// </summary>
    private void BuildRuntimeLayout()
    {
        if (runtimeBuilt && panelRoot != null)
        {
            return;
        }

        ClearChildren();
        ResetReferences();

        RectTransform hostRect = GetOrCreateRectTransform(gameObject);
        StretchFull(hostRect);

        backgroundOverlay = CreateImage("FinalOverlay", transform, new Color(0f, 0f, 0f, 0.95f));
        StretchFull(backgroundOverlay.rectTransform);

        cardBackground = CreateImage("FinalCard", backgroundOverlay.transform, new Color(0.09f, 0.12f, 0.17f, 0.72f));
        SetStretch(cardBackground.rectTransform, 0f, 0f, 0f, 0f);

        accentBar = CreateImage("AccentBar", cardBackground.transform, new Color(0.30f, 0.69f, 0.31f, 1f));
        SetTopBar(accentBar.rectTransform, 10f);

        Image imageFrame = CreateImage("ImageFrame", cardBackground.transform, new Color(0.05f, 0.07f, 0.10f, 0.95f));
        SetAnchoredBlock(imageFrame.rectTransform, 0.06f, 0.17f, 0.40f, 0.52f);

        resultImage = CreateImage("ResultImage", imageFrame.transform, new Color(1f, 1f, 1f, 0.18f));
        SetStretch(resultImage.rectTransform, 18f, 18f, 18f, 18f);
        resultImage.preserveAspect = true;

        Image statsPanel = CreateImage("StatsPanel", cardBackground.transform, new Color(0.05f, 0.07f, 0.10f, 0.95f));
        SetAnchoredBlock(statsPanel.rectTransform, 0.06f, 0.73f, 0.40f, 0.16f);

        titleText = CreateText("TitleText", cardBackground.transform, 40f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetAnchoredBlock(titleText.rectTransform, 0.52f, 0.13f, 0.39f, 0.10f);

        subtitleText = CreateText("SubtitleText", cardBackground.transform, 19f, FontStyles.Italic, TextAlignmentOptions.TopLeft);
        SetAnchoredBlock(subtitleText.rectTransform, 0.52f, 0.22f, 0.39f, 0.05f);

        classificationText = CreateText("ClassificationText", cardBackground.transform, 22f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetAnchoredBlock(classificationText.rectTransform, 0.52f, 0.30f, 0.39f, 0.05f);

        messageText = CreateText("MessageText", cardBackground.transform, 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        SetAnchoredBlock(messageText.rectTransform, 0.52f, 0.40f, 0.39f, 0.24f);

        statsSummaryText = CreateText("StatsSummaryText", statsPanel.transform, 20f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetStretch(statsSummaryText.rectTransform, 18f, 18f, 16f, 16f);

        panelRoot = backgroundOverlay.gameObject;
        runtimeBuilt = true;
    }

    /// <summary>
    /// Remove todos os filhos atuais do objeto para recriar a tela com layout limpo.
    /// </summary>
    private void ClearChildren()
    {
        for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            Transform child = transform.GetChild(childIndex);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Zera as referencias para que o layout seja recriado do zero.
    /// </summary>
    private void ResetReferences()
    {
        panelRoot = null;
        backgroundOverlay = null;
        cardBackground = null;
        accentBar = null;
        resultImage = null;
        titleText = null;
        subtitleText = null;
        classificationText = null;
        messageText = null;
        statsSummaryText = null;
    }

    /// <summary>
    /// Cria um bloco de imagem da interface.
    /// </summary>
    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    /// <summary>
    /// Cria um texto TMP com estilo base.
    /// </summary>
    private TextMeshProUGUI CreateText(string objectName, Transform parent, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.color = Color.white;
        return text;
    }

    /// <summary>
    /// Garante um RectTransform no objeto desejado.
    /// </summary>
    private RectTransform GetOrCreateRectTransform(GameObject target)
    {
        RectTransform rectTransform = target.GetComponent<RectTransform>();

        if (rectTransform == null)
        {
            rectTransform = target.AddComponent<RectTransform>();
        }

        return rectTransform;
    }

    /// <summary>
    /// Estica um elemento para preencher totalmente seu pai.
    /// </summary>
    private void StretchFull(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Estica um bloco com margens fixas internas.
    /// </summary>
    private void SetStretch(RectTransform rectTransform, float left, float right, float top, float bottom)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(-right, -top);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Cria a barra de destaque no topo do card.
    /// </summary>
    private void SetTopBar(RectTransform rectTransform, float height)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.offsetMin = new Vector2(0f, -height);
        rectTransform.offsetMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.localScale = Vector3.one;
    }

    /// <summary>
    /// Posiciona um bloco usando percentuais do card, deixando o layout responsivo.
    /// </summary>
    private void SetAnchoredBlock(RectTransform rectTransform, float x, float y, float width, float height)
    {
        rectTransform.anchorMin = new Vector2(x, 1f - y - height);
        rectTransform.anchorMax = new Vector2(x + width, 1f - y);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;
    }
}
