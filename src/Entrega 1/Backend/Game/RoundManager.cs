using System;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private int maxRounds = 12;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private bool canRoll = true;
    [SerializeField] private bool gameOver = false;

    public int CurrentRound => currentRound;
    public int MaxRounds => maxRounds;
    public bool CanRoll => canRoll && !gameOver;
    public bool IsGameOver => gameOver;

    public event Action<int> OnRoundChanged;
    public event Action OnGameOver;

    /// <summary>
    /// Define se o jogador pode rolar o dado no momento.
    /// </summary>
    public void SetCanRoll(bool value)
    {
        canRoll = value;
    }

    /// <summary>
    /// Registra uma jogada e avanca a rodada atual.
    /// </summary>
    public void RegisterRoll()
    {
        if (gameOver)
        {
            return;
        }

        currentRound++;
        OnRoundChanged?.Invoke(currentRound);

        if (currentRound >= maxRounds)
        {
            gameOver = true;
            OnGameOver?.Invoke();
        }
    }

    /// <summary>
    /// Restaura o controle de rodadas para o estado inicial.
    /// </summary>
    public void ResetRounds()
    {
        currentRound = 0;
        canRoll = true;
        gameOver = false;
    }
}
