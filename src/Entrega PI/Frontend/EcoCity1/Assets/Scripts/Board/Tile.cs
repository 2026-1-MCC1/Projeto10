using UnityEngine;

public class Tile : MonoBehaviour
{
    public int TileIndex { get; private set; }
    public TileData Data { get; private set; }
    public bool IsPurchased { get; private set; }

    public void Initialize(TileData data, int index)
    {
        Data = data;
        TileIndex = index;
        IsPurchased = false;

        gameObject.name = $"Tile_{index:D2}_{data.Name}";
    }

    public void Purchase()
    {
        IsPurchased = true;
    }

    public void ApplyEffectPlaceholder()
    {
        // Este metodo sera expandido nas proximas etapas para aplicar
        // os efeitos da casa ao jogador ou ao estado global da cidade.
    }
}
