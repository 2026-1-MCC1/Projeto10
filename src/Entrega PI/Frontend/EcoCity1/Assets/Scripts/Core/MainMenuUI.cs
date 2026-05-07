using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Paineis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Imagem de Fundo")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color backgroundTint = new Color(1f, 1f, 1f, 0.92f);

    [Header("Cores")]
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.08f, 0.12f, 1f);
    [SerializeField] private Color cardColor = new Color(0.09f, 0.14f, 0.20f, 0.92f);
    [SerializeField] private Color accentColor = new Color(0.30f, 0.78f, 0.46f, 1f);
    [SerializeField] private Color secondaryAccent = new Color(0.22f, 0.52f, 0.86f, 1f);
    [SerializeField] private Color textColor = Color.white;

    private Canvas rootCanvas;
    private TextMeshProUGUI heroTitleText;
    private TextMeshProUGUI heroSubtitleText;

    /// <summary>
    /// Organiza as referencias e aplica o polimento visual do menu.
    /// </summary>
    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ResolveReferences();
        ConfigureCanvas();
        WireButtons();
        BuildVisualLayout();
        ShowMainPanel();
    }

    /// <summary>
    /// Inicia o jogo carregando a cena principal.
    /// </summary>
    public void PlayGame()
    {
        string sceneName = SceneExists("GameScene") ? "GameScene" : "Game_Scene";
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Exibe o painel de opcoes.
    /// </summary>
    public void OpenOptions()
    {
        SetPanelState(false, true, false);
    }

    /// <summary>
    /// Exibe o painel de creditos.
    /// </summary>
    public void OpenCredits()
    {
        SetPanelState(false, false, true);
    }

    /// <summary>
    /// Retorna ao painel principal do menu.
    /// </summary>
    public void BackToMain()
    {
        ShowMainPanel();
    }

    /// <summary>
    /// Encerra o jogo ou para o Play Mode no editor.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Fechando o jogo...");
    }

    /// <summary>
    /// Exibe o painel principal e esconde os secundarios.
    /// </summary>
    private void ShowMainPanel()
    {
        SetPanelState(true, false, false);
    }

    /// <summary>
    /// Ajusta o estado ativo de cada painel.
    /// </summary>
    private void SetPanelState(bool showMain, bool showOptions, bool showCredits)
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(showMain);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(showOptions);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(showCredits);
        }

        UpdateHeroVisibility(showMain);
    }

    /// <summary>
    /// Localiza as referencias principais da cena caso nao estejam preenchidas.
    /// </summary>
    private void ResolveReferences()
    {
        rootCanvas = FindObjectOfType<Canvas>();

        if (mainPanel == null)
        {
            mainPanel = FindByName("MainPanel");
        }

        if (optionsPanel == null)
        {
            optionsPanel = FindByName("OptionsPanel");
        }

        if (creditsPanel == null)
        {
            creditsPanel = FindByName("CreditsPanel");
        }
    }

    /// <summary>
    /// Configura a Canvas para escalar corretamente em telas diferentes.
    /// </summary>
    private void ConfigureCanvas()
    {
        if (rootCanvas == null)
        {
            return;
        }

        CanvasScaler scaler = rootCanvas.GetComponent<CanvasScaler>();

        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    /// <summary>
    /// Garante que todos os botoes relevantes chamem os metodos corretos.
    /// </summary>
    private void WireButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            string lowerName = button.gameObject.name.ToLowerInvariant();
            button.onClick.RemoveAllListeners();

            if (lowerName.Contains("play"))
            {
                button.onClick.AddListener(PlayGame);
                SetButtonLabel(button, "INICIAR JOGO");
            }
            else if (lowerName.Contains("option"))
            {
                button.onClick.AddListener(OpenOptions);
                SetButtonLabel(button, "OPÇÕES");
            }
            else if (lowerName.Contains("credit"))
            {
                button.onClick.AddListener(OpenCredits);
                SetButtonLabel(button, "CRÉDITOS");
            }
            else if (lowerName.Contains("exit") || lowerName.Contains("quit"))
            {
                button.onClick.AddListener(QuitGame);
                SetButtonLabel(button, "SAIR");
            }
            else if (lowerName.Contains("back"))
            {
                button.onClick.AddListener(BackToMain);
                SetButtonLabel(button, "VOLTAR");
            }

            StyleButton(button, lowerName.Contains("play") ? accentColor : secondaryAccent);
        }
    }

    /// <summary>
    /// Monta um layout mais bonito aproveitando os paineis existentes.
    /// </summary>
    private void BuildVisualLayout()
    {
        if (rootCanvas == null)
        {
            return;
        }

        EnsureBackground();
        BuildHeroText();
        StylePanel(mainPanel, string.Empty, string.Empty);
        StylePanel(optionsPanel, "OPÇÕES", "Controles:\n- ESPAÇO para carregar e rolar o dado\n- F1 a F5 para testar finais no editor\n\nDica:\nBusque equilíbrio entre dinheiro, bem-estar e poluição.");
        StylePanel(creditsPanel, "CRÉDITOS", "Projeto: Eco City\nDesenvolvimento:\nArthur Lima De Luiz\nLeonardo Batista França\nBrian Walter\nMotor: Unity\n\nObrigado por jogar e testar nossa cidade sustentável.");
    }

    /// <summary>
    /// Cria um fundo escuro e elegante para o menu.
    /// </summary>
    private void EnsureBackground()
    {
        Image existingBackground = FindChildImage(rootCanvas.transform, "MenuBackground");

        if (existingBackground != null)
        {
            ApplyBackgroundStyle(existingBackground);
            return;
        }

        GameObject backgroundObject = new GameObject("MenuBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(rootCanvas.transform, false);
        backgroundObject.transform.SetAsFirstSibling();
        Image background = backgroundObject.GetComponent<Image>();
        ApplyBackgroundStyle(background);

        RectTransform rectTransform = background.GetComponent<RectTransform>();
        Stretch(rectTransform);
    }

    /// <summary>
    /// Estiliza um painel existente e garante titulo, subtitulo e area interna limpa.
    /// </summary>
    private void StylePanel(GameObject panelObject, string title, string bodyText)
    {
        if (panelObject == null)
        {
            return;
        }

        CleanupLegacyPanelContent(panelObject.transform);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            bool isMainPanel = panelObject == mainPanel;
            bool isCreditsPanel = panelObject == creditsPanel;

            rectTransform.anchorMin = isMainPanel ? new Vector2(0.03f, 0.06f) : new Vector2(0.33f, 0.18f);
            rectTransform.anchorMax = isMainPanel ? new Vector2(0.30f, 0.66f) : new Vector2(0.67f, 0.82f);
            rectTransform.pivot = isMainPanel ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        Image image = panelObject.GetComponent<Image>();

        if (image == null)
        {
            image = panelObject.AddComponent<Image>();
        }

        image.color = cardColor;
        image.type = Image.Type.Sliced;

        CreateOrUpdateAccentBar(panelObject.transform);
        CreateOrUpdateHeader(panelObject.transform, title);
        CreateOrUpdateBodyText(panelObject.transform, bodyText);
        ArrangeButtons(panelObject.transform);
    }

    /// <summary>
    /// Cria ou atualiza a barra superior de destaque.
    /// </summary>
    private void CreateOrUpdateAccentBar(Transform panelTransform)
    {
        Image accent = FindChildImage(panelTransform, "AccentBar");

        if (accent == null)
        {
            GameObject accentObject = new GameObject("AccentBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentObject.transform.SetParent(panelTransform, false);
            accent = accentObject.GetComponent<Image>();
        }

        accent.color = accentColor;
        RectTransform rectTransform = accent.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = new Vector2(0f, 12f);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Cria ou atualiza o cabecalho de um painel.
    /// </summary>
    private void CreateOrUpdateHeader(Transform panelTransform, string title)
    {
        TextMeshProUGUI titleText = FindOrCreateText(panelTransform, "PanelTitle");
        titleText.text = title;
        titleText.fontSize = 48f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rectTransform = titleText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.10f, 0.74f);
        rectTransform.anchorMax = new Vector2(0.90f, 0.90f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
    }

    /// <summary>
    /// Cria ou atualiza o texto central do painel.
    /// </summary>
    private void CreateOrUpdateBodyText(Transform panelTransform, string bodyText)
    {
        TextMeshProUGUI body = FindOrCreateText(panelTransform, "PanelBody");
        body.text = bodyText;
        body.fontSize = 22f;
        body.fontStyle = FontStyles.Normal;
        body.color = new Color(0.88f, 0.92f, 0.96f, 1f);
        body.alignment = TextAlignmentOptions.TopLeft;
        body.enableWordWrapping = true;
        body.overflowMode = TextOverflowModes.Overflow;

        RectTransform rectTransform = body.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.10f, 0.38f);
        rectTransform.anchorMax = new Vector2(0.90f, 0.68f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        body.gameObject.SetActive(!string.IsNullOrWhiteSpace(bodyText));
    }

    /// <summary>
    /// Reposiciona os botoes do painel de forma limpa e padronizada.
    /// </summary>
    private void ArrangeButtons(Transform panelTransform)
    {
        Button[] buttons = panelTransform.GetComponentsInChildren<Button>(true);

        for (int index = 0; index < buttons.Length; index++)
        {
            Button button = buttons[index];
            RectTransform rectTransform = button.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.10f, 0.08f + (buttons.Length - 1 - index) * 0.12f);
            rectTransform.anchorMax = new Vector2(0.64f, 0.17f + (buttons.Length - 1 - index) * 0.12f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);

            if (label != null)
            {
                label.fontSize = 24f;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Cria um titulo principal sobre a imagem de fundo para dar mais identidade ao menu.
    /// </summary>
    private void BuildHeroText()
    {
        if (rootCanvas == null)
        {
            return;
        }

        heroTitleText = FindOrCreateCanvasText("MenuHeroTitle");
        heroTitleText.text = "ECO CITY";
        heroTitleText.fontSize = 92f;
        heroTitleText.fontStyle = FontStyles.Bold;
        heroTitleText.color = Color.white;
        heroTitleText.alignment = TextAlignmentOptions.TopLeft;
        heroTitleText.enableWordWrapping = false;

        RectTransform titleRect = heroTitleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.77f);
        titleRect.anchorMax = new Vector2(0.50f, 0.93f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        heroSubtitleText = FindOrCreateCanvasText("MenuHeroSubtitle");
        heroSubtitleText.text = "Planeje uma cidade onde economia, bem-estar\ne sustentabilidade avancem juntas.";
        heroSubtitleText.fontSize = 28f;
        heroSubtitleText.fontStyle = FontStyles.Italic;
        heroSubtitleText.color = new Color(0.90f, 0.95f, 0.98f, 0.95f);
        heroSubtitleText.alignment = TextAlignmentOptions.TopLeft;
        heroSubtitleText.enableWordWrapping = true;

        RectTransform subtitleRect = heroSubtitleText.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.055f, 0.63f);
        subtitleRect.anchorMax = new Vector2(0.42f, 0.76f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Mostra ou oculta o título principal conforme o painel atual do menu.
    /// </summary>
    private void UpdateHeroVisibility(bool visible)
    {
        if (heroTitleText != null)
        {
            heroTitleText.gameObject.SetActive(visible);
        }

        if (heroSubtitleText != null)
        {
            heroSubtitleText.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Remove textos antigos que estejam poluindo o painel, preservando botoes e os blocos novos.
    /// </summary>
    private void CleanupLegacyPanelContent(Transform panelTransform)
    {
        TextMeshProUGUI[] texts = panelTransform.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI text in texts)
        {
            if (text == null)
            {
                continue;
            }

            string objectName = text.gameObject.name;

            if (objectName == "PanelTitle" || objectName == "PanelBody")
            {
                continue;
            }

            if (text.GetComponentInParent<Button>(true) != null)
            {
                continue;
            }

            text.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Aplica um estilo visual mais forte ao botao.
    /// </summary>
    private void StyleButton(Button button, Color baseColor)
    {
        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            image.color = baseColor;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = baseColor;
        colors.highlightedColor = baseColor * 1.08f;
        colors.pressedColor = baseColor * 0.90f;
        colors.selectedColor = baseColor;
        colors.disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.45f);
        button.colors = colors;
    }

    /// <summary>
    /// Aplica cor ou sprite ao fundo principal do menu.
    /// </summary>
    private void ApplyBackgroundStyle(Image background)
    {
        if (background == null)
        {
            return;
        }

        if (backgroundSprite != null)
        {
            background.sprite = backgroundSprite;
            background.color = backgroundTint;
            background.type = Image.Type.Simple;
            background.preserveAspect = false;
        }
        else
        {
            background.sprite = null;
            background.color = backgroundColor;
            background.type = Image.Type.Simple;
        }
    }

    /// <summary>
    /// Atualiza o texto interno de um botao.
    /// </summary>
    private void SetButtonLabel(Button button, string label)
    {
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (text != null)
        {
            text.text = label;
        }
    }

    /// <summary>
    /// Procura um objeto pelo nome dentro da cena.
    /// </summary>
    private GameObject FindByName(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform currentTransform in transforms)
        {
            if (currentTransform != null && currentTransform.name == objectName && currentTransform.hideFlags == HideFlags.None)
            {
                return currentTransform.gameObject;
            }
        }

        return null;
    }

    /// <summary>
    /// Procura ou cria um texto TMP dentro do painel.
    /// </summary>
    private TextMeshProUGUI FindOrCreateText(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);

        if (child != null)
        {
            TextMeshProUGUI existing = child.GetComponent<TextMeshProUGUI>();

            if (existing != null)
            {
                return existing;
            }
        }

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        return text;
    }

    /// <summary>
    /// Procura ou cria um texto TMP diretamente na Canvas principal.
    /// </summary>
    private TextMeshProUGUI FindOrCreateCanvasText(string objectName)
    {
        Transform child = rootCanvas.transform.Find(objectName);

        if (child != null)
        {
            TextMeshProUGUI existing = child.GetComponent<TextMeshProUGUI>();

            if (existing != null)
            {
                return existing;
            }
        }

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(rootCanvas.transform, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Procura uma imagem filha pelo nome.
    /// </summary>
    private Image FindChildImage(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    /// <summary>
    /// Estica um RectTransform para preencher o pai.
    /// </summary>
    private void Stretch(RectTransform rectTransform)
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
    /// Verifica se a cena existe nas Build Settings.
    /// </summary>
    private bool SceneExists(string sceneName)
    {
        for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(index);

            if (System.IO.Path.GetFileNameWithoutExtension(scenePath) == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}
