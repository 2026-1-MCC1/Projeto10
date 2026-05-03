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

    public bool CanAfford(int price)
    {
        return Money >= price;
    }

    public void SpendMoney(int amount)
    {
        Money = Mathf.Clamp(Money - amount, 0, int.MaxValue);
    }

    public void ApplyTileImpact(TileData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Nao foi possivel aplicar impacto porque TileData esta nulo.", this);
            return;
        }

        Money = Mathf.Clamp(Money + data.FinanceImpact, 0, int.MaxValue);
        WellBeing = Mathf.Clamp(WellBeing + data.WellBeingImpact, 0, 100);
        Pollution = Mathf.Clamp(Pollution + data.PollutionImpact, 0, 100);
    }

    public void PrintStats()
    {
        Debug.Log($"Status atuais -> Money: {Money} | WellBeing: {WellBeing} | Pollution: {Pollution}");
    }
}
