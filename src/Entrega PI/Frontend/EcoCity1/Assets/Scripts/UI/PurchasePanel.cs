using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button skipButton;

    private Action<bool> currentDecisionCallback;

    private void Awake()
    {
        Hide();
    }

    public void Show(Tile tile, PlayerStats stats, Action<bool> onDecision)
    {
        if (tile == null || tile.Data == null || stats == null)
        {
            Debug.LogWarning("PurchasePanel recebeu dados incompletos para exibir a compra.", this);
            return;
        }

        currentDecisionCallback = onDecision;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = tile.Data.Name;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                $"Preco: {tile.Data.Price}\n" +
                $"Impacto Financeiro: {tile.Data.FinanceImpact}\n" +
                $"Impacto Bem-estar: {tile.Data.WellBeingImpact}\n" +
                $"Impacto Poluicao: {tile.Data.PollutionImpact}\n" +
                $"Seu dinheiro atual: {stats.Money}";
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = stats.CanAfford(tile.Data.Price);
            buyButton.onClick.AddListener(() => ResolveDecision(true));
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => ResolveDecision(false));
        }
    }

    public void Hide()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
        }

        currentDecisionCallback = null;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void ResolveDecision(bool didBuy)
    {
        Action<bool> callback = currentDecisionCallback;
        Hide();
        callback?.Invoke(didBuy);
    }
}
