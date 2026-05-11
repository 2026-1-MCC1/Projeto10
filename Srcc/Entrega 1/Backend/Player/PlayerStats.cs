using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private const int InitialMoney = 500;
    private const int InitialWellBeing = 50;
    private const int InitialPollution = 20;

    public int Money { get; private set; }
    public int WellBeing { get; private set; }
    public int Pollution { get; private set; }

    private void Awake()
    {
        Money = InitialMoney;
        WellBeing = InitialWellBeing;
        Pollution = InitialPollution;
    }

    /// <summary>
    /// Verifica se o jogador pode pagar um determinado valor.
    /// </summary>
    public bool CanAfford(int price)
    {
        return Money >= price;
    }

    /// <summary>
    /// Verifica se o jogador possui dinheiro suficiente para uma compra.
    /// </summary>
    public bool HasEnoughMoney(int amount)
    {
        return Money >= amount;
    }

    /// <summary>
    /// Desconta dinheiro do jogador sem permitir valores negativos.
    /// </summary>
    public void SpendMoney(int amount)
    {
        Money = Mathf.Clamp(Money - amount, 0, int.MaxValue);
    }

    /// <summary>
    /// Aplica impactos diretos nos atributos do jogador.
    /// </summary>
    public void ApplyImpacts(int money, int wellBeing, int pollution)
    {
        Money = Mathf.Clamp(Money + money, 0, int.MaxValue);
        WellBeing = Mathf.Clamp(WellBeing + wellBeing, 0, 100);
        Pollution = Mathf.Clamp(Pollution + pollution, 0, 100);
    }

    /// <summary>
    /// Aplica os impactos configurados em uma casa do tabuleiro.
    /// </summary>
    public void ApplyTileImpact(TileData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Nao foi possivel aplicar impacto porque TileData esta nulo.", this);
            return;
        }

        ApplyImpacts(data.FinanceImpact, data.WellBeingImpact, data.PollutionImpact);
    }

    /// <summary>
    /// Exibe os status atuais do jogador no console para debug.
    /// </summary>
    public void PrintStats()
    {
        Debug.Log($"Status atuais -> Dinheiro: {Money} | Bem-estar: {WellBeing} | Poluição: {Pollution}");
    }
}
