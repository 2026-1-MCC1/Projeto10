using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePanel : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI moneyImpactText;
    [SerializeField] private TextMeshProUGUI wellBeingText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Botões")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button passButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;

    [Header("Referências")]
    [SerializeField] private PlayerStats playerStats;

    private Tile currentTile;
    private Action onDecisionMade;

    /// <summary>
    /// Indica se o painel de compra esta visivel.
    /// </summary>
    public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

    /// <summary>
    /// Prepara o painel e conecta os eventos dos botoes.
    /// </summary>
    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (passButton != null)
        {
            passButton.onClick.RemoveAllListeners();
            passButton.onClick.AddListener(OnPassClicked);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Exibe o painel de compra com todas as informacoes da propriedade.
    /// </summary>
    public void ShowPurchase(Tile tile, PlayerStats stats, Action callback)
    {
        if (tile == null || tile.Data == null || stats == null)
        {
            Debug.LogWarning("PurchasePanel recebeu dados incompletos para exibir a compra.", this);
            callback?.Invoke();
            return;
        }

        currentTile = tile;
        playerStats = stats;
        onDecisionMade = callback;

        SetMainContentVisible(true);

        if (nameText != null)
        {
            nameText.text = tile.Data.Name;
        }

        if (typeText != null)
        {
            typeText.text = FormatTileType(tile.Data.Type);
        }

        if (priceText != null)
        {
            priceText.text = $"Preço: ${tile.Data.purchasePrice}";
        }

        if (descriptionText != null)
        {
            descriptionText.text = tile.Data.propertyDescription;
        }

        UpdateImpactTexts(tile.Data);

        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackText.text = string.Empty;
        }

        bool hasEnoughMoney = stats.HasEnoughMoney(tile.Data.purchasePrice);

        if (buyButton != null)
        {
            buyButton.interactable = hasEnoughMoney;
        }

        if (buyButtonText != null)
        {
            buyButtonText.text = hasEnoughMoney ? "COMPRAR" : "SEM DINHEIRO";
        }

        if (!hasEnoughMoney && feedbackText != null)
        {
            feedbackText.text = "Dinheiro insuficiente!";
            feedbackText.gameObject.SetActive(true);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Exibe uma mensagem rapida de aluguel e cobra o valor do jogador.
    /// </summary>
    public void ShowRentMessage(Tile tile, PlayerStats stats, Action callback)
    {
        if (tile == null || tile.Data == null || stats == null)
        {
            callback?.Invoke();
            return;
        }

        currentTile = tile;
        playerStats = stats;
        onDecisionMade = callback;
        stats.SpendMoney(tile.Data.rentPrice);
        StartCoroutine(ShowQuickMessage("Você pagou aluguel: $" + tile.Data.rentPrice));
    }

    /// <summary>
    /// Exibe apenas uma mensagem curta no painel por alguns segundos.
    /// </summary>
    private IEnumerator ShowQuickMessage(string message)
    {
        SetMainContentVisible(false);

        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        onDecisionMade?.Invoke();
    }

    /// <summary>
    /// Processa a confirmacao da compra pelo jogador.
    /// </summary>
    private void OnBuyClicked()
    {
        if (currentTile == null || playerStats == null)
        {
            return;
        }

        currentTile.Purchase(playerStats);
        StartCoroutine(ClosePanelWithFeedback("Propriedade comprada!"));
    }

    /// <summary>
    /// Processa a escolha de passar a compra sem alterar os status.
    /// </summary>
    private void OnPassClicked()
    {
        StartCoroutine(ClosePanelWithFeedback(string.Empty));
    }

    /// <summary>
    /// Fecha o painel apos mostrar uma mensagem opcional de feedback.
    /// </summary>
    private IEnumerator ClosePanelWithFeedback(string message)
    {
        if (!string.IsNullOrEmpty(message) && feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.8f);
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        onDecisionMade?.Invoke();
    }

    /// <summary>
    /// Atualiza os textos de impacto com formatacao e cores apropriadas.
    /// </summary>
    private void UpdateImpactTexts(TileData data)
    {
        if (moneyImpactText != null)
        {
            bool isPositive = data.moneyImpact >= 0;
            moneyImpactText.text = isPositive
                ? $"Dinheiro: +${data.moneyImpact}/rodada"
                : $"Dinheiro: -${Mathf.Abs(data.moneyImpact)}";
            moneyImpactText.color = isPositive ? Color.green : Color.red;
        }

        if (wellBeingText != null)
        {
            bool isPositive = data.wellBeingImpact >= 0;
            wellBeingText.text = isPositive
                ? $"Bem-estar: +{data.wellBeingImpact}"
                : $"Bem-estar: {data.wellBeingImpact}";
            wellBeingText.color = isPositive ? new Color(0.5f, 0.85f, 1f) : new Color(1f, 0.65f, 0.2f);
        }

        if (pollutionText != null)
        {
            bool increasesPollution = data.pollutionImpact >= 0;
            pollutionText.text = increasesPollution
                ? $"Poluição: +{data.pollutionImpact}"
                : $"Poluição: {data.pollutionImpact}";
            pollutionText.color = increasesPollution ? new Color(0.65f, 0.72f, 0.72f) : Color.green;
        }
    }

    /// <summary>
    /// Mostra ou oculta os elementos principais do painel sem esconder o feedback.
    /// </summary>
    private void SetMainContentVisible(bool visible)
    {
        SetTextVisible(nameText, visible);
        SetTextVisible(typeText, visible);
        SetTextVisible(priceText, visible);
        SetTextVisible(descriptionText, visible);
        SetTextVisible(moneyImpactText, visible);
        SetTextVisible(wellBeingText, visible);
        SetTextVisible(pollutionText, visible);

        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(visible);
        }

        if (passButton != null)
        {
            passButton.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Ativa ou desativa um texto quando a referencia estiver configurada.
    /// </summary>
    private void SetTextVisible(TextMeshProUGUI targetText, bool visible)
    {
        if (targetText != null)
        {
            targetText.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Converte o tipo interno da casa para uma legenda amigavel no painel.
    /// </summary>
    private string FormatTileType(TileType type)
    {
        switch (type)
        {
            case TileType.Factory:
                return "Fábrica";
            case TileType.Park:
                return "Parque";
            case TileType.Residential:
                return "Residência";
            case TileType.Shopping:
                return "Comércio";
            case TileType.TreatmentPlant:
                return "Tratamento";
            case TileType.School:
                return "Escola";
            case TileType.Hospital:
                return "Hospital";
            case TileType.SolarPlant:
                return "Usina Solar";
            case TileType.FoodCourt:
                return "Praça de Alimentação";
            case TileType.Start:
                return "Início";
            default:
                return "Especial";
        }
    }
}

