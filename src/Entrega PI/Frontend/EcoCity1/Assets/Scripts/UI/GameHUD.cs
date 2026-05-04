using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI wellbeingText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI actionMessageText;
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

    /// <summary>
    /// Inscreve a interface nos eventos do dado e prepara a barra de poder.
    /// </summary>
    private void OnEnable()
    {
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
            powerLabel.text = "SEGURE ESPACO PARA CARREGAR";
        }

        if (instructionLabel != null)
        {
            instructionLabel.text = "SEGURE [ESPACO] PARA CARREGAR";
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
    /// Atualiza a barra de poder conforme o tempo de charge do dado.
    /// </summary>
    private void UpdatePowerBar(float power)
    {
        if (powerChargePanel == null || powerSlider == null)
        {
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
                instructionLabel.text = "SEGURE [ESPACO] PARA CARREGAR";
            }
            else if (power < 0.4f)
            {
                instructionLabel.text = "CARREGANDO...";
            }
            else if (power < 0.75f)
            {
                instructionLabel.text = "BOA FORCA!";
            }
            else
            {
                instructionLabel.text = "FORCA MAXIMA!";
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
}
