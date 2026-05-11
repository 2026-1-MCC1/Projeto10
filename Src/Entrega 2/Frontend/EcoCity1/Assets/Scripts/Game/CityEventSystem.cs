using UnityEngine;

public enum CityEventTone
{
    Positive,
    Warning,
    Neutral
}

public sealed class CityEventResult
{
    public bool Triggered { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public int MoneyDelta { get; private set; }
    public int WellBeingDelta { get; private set; }
    public int PollutionDelta { get; private set; }
    public CityEventTone Tone { get; private set; }

    public CityEventResult(bool triggered, string title, string message, int moneyDelta, int wellBeingDelta, int pollutionDelta, CityEventTone tone)
    {
        Triggered = triggered;
        Title = title;
        Message = message;
        MoneyDelta = moneyDelta;
        WellBeingDelta = wellBeingDelta;
        PollutionDelta = pollutionDelta;
        Tone = tone;
    }
}

public class CityEventSystem : MonoBehaviour
{
    [Header("Evento da Casa Inicial")]
    [SerializeField] private int startBonusMoney = 70;
    [SerializeField] private int startBonusWellBeing = 4;
    [SerializeField] private int startBonusPollution = -2;

    /// <summary>
    /// Aplica um bonus especial ao passar pela casa inicial.
    /// </summary>
    public CityEventResult ApplyStartBonus(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            return CreateNone();
        }

        playerStats.ApplyImpacts(startBonusMoney, startBonusWellBeing, startBonusPollution);

        return new CityEventResult(
            true,
            "Repasse Municipal",
            $"A prefeitura recebeu um novo repasse. +${startBonusMoney}, bem-estar +{startBonusWellBeing} e poluição {startBonusPollution}.",
            startBonusMoney,
            startBonusWellBeing,
            startBonusPollution,
            CityEventTone.Positive);
    }

    /// <summary>
    /// Dispara eventos de cidade em rodadas especificas para variar a partida.
    /// </summary>
    public CityEventResult TryApplyRoundEvent(int roundNumber, PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            return CreateNone();
        }

        switch (roundNumber)
        {
            case 3:
                return ApplyEvent(
                    playerStats,
                    "Mutirão de Limpeza",
                    "Moradores e voluntários organizaram uma limpeza urbana que melhorou a cidade.",
                    -10,
                    8,
                    -12,
                    CityEventTone.Positive);

            case 6:
                return ApplyEvent(
                    playerStats,
                    "Incentivo Verde",
                    "Um programa estadual liberou verba para energia limpa e melhorias sustentáveis.",
                    90,
                    4,
                    -8,
                    CityEventTone.Positive);

            case 9:
                return ApplyEvent(
                    playerStats,
                    "Festival da Cidade",
                    "O festival trouxe alegria e movimento econômico, mas deixou impactos temporários na limpeza urbana.",
                    35,
                    12,
                    6,
                    CityEventTone.Neutral);

            default:
                if (playerStats.Pollution >= 70)
                {
                    return ApplyEvent(
                        playerStats,
                        "Crise de Poluição",
                        "A cidade entrou em alerta ambiental e foi preciso gastar recursos emergenciais.",
                        -45,
                        -10,
                        4,
                        CityEventTone.Warning);
                }

                if (playerStats.WellBeing <= 35)
                {
                    return ApplyEvent(
                        playerStats,
                        "Mobilização Comunitária",
                        "A comunidade se uniu para recuperar espaços urbanos e apoiar a população.",
                        -20,
                        12,
                        -4,
                        CityEventTone.Positive);
                }

                return CreateNone();
        }
    }

    /// <summary>
    /// Formata um resumo curto do evento para HUD e logs.
    /// </summary>
    public string BuildEventSummary(CityEventResult result)
    {
        if (result == null || !result.Triggered)
        {
            return string.Empty;
        }

        return
            $"{result.Title}: " +
            $"{FormatSignedMoney(result.MoneyDelta)} | " +
            $"Bem-estar {FormatSignedValue(result.WellBeingDelta)} | " +
            $"Poluição {FormatSignedValue(result.PollutionDelta)}";
    }

    /// <summary>
    /// Aplica um evento especifico ao jogador e devolve o resultado consolidado.
    /// </summary>
    private CityEventResult ApplyEvent(
        PlayerStats playerStats,
        string title,
        string message,
        int moneyDelta,
        int wellBeingDelta,
        int pollutionDelta,
        CityEventTone tone)
    {
        playerStats.ApplyImpacts(moneyDelta, wellBeingDelta, pollutionDelta);

        return new CityEventResult(
            true,
            title,
            message,
            moneyDelta,
            wellBeingDelta,
            pollutionDelta,
            tone);
    }

    /// <summary>
    /// Cria um resultado vazio quando nenhum evento deve acontecer.
    /// </summary>
    private CityEventResult CreateNone()
    {
        return new CityEventResult(false, string.Empty, string.Empty, 0, 0, 0, CityEventTone.Neutral);
    }

    /// <summary>
    /// Formata uma variacao financeira com sinal.
    /// </summary>
    private string FormatSignedMoney(int value)
    {
        return value >= 0 ? $"+${value}" : $"-${Mathf.Abs(value)}";
    }

    /// <summary>
    /// Formata qualquer valor inteiro com sinal.
    /// </summary>
    private string FormatSignedValue(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }
}
