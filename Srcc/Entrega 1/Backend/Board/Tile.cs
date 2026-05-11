using UnityEngine;

public class Tile : MonoBehaviour
{
    public int TileIndex { get; private set; }
    public TileData Data { get; private set; }
    public bool isPurchased = false;
    public PlayerStats owner = null;

    public bool IsPurchased => isPurchased;

    /// <summary>
    /// Inicializa a casa com seus dados e indice no tabuleiro.
    /// </summary>
    public void Initialize(TileData data, int index)
    {
        Data = data;
        TileIndex = index;
        isPurchased = false;
        owner = null;

        gameObject.name = $"Tile_{index:D2}_{data.Name}";
    }

    /// <summary>
    /// Marca a propriedade como comprada e aplica os custos e impactos ao comprador.
    /// </summary>
    public void Purchase(PlayerStats buyer)
    {
        if (buyer == null || Data == null)
        {
            return;
        }

        isPurchased = true;
        owner = buyer;
        buyer.SpendMoney(Data.purchasePrice);
        buyer.ApplyImpacts(Data.moneyImpact, Data.wellBeingImpact, Data.pollutionImpact);
    }

    /// <summary>
    /// Indica se a casa atual pode ser comprada pelo jogador.
    /// </summary>
    public bool CanBePurchased()
    {
        if (Data == null)
        {
            return false;
        }

        return !isPurchased &&
               Data.purchasePrice > 0 &&
               Data.Type != TileType.Start &&
               Data.Type != TileType.Empty;
    }

    /// <summary>
    /// Mantem um ponto de extensao para efeitos futuros da casa.
    /// </summary>
    public void ApplyEffectPlaceholder()
    {
        // Este metodo sera expandido nas proximas etapas para aplicar
        // os efeitos da casa ao jogador ou ao estado global da cidade.
    }
}
