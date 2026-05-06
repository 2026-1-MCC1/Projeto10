using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameHUD : MonoBehaviour
{
    [Header("Raizes da HUD")]
    [SerializeField] private GameObject hudPanelRoot;
    [SerializeField] private GameObject topLeftStatsRoot;
    [SerializeField] private GameObject centerMessagePanelRoot;
    [SerializeField] private GameObject purchasePanelRoot;
    [SerializeField] private GameObject tutorialPanelRoot;
    [SerializeField] private GameObject eventToastRoot;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI wellbeingText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI actionMessageText;
    [SerializeField] private TextMeshProUGUI eventToastText;
    [Header("Power Charge UI")]
    [SerializeField] private GameObject powerChargePanel;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private TextMeshProUGUI powerLabel;
    [SerializeField] private PhysicalDice dice;
    [Header("Cores do Power Charge")]
    [SerializeField] private Image powerFillImage;
    [SerializeField] private Color colorWeak = new Color(0.31f, 0.76f, 0.97f);
    [SerializeField] private Color colorMedium = new Color(1f, 0.84f, 0.31f);
    [SerializeField] private Color colorStrong = new Color(0.94f, 0.33f, 0.31f);
    [Header("Texto de Instrucao")]
    [SerializeField] private TextMeshProUGUI instructionLabel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    private bool warnedMissingPowerReferences;
    private Coroutine temporaryMessageRoutine;
    private Coroutine tutorialRoutine;

    /// <summary>
    /// Tenta conectar automaticamente os elementos principais da interface.
    /// </summary>
    private void Awake()
    {
        ResolveUIReferences();
    }

    /// <summary>
    /// Inscreve a interface nos eventos do dado e prepara a barra de poder.
    /// </summary>
    private void OnEnable()
    {
        ResolveUIReferences();
        ResolveDiceReference();

        if (dice != null)
        {
            dice.OnChargePowerChanged -= UpdatePowerBar;
            dice.OnChargePowerChanged += UpdatePowerBar;
        }

        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
        }

        if (powerLabel != null && string.IsNullOrWhiteSpace(powerLabel.text))
        {
            powerLabel.text = "POWER CHARGE";
        }

        if (instructionLabel != null)
        {
            instructionLabel.text = "SEGURE [ESPAÇO] PARA CARREGAR";
        }

        if (powerFillImage != null)
        {
            powerFillImage.color = colorWeak;
        }

        if (powerChargePanel != null)
        {
            powerChargePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Atualiza a interface de charge mesmo quando alguma referencia foi perdida no Inspector.
    /// </summary>
    private void Update()
    {
        ResolveUIReferences();
        ResolveDiceReference();

        if (dice == null || powerChargePanel == null || powerSlider == null)
        {
            return;
        }

        if (dice.IsWaitingForCharge || powerChargePanel.activeSelf)
        {
            UpdatePowerBar(dice.CurrentChargePower);
        }
    }

    /// <summary>
    /// Remove inscricoes em eventos quando a interface e desativada.
    /// </summary>
    private void OnDisable()
    {
        if (dice != null)
        {
            dice.OnChargePowerChanged -= UpdatePowerBar;
        }
    }

    /// <summary>
    /// Atualiza os valores exibidos na interface do jogador.
    /// </summary>
    public void UpdateStats(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        if (moneyText != null)
        {
            moneyText.text = $"Money: {stats.Money}";
        }

        if (wellbeingText != null)
        {
            wellbeingText.text = $"WellBeing: {stats.WellBeing}";
        }

        if (pollutionText != null)
        {
            pollutionText.text = $"Pollution: {stats.Pollution}";
        }

        if (scoreText != null)
        {
            float score = ScoreCalculator.CalculateFinalScore(stats);
            scoreText.text = $"Score: {score:0}";
        }
    }

    /// <summary>
    /// Atualiza o indicador de rodada atual na interface.
    /// </summary>
    public void UpdateRoundInfo(int currentRound, int maxRounds)
    {
        EnsureRoundText();

        if (roundText != null)
        {
            roundText.text = $"Rodada: {currentRound}/{maxRounds}";
        }
    }

    /// <summary>
    /// Mostra ou oculta os elementos principais da HUD durante telas de transicao.
    /// </summary>
    public void SetGameplayUIVisible(bool visible)
    {
        ResolveUIReferences();

        SetRootVisible(hudPanelRoot, visible);
        SetRootVisible(topLeftStatsRoot, visible);
        SetRootVisible(centerMessagePanelRoot, visible);
        SetRootVisible(purchasePanelRoot, visible);

        if (roundText != null)
        {
            roundText.gameObject.SetActive(visible);
        }

        if (powerChargePanel != null)
        {
            powerChargePanel.SetActive(visible && powerSlider != null && powerSlider.value > 0f);
        }

        if (tutorialPanelRoot != null && !visible)
        {
            tutorialPanelRoot.SetActive(false);
        }

        if (eventToastRoot != null && !visible)
        {
            eventToastRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Exibe o valor numerico do dado na interface.
    /// </summary>
    public void UpdateDiceResult(int diceValue)
    {
        if (diceResultText != null)
        {
            diceResultText.text = $"Dado: {diceValue}";
        }
    }

    /// <summary>
    /// Limpa o resultado anterior do dado na interface.
    /// </summary>
    public void ClearDiceResult()
    {
        if (diceResultText != null)
        {
            diceResultText.text = string.Empty;
        }
    }

    /// <summary>
    /// Exibe uma mensagem de acao para orientar o jogador.
    /// </summary>
    public void ShowActionMessage(string message)
    {
        if (actionMessageText != null)
        {
            actionMessageText.text = message;
        }
    }

    /// <summary>
    /// Exibe uma mensagem temporaria e depois restaura o painel central.
    /// </summary>
    public void ShowTemporaryActionMessage(string message, float duration)
    {
        EnsureEventToast();

        if (eventToastRoot == null || eventToastText == null)
        {
            ShowActionMessage(message);
            return;
        }

        if (temporaryMessageRoutine != null)
        {
            StopCoroutine(temporaryMessageRoutine);
        }

        temporaryMessageRoutine = StartCoroutine(ShowTemporaryActionMessageRoutine(message, duration));
    }

    /// <summary>
    /// Limpa a mensagem de acao exibida na interface.
    /// </summary>
    public void ClearActionMessage()
    {
        if (actionMessageText != null)
        {
            actionMessageText.text = string.Empty;
        }
    }

    /// <summary>
    /// Exibe uma sequencia curta de tutorial no inicio da partida.
    /// </summary>
    public void ShowTutorialSequence()
    {
        ResolveUIReferences();
        EnsureTutorialPanel();

        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
        }

        tutorialRoutine = StartCoroutine(ShowTutorialSequenceRoutine());
    }

    /// <summary>
    /// Atualiza a barra de poder conforme o tempo de charge do dado.
    /// </summary>
    private void UpdatePowerBar(float power)
    {
        if (powerChargePanel == null || powerSlider == null)
        {
            WarnMissingPowerReferences();
            return;
        }

        if (powerFillImage != null)
        {
            Color barColor;

            if (power < 0.5f)
            {
                barColor = Color.Lerp(colorWeak, colorMedium, power * 2f);
            }
            else
            {
                barColor = Color.Lerp(colorMedium, colorStrong, (power - 0.5f) * 2f);
            }

            powerFillImage.color = barColor;
        }

        if (instructionLabel != null)
        {
            if (power == 0f)
            {
                instructionLabel.text = "SEGURE [ESPAÇO] PARA CARREGAR";
            }
            else if (power < 0.4f)
            {
                instructionLabel.text = "CARREGANDO...";
            }
            else if (power < 0.75f)
            {
                instructionLabel.text = "BOA FORÇA!";
            }
            else
            {
                instructionLabel.text = "FORÇA MÁXIMA!";
            }
        }

        if (power > 0f)
        {
            powerChargePanel.SetActive(true);
            powerSlider.value = power;
            return;
        }

        powerSlider.value = 0f;
        powerChargePanel.SetActive(false);
    }

    /// <summary>
    /// Procura automaticamente a referencia do dado quando necessario.
    /// </summary>
    private void ResolveDiceReference()
    {
        if (dice == null)
        {
            dice = FindObjectOfType<PhysicalDice>();
        }
    }

    /// <summary>
    /// Procura automaticamente os objetos da HUD pelos nomes mais usados na cena.
    /// </summary>
    private void ResolveUIReferences()
    {
        Transform searchRoot = GetUISearchRoot();

        if (powerChargePanel == null)
        {
            Transform panelTransform = FindChildRecursive(searchRoot, "PowerChargePanel");

            if (panelTransform != null)
            {
                powerChargePanel = panelTransform.gameObject;
            }
        }

        if (powerSlider == null)
        {
            Transform sliderTransform = FindChildRecursive(searchRoot, "PowerSlider");

            if (sliderTransform != null)
            {
                powerSlider = sliderTransform.GetComponent<Slider>();
            }
        }

        if (powerLabel == null)
        {
            Transform labelTransform = FindChildRecursive(searchRoot, "PowerLabel");

            if (labelTransform != null)
            {
                powerLabel = labelTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (instructionLabel == null)
        {
            Transform instructionTransform = FindChildRecursive(searchRoot, "InstructionLabel");

            if (instructionTransform != null)
            {
                instructionLabel = instructionTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        if (powerFillImage == null && powerSlider != null)
        {
            Transform fillTransform = powerSlider.transform.Find("Fill Area/Fill");

            if (fillTransform != null)
            {
                powerFillImage = fillTransform.GetComponent<Image>();
            }
        }

        if (moneyText == null)
        {
            moneyText = ResolveTextByName("MoneyText");
        }

        if (hudPanelRoot == null)
        {
            Transform hudPanelTransform = FindChildRecursive(searchRoot, "HUDPanel");

            if (hudPanelTransform != null)
            {
                hudPanelRoot = hudPanelTransform.gameObject;
            }
        }

        if (topLeftStatsRoot == null)
        {
            Transform topLeftTransform = FindChildRecursive(searchRoot, "TopLeftStats");

            if (topLeftTransform != null)
            {
                topLeftStatsRoot = topLeftTransform.gameObject;
            }
        }

        if (centerMessagePanelRoot == null)
        {
            Transform messagePanelTransform = FindChildRecursive(searchRoot, "CenterMessagePanel");

            if (messagePanelTransform != null)
            {
                centerMessagePanelRoot = messagePanelTransform.gameObject;
            }
        }

        if (purchasePanelRoot == null)
        {
            Transform purchasePanelTransform = FindChildRecursive(searchRoot, "PurchasePanelRoot");

            if (purchasePanelTransform != null)
            {
                purchasePanelRoot = purchasePanelTransform.gameObject;
            }
        }

        if (eventToastRoot == null)
        {
            Transform eventToastTransform = FindChildRecursive(searchRoot, "EventToastPanel");

            if (eventToastTransform != null)
            {
                eventToastRoot = eventToastTransform.gameObject;
            }
        }

        if (tutorialPanelRoot == null)
        {
            Transform tutorialTransform = FindChildRecursive(searchRoot, "TutorialPanel");

            if (tutorialTransform != null)
            {
                tutorialPanelRoot = tutorialTransform.gameObject;
            }
        }

        if (wellbeingText == null)
        {
            wellbeingText = ResolveTextByName("WellbeingText");
        }

        if (pollutionText == null)
        {
            pollutionText = ResolveTextByName("PollutionText");
        }

        if (scoreText == null)
        {
            scoreText = ResolveTextByName("ScoreText");
        }

        if (roundText == null)
        {
            roundText = ResolveTextByName("RoundText");
        }

        if (diceResultText == null)
        {
            diceResultText = ResolveTextByName("DiceResultText");
        }

        if (actionMessageText == null)
        {
            actionMessageText = ResolveTextByName("ActionMessageText");
        }

        if (tutorialText == null)
        {
            tutorialText = ResolveTextByName("TutorialText");
        }

        if (eventToastText == null)
        {
            eventToastText = ResolveTextByName("EventToastText");
        }
    }

    /// <summary>
    /// Exibe um aviso unico quando faltam referencias da barra de power charge.
    /// </summary>
    private void WarnMissingPowerReferences()
    {
        if (warnedMissingPowerReferences)
        {
            return;
        }

        warnedMissingPowerReferences = true;
        Debug.LogWarning("GameHUD ainda esta sem referencias completas do Power Charge. O script esta tentando resolver automaticamente pela hierarquia.", this);
    }

    /// <summary>
    /// Procura um texto TMP pelo nome dentro da hierarquia da HUD.
    /// </summary>
    private TextMeshProUGUI ResolveTextByName(string objectName)
    {
        Transform foundTransform = FindChildRecursive(GetUISearchRoot(), objectName);
        return foundTransform != null ? foundTransform.GetComponent<TextMeshProUGUI>() : null;
    }

    /// <summary>
    /// Define a raiz correta de busca da interface, priorizando a Canvas inteira.
    /// </summary>
    private Transform GetUISearchRoot()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null)
        {
            return parentCanvas.transform;
        }

        return transform.root;
    }

    /// <summary>
    /// Procura recursivamente um filho pelo nome dentro da interface.
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int index = 0; index < parent.childCount; index++)
        {
            Transform child = parent.GetChild(index);
            Transform result = FindChildRecursive(child, childName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Garante a criacao de um texto de rodada no canto superior direito quando ele nao existir na cena.
    /// </summary>
    private void EnsureRoundText()
    {
        if (roundText != null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            return;
        }

        GameObject roundObject = new GameObject("RoundText", typeof(RectTransform), typeof(TextMeshProUGUI));
        roundObject.transform.SetParent(parentCanvas.transform, false);
        roundText = roundObject.GetComponent<TextMeshProUGUI>();
        roundText.fontSize = 26f;
        roundText.fontStyle = FontStyles.Bold;
        roundText.color = Color.white;
        roundText.alignment = TextAlignmentOptions.Center;
        roundText.text = "Rodada: 0/12";

        RectTransform rectTransform = roundObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-22f, -22f);
        rectTransform.sizeDelta = new Vector2(220f, 42f);
    }

    /// <summary>
    /// Cria um painel simples de tutorial quando ele ainda nao existe na cena.
    /// </summary>
    private void EnsureTutorialPanel()
    {
        if (tutorialPanelRoot != null && tutorialText != null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            return;
        }

        GameObject panelObject = new GameObject("TutorialPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parentCanvas.transform, false);
        tutorialPanelRoot = panelObject;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.05f, 0.07f, 0.10f, 0.88f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 160f);
        panelRect.anchoredPosition = new Vector2(0f, 250f);

        GameObject textObject = new GameObject("TutorialText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        tutorialText = textObject.GetComponent<TextMeshProUGUI>();
        tutorialText.fontSize = 26f;
        tutorialText.fontStyle = FontStyles.Bold;
        tutorialText.alignment = TextAlignmentOptions.Center;
        tutorialText.enableWordWrapping = true;
        tutorialText.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 18f);
        textRect.offsetMax = new Vector2(-24f, -18f);

        tutorialPanelRoot.SetActive(false);
    }

    /// <summary>
    /// Exibe uma mensagem curta por alguns segundos no painel central da HUD.
    /// </summary>
    private IEnumerator ShowTemporaryActionMessageRoutine(string message, float duration)
    {
        eventToastRoot.SetActive(true);
        eventToastText.text = message;
        yield return new WaitForSeconds(duration);
        eventToastText.text = string.Empty;
        eventToastRoot.SetActive(false);
        temporaryMessageRoutine = null;
    }

    /// <summary>
    /// Mostra instrucoes iniciais para orientar o jogador nas primeiras acoes.
    /// </summary>
    private IEnumerator ShowTutorialSequenceRoutine()
    {
        if (tutorialPanelRoot == null || tutorialText == null)
        {
            yield break;
        }

        tutorialPanelRoot.SetActive(true);
        tutorialText.text =
            "Segure ESPAÇO para carregar o dado.\n" +
            "Compre propriedades para desenvolver a cidade.\n" +
            "Equilibre dinheiro, bem-estar e poluição.";

        yield return new WaitForSeconds(6f);

        tutorialPanelRoot.SetActive(false);
        tutorialRoutine = null;
    }

    /// <summary>
    /// Cria um pequeno painel de resumo para eventos, renda e feedbacks curtos.
    /// </summary>
    private void EnsureEventToast()
    {
        if (eventToastRoot != null && eventToastText != null)
        {
            return;
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas == null)
        {
            return;
        }

        GameObject panelObject = new GameObject("EventToastPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parentCanvas.transform, false);
        eventToastRoot = panelObject;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.06f, 0.09f, 0.12f, 0.92f);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 18f);
        panelRect.sizeDelta = new Vector2(780f, 78f);

        GameObject textObject = new GameObject("EventToastText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        eventToastText = textObject.GetComponent<TextMeshProUGUI>();
        eventToastText.fontSize = 24f;
        eventToastText.fontStyle = FontStyles.Bold;
        eventToastText.alignment = TextAlignmentOptions.Center;
        eventToastText.enableWordWrapping = true;
        eventToastText.color = Color.white;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 12f);
        textRect.offsetMax = new Vector2(-20f, -12f);

        eventToastRoot.SetActive(false);
    }

    /// <summary>
    /// Mostra ou oculta o proprio objeto ou o pai mais imediato do elemento da interface.
    /// </summary>
    private void SetRootVisible(GameObject targetRoot, bool visible)
    {
        if (targetRoot == null)
        {
            return;
        }

        targetRoot.SetActive(visible);
    }
}
