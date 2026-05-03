using TMPro;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI wellbeingText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI actionMessageText;

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

    public void UpdateDiceResult(int diceValue)
    {
        if (diceResultText != null)
        {
            diceResultText.text = $"Dado: {diceValue}";
        }
    }

    public void ClearDiceResult()
    {
        if (diceResultText != null)
        {
            diceResultText.text = string.Empty;
        }
    }

    public void ShowActionMessage(string message)
    {
        if (actionMessageText != null)
        {
            actionMessageText.text = message;
        }
    }

    public void ClearActionMessage()
    {
        if (actionMessageText != null)
        {
            actionMessageText.text = string.Empty;
        }
    }
}
